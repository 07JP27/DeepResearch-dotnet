using DeepResearch.Core.Models;
using DeepResearch.Core.JsonSchema;
using DeepResearch.Core.SearchClient;
using Microsoft.Extensions.AI;

namespace DeepResearch.Core;

public class DeepResearchService
{
    private readonly IChatClient _aiChatClient;
    private readonly ISearchClient _searchClient;

    private static readonly ChatOptions _reflectionOnSummaryOptions = new()
    {
       ResponseFormat = ChatResponseFormat.ForJsonSchema(AIJsonUtilities.CreateJsonSchema(typeof(ReflectionOnSummaryResponse))),
    };

    private static readonly ChatOptions _generateQueryOptions = new()
    {
        ResponseFormat = ChatResponseFormat.ForJsonSchema(AIJsonUtilities.CreateJsonSchema(typeof(GenerateQueryResponse))),
    };

    public DeepResearchService(
        IChatClient aiChatClient,
        ISearchClient searchClient)
    {
        _aiChatClient = aiChatClient;
        _searchClient = searchClient;
    }

    public async Task<ResearchResult> RunResearchAsync(string topic, DeepResearchOptions? researchOptions = null, IProgress<ProgressBase>? progress = null, CancellationToken cancellationToken = default)
    {
        researchOptions ??= new();
        var state = new ResearchState { ResearchTopic = topic };

        await GenerateQueryAsync(state, progress, cancellationToken);

        while (state.ResearchLoopCount < researchOptions.MaxResearchLoops)
        {
            progress?.Report(new RoutingProgress
            {
                Decision = RoutingDecision.Continue,
                LoopCount = state.ResearchLoopCount
            });

            await WebResearchAsync(state, researchOptions, progress, cancellationToken);
            await SummarizeSourcesAsync(state, progress, cancellationToken);
            await ReflectOnSummaryAsync(state, progress, cancellationToken);
            state.ResearchLoopCount++;
        }

        progress?.Report(new RoutingProgress
        {
            Decision = RoutingDecision.Finalize,
            LoopCount = state.ResearchLoopCount
        });

        await FinalizeSummaryAsync(state, researchOptions, progress, cancellationToken);

        progress?.Report(new ResearchCompleteProgress
        {
            FinalSummary = state.RunningSummary,
            Sources = state.SourcesGathered,
            Images = state.Images
        });

        return new ResearchResult
        {
            ResearchTopic = state.ResearchTopic,
            Summary = state.RunningSummary,
            Sources = state.SourcesGathered,
            Images = state.Images
        };
    }

    private async Task GenerateQueryAsync(ResearchState state, IProgress<ProgressBase>? progress = null, CancellationToken cancellationToken = default, bool isRetry = false)
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

        var result = await _aiChatClient.GetResponseAsync<GenerateQueryResponse>(state.QueryGenerationMessages, _generateQueryOptions);
        if (result.FinishReason != ChatFinishReason.Stop) throw new InvalidOperationException($"AI chat completion failed with finish reason: {result.FinishReason}");

        var generateQueryResponse = result.Result;
        // Add assistant response to conversation history
        state.QueryGenerationMessages.AddRange(result.Messages);

        state.SearchQuery = generateQueryResponse.Query;
        state.QueryRationale = generateQueryResponse.Rationale;

