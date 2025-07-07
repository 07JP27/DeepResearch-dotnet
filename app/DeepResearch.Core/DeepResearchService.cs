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
    private readonly int _maxLoops;
    private readonly int _maxCharacterPerSource;
    private readonly int _maxSourceCountPerSearch;

    private readonly Action<ProgressBase>? _onProgressChanged;

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
        Action<ProgressBase>? onProgressChanged = null,
        DeepResearchOptions? options = null)
    {
        _aiChatClient = aiChatClient;
        _searchClient = searchClient;
        _onProgressChanged = onProgressChanged;

        options ??= new DeepResearchOptions();
        _maxLoops = options.MaxResearchLoops;
        _maxCharacterPerSource = options.MaxCharacterPerSource;
        _maxSourceCountPerSearch = options.MaxSourceCountPerSearch;
    }

    public async Task<ResearchResult> RunResearchAsync(string topic, CancellationToken cancellationToken = default)
    {
        var state = new ResearchState { ResearchTopic = topic };

        await GenerateQueryAsync(state, cancellationToken);

        while (state.ResearchLoopCount < _maxLoops)
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

    private async Task GenerateQueryAsync(ResearchState state, CancellationToken cancellationToken = default)
    {
        var prompt = string.Format(Prompts.QueryWriterInstructions, DateTime.Now.ToString("MMMM dd, yyyy"), state.ResearchTopic);
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(prompt),
            new UserChatMessage("Generate a query for web search:")
        };

        var result = await _aiChatClient.CompleteChatAsync(messages, _generateQueryOptions);
        if (result.Value.FinishReason != ChatFinishReason.Stop) throw new InvalidOperationException($"AI chat completion failed withz finish reason: {result.Value.FinishReason}");

        var generateQueryResponse = JsonSerializer.Deserialize<GenerateQueryResponse>(result.Value.Content.First().Text, _jsonSerializerOptions);
        if (generateQueryResponse == null) throw new InvalidOperationException("Failed to deserialize GenerateQueryResponse");

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
            maxResults: _maxSourceCountPerSearch,
            cancellationToken: cancellationToken);
        state.Images.AddRange(searchResult.Images ?? new List<string>());

        // Add deduplicated and cleaned sources to SourcesGathered
        var newSources = Formatting.DeduplicateAndCleanSources(searchResult.Results, state.SourcesGathered);
        state.SourcesGathered.AddRange(newSources);

        state.WebResearchResults.Add(Formatting.DeduplicateAndFormatSources(searchResult, _maxCharacterPerSource));

        NotifyProgress(new WebResearchProgress
        {
            Sources = searchResult.Results,
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

        NotifyProgress(new SummarizeProgress
        {
            Summary = state.RunningSummary
        });
    }

    private async Task ReflectOnSummaryAsync(ResearchState state, CancellationToken cancellationToken = default)
    {
        var prompt = string.Format(Prompts.ReflectionInstructions, state.ResearchTopic);
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(prompt),
            new UserChatMessage($"Reflect on our existing knowledge: \n===\n{state.RunningSummary},\n===\nAnd now identify a knowledge gap and generate a follow-up web search query:")
        };

        var result = await _aiChatClient.CompleteChatAsync(messages, _reflectionOnSummaryOptions);
        if (result.Value.FinishReason != ChatFinishReason.Stop) throw new InvalidOperationException($"AI chat completion failed with finish reason: {result.Value.FinishReason}");

        var reflectionResponse = JsonSerializer.Deserialize<ReflectionOnSummaryResponse>(result.Value.Content.First().Text, _jsonSerializerOptions);
        if (reflectionResponse == null) throw new InvalidOperationException("Failed to deserialize ReflectionOnSummaryResponse");

        state.SearchQuery = reflectionResponse.FollowUpQuery;
        state.KnowledgeGap = reflectionResponse.KnowledgeGap;

        NotifyProgress(new ReflectionProgress
        {
            Query = state.SearchQuery,
            KnowledgeGap = state.KnowledgeGap
        });
    }

    private Task FinalizeSummaryAsync(ResearchState state, CancellationToken cancellationToken = default)
    {
        // Keep summary pure without appending sources
        // Sources will be provided separately in ResearchResult.Sources

        NotifyProgress(new FinalizeProgress());

        return Task.CompletedTask;
    }

    private void NotifyProgress(ProgressBase progress)
    {
        _onProgressChanged?.Invoke(progress);
    }
}
