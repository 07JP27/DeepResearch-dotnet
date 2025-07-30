using DeepResearch.Core.Models;
using DeepResearch.Core.JsonSchema;
using DeepResearch.Core.SearchClient;
using Microsoft.Extensions.AI;

namespace DeepResearch.Core;

public class DeepResearchService(
    IChatClient aiChatClient,
    ISearchClient searchClient)
{
    private static readonly ChatOptions _reflectionOnSummaryOptions = new()
    {
        ResponseFormat = ChatResponseFormat.ForJsonSchema(AIJsonUtilities.CreateJsonSchema(typeof(ReflectionOnSummaryResponse))),
    };

    private static readonly ChatOptions _generateQueryOptions = new()
    {
        ResponseFormat = ChatResponseFormat.ForJsonSchema(AIJsonUtilities.CreateJsonSchema(typeof(GenerateQueryResponse))),
    };

    public async Task<ResearchResult> RunResearchAsync(string topic, DeepResearchOptions? researchOptions = null, IProgress<ProgressBase>? progress = null, CancellationToken cancellationToken = default)
    {
        var asyncProgress = AsyncProgress<ProgressBase>.FromProgress(progress);
        return await RunResearchInternalAsync(topic, researchOptions, asyncProgress, cancellationToken);
    }

    public async Task<ResearchResult> RunResearchAsync(string topic, DeepResearchOptions? researchOptions, IAsyncProgress<ProgressBase> asyncProgress, CancellationToken cancellationToken = default)
    {
        return await RunResearchInternalAsync(topic, researchOptions, asyncProgress, cancellationToken);
    }

    public async Task<ResearchResult> RunResearchAsync(string topic, IAsyncProgress<ProgressBase> asyncProgress, CancellationToken cancellationToken = default)
    {
        return await RunResearchInternalAsync(topic, null, asyncProgress, cancellationToken);
    }

    private async Task<ResearchResult> RunResearchInternalAsync(string topic, DeepResearchOptions? researchOptions, IAsyncProgress<ProgressBase> asyncProgress, CancellationToken cancellationToken)
    {
        researchOptions ??= new();
        var state = new ResearchState { ResearchTopic = topic };

        await GenerateQueryAsync(state, asyncProgress, cancellationToken);

        while (state.ResearchLoopCount < researchOptions.MaxResearchLoops)
        {
            await ReportProgressAsync(new RoutingProgress
            {
                Decision = RoutingDecision.Continue,
                LoopCount = state.ResearchLoopCount
            }, asyncProgress, cancellationToken);

            await WebResearchAsync(state, researchOptions, asyncProgress, cancellationToken);
            await SummarizeSourcesAsync(state, asyncProgress, cancellationToken);
            await ReflectOnSummaryAsync(state, asyncProgress, cancellationToken);
            state.ResearchLoopCount++;
        }

        await ReportProgressAsync(new RoutingProgress
        {
            Decision = RoutingDecision.Finalize,
            LoopCount = state.ResearchLoopCount
        }, asyncProgress, cancellationToken);

        await FinalizeSummaryAsync(state, researchOptions, asyncProgress, cancellationToken);

        await ReportProgressAsync(new ResearchCompleteProgress
        {
            FinalSummary = state.RunningSummary,
            Sources = state.SourcesGathered,
            Images = state.Images
        }, asyncProgress, cancellationToken);

        return new ResearchResult
        {
            ResearchTopic = state.ResearchTopic,
            Summary = state.RunningSummary,
            Sources = state.SourcesGathered,
            Images = state.Images
        };
    }

    private async ValueTask ReportProgressAsync(ProgressBase progressValue, IAsyncProgress<ProgressBase> asyncProgress, CancellationToken cancellationToken)
    {
        await asyncProgress.ReportAsync(progressValue, cancellationToken);
    }

    private async Task GenerateQueryAsync(ResearchState state, IAsyncProgress<ProgressBase> asyncProgress, CancellationToken cancellationToken, bool isRetry = false)
    {
        var prompt = string.Format(Prompts.QueryWriterInstructions, DateTime.Now.ToString("MMMM dd, yyyy"), state.ResearchTopic);

        if (isRetry && state.QueryGenerationMessages.Count > 0)
        {
            // Add retry message to existing conversation
            state.QueryGenerationMessages.Add(new(ChatRole.User, "The previous query returned no results. Please generate a different search query."));
        }
        else
        {
            // Start new conversation
            state.QueryGenerationMessages.Clear();
            state.QueryGenerationMessages.Add(new(ChatRole.System, prompt));
            state.QueryGenerationMessages.Add(new(ChatRole.User, "Generate a query for web search:"));
        }

        var result = await aiChatClient.GetResponseAsync<GenerateQueryResponse>(state.QueryGenerationMessages, _generateQueryOptions);
        if (result.FinishReason != ChatFinishReason.Stop)
        {
            throw new InvalidOperationException($"AI chat completion failed with finish reason: {result.FinishReason}");
        }

        if (!result.TryGetResult(out var generateQueryResponse))
        {
            // JSONパースに失敗した場合のフォールバック処理
            throw new InvalidOperationException($"Failed to parse AI response to GenerateQueryResponse. Raw response: {result.Text}");
        }
        // Add assistant response to conversation history
        state.QueryGenerationMessages.AddRange(result.Messages);

        state.SearchQuery = generateQueryResponse.Query;
        state.QueryRationale = generateQueryResponse.Rationale;

        await ReportProgressAsync(new QueryGenerationProgress
        {
            Query = state.SearchQuery,
            Rationale = state.QueryRationale
        }, asyncProgress, cancellationToken);
    }

    private async Task WebResearchAsync(ResearchState state, DeepResearchOptions researchOptions, IAsyncProgress<ProgressBase> asyncProgress, CancellationToken cancellationToken)
    {
        var searchResult = await searchClient.SearchAsync(
            query: state.SearchQuery,
            maxResults: researchOptions.MaxSourceCountPerSearch,
            cancellationToken: cancellationToken);

        // Check if no results and retry limit not exceeded
        if ((searchResult.Results == null || searchResult.Results.Count == 0) &&
            state.QueryRetryCount < researchOptions.MaxSearchRetryAttempts)
        {
            await ReportProgressAsync(new RoutingProgress
            {
                Decision = RoutingDecision.RetrySearch,
                LoopCount = state.QueryRetryCount
            }, asyncProgress, cancellationToken);

            state.QueryRetryCount++;

            // Decide whether to use GenerateQueryAsync or ReflectOnSummaryAsync
            if (string.IsNullOrEmpty(state.RunningSummary))
            {
                // No summary yet, regenerate initial query with retry message
                await GenerateQueryAsync(state, asyncProgress, cancellationToken, isRetry: true);
            }
            else
            {
                // Have summary, use reflection to generate new query with retry message
                await ReflectOnSummaryAsync(state, asyncProgress, cancellationToken, isRetry: true);
            }

            // Recursively call WebResearchAsync with new query
            await WebResearchAsync(state, researchOptions, asyncProgress, cancellationToken);
        }

        // Reset retry count on successful search or when max retries exceeded
        state.QueryRetryCount = 0;

        if (searchResult.Images is { Count: > 0 })
        {
            state.Images.AddRange(searchResult.Images);
        }

        // Add deduplicated and cleaned sources to SourcesGathered
        var newSources = Formatting.DeduplicateAndCleanSources(searchResult.Results ?? [], state.SourcesGathered);
        state.SourcesGathered.AddRange(newSources);

        state.WebResearchResults.Add(Formatting.DeduplicateAndFormatSources(searchResult, researchOptions.MaxCharacterPerSource));

        await ReportProgressAsync(new WebResearchProgress
        {
            Sources = searchResult.Results ?? [],
            Images = searchResult.Images ?? [],
        }, asyncProgress, cancellationToken);
    }

    private async Task SummarizeSourcesAsync(ResearchState state, IAsyncProgress<ProgressBase> asyncProgress, CancellationToken cancellationToken)
    {
        var mostRecent = state.WebResearchResults.Count > 0 ? state.WebResearchResults[^1] : "";
        string humanMessage;
        if (!string.IsNullOrEmpty(state.RunningSummary))
        {
            humanMessage = $"<Existing Summary>\n{state.RunningSummary}\n</Existing Summary>\n\n<New Context>\n{mostRecent}\n</New Context>Update the Existing Summary with the New Context on this topic:\n<User Input>\n{state.ResearchTopic}\n</User Input>\n\n";
        }
        else
        {
            humanMessage = $"<Context>\n{mostRecent}\n</Context>Create a Summary using the Context on this topic:\n<User Input>\n{state.ResearchTopic}\n</User Input>\n\n";
        }

        var result = await aiChatClient.GetResponseAsync([
            new (ChatRole.System, Prompts.SummarizerInstructions),
            new (ChatRole.User, humanMessage),
        ]);
        state.RunningSummary = result.Text.Trim();
        state.SummariesGathered.Add(state.RunningSummary);

        await ReportProgressAsync(new SummarizeProgress
        {
            Summary = state.RunningSummary
        }, asyncProgress, cancellationToken);
    }

    private async Task ReflectOnSummaryAsync(ResearchState state, IAsyncProgress<ProgressBase> asyncProgress, CancellationToken cancellationToken, bool isRetry = false)
    {
        var prompt = string.Format(Prompts.ReflectionInstructions, state.ResearchTopic);
        var baseMessage = $"Reflect on our existing knowledge: \n===\n{state.RunningSummary},\n===\nAnd now identify a knowledge gap and generate a follow-up web search query:";

        if (isRetry && state.ReflectionMessages.Count > 0)
        {
            // Add retry message to existing conversation
            state.ReflectionMessages.Add(new(ChatRole.User, "The previous query returned no results. Please generate a different search query."));
        }
        else
        {
            // Start new conversation
            state.ReflectionMessages.Clear();
            state.ReflectionMessages.Add(new(ChatRole.System, prompt));
            state.ReflectionMessages.Add(new(ChatRole.User, baseMessage));
        }

        var result = await aiChatClient.GetResponseAsync<ReflectionOnSummaryResponse>(state.ReflectionMessages, _reflectionOnSummaryOptions);
        if (result.FinishReason != ChatFinishReason.Stop)
        {
            throw new InvalidOperationException($"AI chat completion failed with finish reason: {result.FinishReason}");
        }

        if (!result.TryGetResult(out var reflectionResponse))
        {
            // JSONパースに失敗した場合のフォールバック処理
            throw new InvalidOperationException($"Failed to parse AI response to ReflectionOnSummaryResponse. Raw response: {result.Text}");
        }

        // Add assistant response to conversation history
        state.ReflectionMessages.AddRange(result.Messages);

        state.SearchQuery = reflectionResponse.FollowUpQuery;
        state.KnowledgeGap = reflectionResponse.KnowledgeGap;

        await ReportProgressAsync(new ReflectionProgress
        {
            Query = state.SearchQuery,
            KnowledgeGap = state.KnowledgeGap
        }, asyncProgress, cancellationToken);
    }

    private async Task FinalizeSummaryAsync(ResearchState state, DeepResearchOptions researchOptions, IAsyncProgress<ProgressBase> asyncProgress, CancellationToken cancellationToken)
    {
        await ReportProgressAsync(new FinalizeProgress(), asyncProgress, cancellationToken);

        if (researchOptions.EnableSummaryConsolidation)
        {
            var prompt = Prompts.FinalizeInstructions(state.SummariesGathered);
            List<ChatMessage> messages = [
                new (ChatRole.System, prompt),
                new (ChatRole.User, $"<TOPIC>{state.ResearchTopic} </TOPIC>")
            ];

            var result = await aiChatClient.GetResponseAsync(messages);
            state.RunningSummary = result.Text.Trim();
        }
    }
}
