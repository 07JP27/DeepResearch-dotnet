using DeepResearch.SearchClient;
using DeepResearch.Core.Models;
using DeepResearch.Core.JsonSchema;
using OpenAI.Chat;
using System.Text.Json;

namespace DeepResearch.Core;

public class DeepResearchService
{
    private readonly ChatClient _aiChatClient;
    private readonly ISearchClient _searchClient;
    private readonly DeepResearchOptions _researchOptions;
    private IProgress<ProgressBase>? _progress;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly ChatCompletionOptions _reflectionOnSummaryOptions = new()
    {
        ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat("ReflectionResponse", JsonSchemaGenerator.GenerateSchemaAsBinaryData(SourceGenerationContext.Default.ReflectionOnSummaryResponse)),
    };

    private static readonly ChatCompletionOptions _generateQueryOptions = new()
    {
        ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat("AnalysisAgentResponse", JsonSchemaGenerator.GenerateSchemaAsBinaryData(SourceGenerationContext.Default.GenerateQueryResponse)),
    };

    public DeepResearchService(
        ChatClient aiChatClient,
        ISearchClient searchClient,
        DeepResearchOptions? options = null)
    {
        _aiChatClient = aiChatClient;
        _searchClient = searchClient;

        options ??= new DeepResearchOptions();
        _researchOptions = options;
    }

    public async Task<ResearchResult> RunResearchAsync(string topic, IProgress<ProgressBase>? progress = null, CancellationToken cancellationToken = default)
    {
        _progress = progress;
        var state = new ResearchState { ResearchTopic = topic };

        await GenerateQueryAsync(state, cancellationToken);

        while (state.ResearchLoopCount < _researchOptions.MaxResearchLoops)
        {
            NotifyProgress(new RoutingProgress
            {
                Decision = RoutingDecision.Continue,
                LoopCount = state.ResearchLoopCount
            });

            await WebResearchAsync(state, cancellationToken);
            await SummarizeSourcesAsync(state, cancellationToken);
            await ReflectOnSummaryAsync(state, cancellationToken);
            state.ResearchLoopCount++;
        }

        NotifyProgress(new RoutingProgress
        {
            Decision = RoutingDecision.Finalize,
            LoopCount = state.ResearchLoopCount
        });

        await FinalizeSummaryAsync(state, cancellationToken);

        NotifyProgress(new ResearchCompleteProgress
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

    private async Task GenerateQueryAsync(ResearchState state, CancellationToken cancellationToken = default, bool isRetry = false)
    {
        var prompt = string.Format(Prompts.QueryWriterInstructions, DateTime.Now.ToString("MMMM dd, yyyy"), state.ResearchTopic);

        if (isRetry && state.QueryGenerationMessages.Count > 0)
        {
            // Add retry message to existing conversation
            state.QueryGenerationMessages.Add(new UserChatMessage("The previous query returned no results. Please generate a different search query."));
        }
        else
        {
            // Start new conversation
            state.QueryGenerationMessages.Clear();
            state.QueryGenerationMessages.Add(new SystemChatMessage(prompt));
            state.QueryGenerationMessages.Add(new UserChatMessage("Generate a query for web search:"));
        }

        var result = await _aiChatClient.CompleteChatAsync(state.QueryGenerationMessages, _generateQueryOptions);
        if (result.Value.FinishReason != ChatFinishReason.Stop) throw new InvalidOperationException($"AI chat completion failed with finish reason: {result.Value.FinishReason}");

        var generateQueryResponse = JsonSerializer.Deserialize<GenerateQueryResponse>(result.Value.Content.First().Text, _jsonSerializerOptions);
        if (generateQueryResponse == null) throw new InvalidOperationException("Failed to deserialize GenerateQueryResponse");

        // Add assistant response to conversation history
        state.QueryGenerationMessages.Add(new AssistantChatMessage(result.Value.Content.First().Text));

        state.SearchQuery = generateQueryResponse.Query;
        state.QueryRationale = generateQueryResponse.Rationale;

        NotifyProgress(new QueryGenerationProgress
        {
            Query = state.SearchQuery,
            Rationale = state.QueryRationale
        });
    }

    private async Task WebResearchAsync(ResearchState state, CancellationToken cancellationToken = default)
    {
        var searchResult = await _searchClient.SearchAsync(
            query: state.SearchQuery,
            maxResults: _researchOptions.MaxSourceCountPerSearch,
            cancellationToken: cancellationToken);

        // Check if no results and retry limit not exceeded
        if ((searchResult.Results == null || searchResult.Results.Count == 0) &&
            state.QueryRetryCount < _researchOptions.MaxSearchRetryAttempts)
        {
            state.QueryRetryCount++;

            NotifyProgress(new RoutingProgress
            {
                Decision = RoutingDecision.RetrySearch,
                LoopCount = state.QueryRetryCount
            });

            // Decide whether to use GenerateQueryAsync or ReflectOnSummaryAsync
            if (string.IsNullOrEmpty(state.RunningSummary))
            {
                // No summary yet, regenerate initial query with retry message
                await GenerateQueryAsync(state, cancellationToken, isRetry: true);
            }
            else
            {
                // Have summary, use reflection to generate new query with retry message
                await ReflectOnSummaryAsync(state, cancellationToken, isRetry: true);
            }

            // Recursively call WebResearchAsync with new query
            await WebResearchAsync(state, cancellationToken);
            return;
        }

        // Reset retry count on successful search or when max retries exceeded
        state.QueryRetryCount = 0;

        state.Images.AddRange(searchResult.Images ?? new List<string>());

        // Add deduplicated and cleaned sources to SourcesGathered
        var newSources = Formatting.DeduplicateAndCleanSources(searchResult.Results ?? new List<SearchResultItem>(), state.SourcesGathered);
        state.SourcesGathered.AddRange(newSources);

        state.WebResearchResults.Add(Formatting.DeduplicateAndFormatSources(searchResult, _researchOptions.MaxCharacterPerSource));

        NotifyProgress(new WebResearchProgress
        {
            Sources = searchResult.Results ?? new List<SearchResultItem>(),
            Images = searchResult.Images ?? new List<string>()
        });
    }

    private async Task SummarizeSourcesAsync(ResearchState state, CancellationToken cancellationToken = default)
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
            new SystemChatMessage(Prompts.SummarizerInstructions),
            new UserChatMessage(humanMessage)
        };
        var result = await _aiChatClient.CompleteChatAsync(messages);
        state.RunningSummary = result.Value.Content.First().Text.Trim();
        state.SummariesGathered.Add(state.RunningSummary);

        NotifyProgress(new SummarizeProgress
        {
            Summary = state.RunningSummary
        });
    }