        progress?.Report(new QueryGenerationProgress
        {
            Query = state.SearchQuery,
            Rationale = state.QueryRationale
        });
    }

    private async Task WebResearchAsync(ResearchState state, DeepResearchOptions researchOptions, IProgress<ProgressBase>? progress = null, CancellationToken cancellationToken = default)
    {
        var searchResult = await _searchClient.SearchAsync(
            query: state.SearchQuery,
            maxResults: researchOptions.MaxSourceCountPerSearch,
            cancellationToken: cancellationToken);

        // Check if no results and retry limit not exceeded
        if ((searchResult.Results == null || searchResult.Results.Count == 0) &&
            state.QueryRetryCount < researchOptions.MaxSearchRetryAttempts)
        {
            progress?.Report(new RoutingProgress
            {
                Decision = RoutingDecision.RetrySearch,
                LoopCount = state.QueryRetryCount
            });

            state.QueryRetryCount++;

            // Decide whether to use GenerateQueryAsync or ReflectOnSummaryAsync
            if (string.IsNullOrEmpty(state.RunningSummary))
            {
                // No summary yet, regenerate initial query with retry message
                await GenerateQueryAsync(state, progress, cancellationToken, isRetry: true);
            }
            else
            {
                // Have summary, use reflection to generate new query with retry message
                await ReflectOnSummaryAsync(state, progress, cancellationToken, isRetry: true);
            }

            // Recursively call WebResearchAsync with new query
            await WebResearchAsync(state, researchOptions, progress, cancellationToken);
        }

        // Reset retry count on successful search or when max retries exceeded
        state.QueryRetryCount = 0;

        state.Images.AddRange(searchResult.Images ?? new List<string>());

        // Add deduplicated and cleaned sources to SourcesGathered
        var newSources = Formatting.DeduplicateAndCleanSources(searchResult.Results ?? new List<SearchResultItem>(), state.SourcesGathered);
        state.SourcesGathered.AddRange(newSources);

        state.WebResearchResults.Add(Formatting.DeduplicateAndFormatSources(searchResult, researchOptions.MaxCharacterPerSource));

        progress?.Report(new WebResearchProgress
        {
            Sources = searchResult.Results ?? new List<SearchResultItem>(),
            Images = searchResult.Images ?? new List<string>()
        });
    }

    private async Task SummarizeSourcesAsync(ResearchState state, IProgress<ProgressBase>? progress = null, CancellationToken cancellationToken = default)
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
        var messages = new List<ChatMessage>
        {
            new (ChatRole.System, Prompts.SummarizerInstructions),
            new (ChatRole.User, humanMessage)
        };
        var result = await _aiChatClient.GetResponseAsync(messages);
        state.RunningSummary = result.Text.Trim();
        state.SummariesGathered.Add(state.RunningSummary);

        progress?.Report(new SummarizeProgress
        {
            Summary = state.RunningSummary
        });
    }

    private async Task ReflectOnSummaryAsync(ResearchState state, IProgress<ProgressBase>? progress = null, CancellationToken cancellationToken = default, bool isRetry = false)
    {
        var prompt = string.Format(Prompts.ReflectionInstructions, state.ResearchTopic);
        var baseMessage = $"Reflect on our existing knowledge: \n===\n{state.RunningSummary},\n===\nAnd now identify a knowledge gap and generate a follow-up web search query:";

        if (isRetry && state.ReflectionMessages.Count > 0)
        {
            // Add retry message to existing conversation
            state.ReflectionMessages.Add(new (ChatRole.User, "The previous query returned no results. Please generate a different search query."));
        }
        else
        {
            // Start new conversation
            state.ReflectionMessages.Clear();
            state.ReflectionMessages.Add(new (ChatRole.System, prompt));
            state.ReflectionMessages.Add(new (ChatRole.User, baseMessage));
        }

        var result = await _aiChatClient.GetResponseAsync<ReflectionOnSummaryResponse>(state.ReflectionMessages, _reflectionOnSummaryOptions);
        if (result.FinishReason != ChatFinishReason.Stop) throw new InvalidOperationException($"AI chat completion failed with finish reason: {result.FinishReason}");

        var reflectionResponse = result.Result;

        // Add assistant response to conversation history
        state.ReflectionMessages.AddRange(result.Messages);

        state.SearchQuery = reflectionResponse.FollowUpQuery;
        state.KnowledgeGap = reflectionResponse.KnowledgeGap;

        progress?.Report(new ReflectionProgress
        {
            Query = state.SearchQuery,
            KnowledgeGap = state.KnowledgeGap
        });
    }

    private async Task FinalizeSummaryAsync(ResearchState state, DeepResearchOptions researchOptions, IProgress<ProgressBase>? progress = null, CancellationToken cancellationToken = default)
    {
        progress?.Report(new FinalizeProgress());

        if (researchOptions.EnableSummaryConsolidation)
        {
            var prompt = Prompts.FinalizeInstructions(state.SummariesGathered);
            var messages = new List<ChatMessage>
            {
                new (ChatRole.System, prompt),
                new (ChatRole.User, $"<TOPIC>{state.ResearchTopic} </TOPIC>")
            };

            var result = await _aiChatClient.GetResponseAsync(messages);
            state.RunningSummary = result.Text.Trim();
        }
    }
}
