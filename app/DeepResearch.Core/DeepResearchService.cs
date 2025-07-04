using System.Threading;
using System.Threading.Tasks;
using DeepResearch.Core.Clients;
using System.Collections.Generic;

namespace DeepResearch.Core;

public class DeepResearchService
{
    private readonly IAzureAIClient _aiClient;
    private readonly IWebSearchClient _searchClient;
    private readonly int _maxLoops;
    private readonly int _maxTokensPerSource;

    public DeepResearchService(IAzureAIClient aiClient, IWebSearchClient searchClient, int maxLoops = 3, int maxTokensPerSource = 1000)
    {
        _aiClient = aiClient;
        _searchClient = searchClient;
        _maxLoops = maxLoops;
        _maxTokensPerSource = maxTokensPerSource;
    }

    public async Task<ResearchState> RunResearchAsync(string topic, CancellationToken cancellationToken = default)
    {
        var state = new ResearchState { ResearchTopic = topic };
        await GenerateQueryAsync(state, cancellationToken);
        while (state.ResearchLoopCount < _maxLoops)
        {
            await WebResearchAsync(state, cancellationToken);
            await SummarizeSourcesAsync(state, cancellationToken);
            await ReflectOnSummaryAsync(state, cancellationToken);
            state.ResearchLoopCount++;
        }
        await FinalizeSummaryAsync(state, cancellationToken);
        return state;
    }

    private async Task GenerateQueryAsync(ResearchState state, CancellationToken cancellationToken = default)
    {
        var prompt = string.Format(Prompts.QueryWriterInstructions, Prompts.GetCurrentDate(), state.ResearchTopic);
        var messages = new List<Message>
        {
            new Message { Role = "system", Content = prompt },
            new Message { Role = "user", Content = "Generate a query for web search:" }
        };
        var result = await _aiClient.GetCompletionAsync(messages, cancellationToken);
        // ここでJSONパースしてstate.SearchQuery, state.QueryRationaleをセット
        // 例: { "query": "...", "rationale": "..." }
        var obj = System.Text.Json.JsonDocument.Parse(result).RootElement;
        state.SearchQuery = obj.GetProperty("query").GetString();
        state.QueryRationale = obj.GetProperty("rationale").GetString();
    }

    private async Task WebResearchAsync(ResearchState state, CancellationToken cancellationToken = default)
    {
        var searchResult = await _searchClient.SearchAsync(state.SearchQuery, 1, cancellationToken);
        state.Images.AddRange(searchResult.Images);
        state.SourcesGathered.Add(Formatting.FormatSources(searchResult));
        state.WebResearchResults.Add(Formatting.DeduplicateAndFormatSources(searchResult, _maxTokensPerSource));
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
        var messages = new List<Message>
        {
            new Message { Role = "system", Content = Prompts.SummarizerInstructions },
            new Message { Role = "user", Content = humanMessage }
        };
        var result = await _aiClient.GetCompletionAsync(messages, cancellationToken);
        state.RunningSummary = result.Trim();
    }

    private async Task ReflectOnSummaryAsync(ResearchState state, CancellationToken cancellationToken = default)
    {
        var prompt = string.Format(Prompts.ReflectionInstructions, state.ResearchTopic);
        var messages = new List<Message>
        {
            new Message { Role = "system", Content = prompt },
            new Message { Role = "user", Content = $"Reflect on our existing knowledge: \n===\n{state.RunningSummary},\n===\nAnd now identify a knowledge gap and generate a follow-up web search query:" }
        };
        var result = await _aiClient.GetCompletionAsync(messages, cancellationToken);
        try
        {
            var obj = System.Text.Json.JsonDocument.Parse(result).RootElement;
            state.SearchQuery = obj.GetProperty("follow_up_query").GetString();
            state.KnowledgeGap = obj.GetProperty("knowledge_gap").GetString();
        }
        catch
        {
            state.SearchQuery = $"Tell me more about {state.ResearchTopic}";
            state.KnowledgeGap = "Unable to identify specific knowledge gap";
        }
    }

    private Task FinalizeSummaryAsync(ResearchState state, CancellationToken cancellationToken = default)
    {
        // 画像やソースを含めた最終まとめ
        var imageSection = "";
        if (state.Images.Count >= 2)
        {
            imageSection = string.Concat(
                "<div class='flex flex-col md:flex-row gap-4 mb-6'>",
                "<div class='w-full md:w-1/2'>",
                $"<img src='{state.Images[0]}' alt='Research image 1' class='w-full h-auto rounded-lg shadow-md'>",
                "</div>",
                "<div class='w-full md:w-1/2'>",
                $"<img src='{state.Images[1]}' alt='Research image 2' class='w-full h-auto rounded-lg shadow-md'>",
                "</div>",
                "</div>\n"
            );
        }
        else if (state.Images.Count == 1)
        {
            imageSection = string.Concat(
                "<div class='flex justify-center mb-6'>",
                "<div class='w-full max-w-lg'>",
                $"<img src='{state.Images[0]}' alt='Research image' class='w-full h-auto rounded-lg shadow-md'>",
                "</div>",
                "</div>\n"
            );
        }
        var finalSummary = $"{imageSection}## Summary\n{state.RunningSummary}\n\n### Sources:\n";
        foreach (var source in state.SourcesGathered)
        {
            finalSummary += $"{source}\n";
        }
        state.RunningSummary = finalSummary;
        return Task.CompletedTask;
    }
}