    private async Task ReflectOnSummaryAsync(ResearchState state, CancellationToken cancellationToken = default, bool isRetry = false)
    {
        var prompt = string.Format(Prompts.ReflectionInstructions, state.ResearchTopic);
        var baseMessage = $"Reflect on our existing knowledge: \n===\n{state.RunningSummary},\n===\nAnd now identify a knowledge gap and generate a follow-up web search query:";

        if (isRetry && state.ReflectionMessages.Count > 0)
        {
            // Add retry message to existing conversation
            state.ReflectionMessages.Add(new UserChatMessage("The previous query returned no results. Please generate a different search query."));
        }
        else
        {
            // Start new conversation
            state.ReflectionMessages.Clear();
            state.ReflectionMessages.Add(new SystemChatMessage(prompt));
            state.ReflectionMessages.Add(new UserChatMessage(baseMessage));
        }

        var result = await _aiChatClient.CompleteChatAsync(state.ReflectionMessages, _reflectionOnSummaryOptions);
        if (result.Value.FinishReason != ChatFinishReason.Stop) throw new InvalidOperationException($"AI chat completion failed with finish reason: {result.Value.FinishReason}");

        var reflectionResponse = JsonSerializer.Deserialize<ReflectionOnSummaryResponse>(result.Value.Content.First().Text, _jsonSerializerOptions);
        if (reflectionResponse == null) throw new InvalidOperationException("Failed to deserialize ReflectionOnSummaryResponse");

        // Add assistant response to conversation history
        state.ReflectionMessages.Add(new AssistantChatMessage(result.Value.Content.First().Text));

        state.SearchQuery = reflectionResponse.FollowUpQuery;
        state.KnowledgeGap = reflectionResponse.KnowledgeGap;

        NotifyProgress(new ReflectionProgress
        {
            Query = state.SearchQuery,
            KnowledgeGap = state.KnowledgeGap
        });
    }

    private async Task FinalizeSummaryAsync(ResearchState state, CancellationToken cancellationToken = default)
    {
        NotifyProgress(new FinalizeProgress());

        if (_researchOptions.EnableSummaryConsolidation)
        {
            var prompt = Prompts.FinalizeInstructions(state.SummariesGathered);
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(prompt),
                new UserChatMessage($"<TOPIC>{state.ResearchTopic} </TOPIC>")
            };

            var result = await _aiChatClient.CompleteChatAsync(messages);
            state.RunningSummary = result.Value.Content.First().Text.Trim();
        }
    }

    private void NotifyProgress(ProgressBase progressInfo)
    {
        _progress?.Report(progressInfo);
    }
}
