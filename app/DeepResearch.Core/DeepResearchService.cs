using System.Threading;
using System.Threading.Tasks;
using DeepResearch.SearchClient.Tavily;
using DeepResearch.SearchClient;
using DeepResearch.Core.Events;
using System.Collections.Generic;
using System;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace DeepResearch.Core;

public class DeepResearchService
{
    private readonly ChatClient _aiChatClient;
    private readonly ISearchClient _searchClient;
    private readonly int _maxLoops;
    private readonly int _maxTokensPerSource;
    private readonly Action<ProgressBase>? _onProgressChanged;

    public DeepResearchService(
        ChatClient aiChatClient,
        ISearchClient searchClient,
        Action<ProgressBase>? onProgressChanged = null,
        int maxLoops = 3,
        int maxTokensPerSource = 1000)
    {
        _aiChatClient = aiChatClient;
        _searchClient = searchClient;
        _onProgressChanged = onProgressChanged;
        _maxLoops = maxLoops;
        _maxTokensPerSource = maxTokensPerSource;
    }

    public async Task<ResearchState> RunResearchAsync(string topic, CancellationToken cancellationToken = default)
    {
        var state = new ResearchState { ResearchTopic = topic };

        await GenerateQueryAsync(state, cancellationToken);

        while (state.ResearchLoopCount < _maxLoops)
        {
            // ルーティング通知
            NotifyProgress(new RoutingProgress
            {
                Decision = "continue",
                LoopCount = state.ResearchLoopCount
            });

            await WebResearchAsync(state, cancellationToken);
            await SummarizeSourcesAsync(state, cancellationToken);
            await ReflectOnSummaryAsync(state, cancellationToken);
            state.ResearchLoopCount++;
        }

        // 最終化通知
        NotifyProgress(new RoutingProgress
        {
            Decision = "finalize",
            LoopCount = state.ResearchLoopCount
        });

        await FinalizeSummaryAsync(state, cancellationToken);

        // 完了通知
        NotifyProgress(new ResearchCompleteProgress
        {
            Status = "complete"
        });

        return state;
    }

    private async Task GenerateQueryAsync(ResearchState state, CancellationToken cancellationToken = default)
    {
        var prompt = string.Format(Prompts.QueryWriterInstructions, Prompts.GetCurrentDate(), state.ResearchTopic);
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(prompt),
            new UserChatMessage("Generate a query for web search:")
        };
        var result = await _aiChatClient.CompleteChatAsync(messages);
        var textResult = result.Value.Content.First().Text;

        // JSONパースしてstate.SearchQuery, state.QueryRationaleをセット
        var obj = System.Text.Json.JsonDocument.Parse(textResult).RootElement;
        state.SearchQuery = obj.GetProperty("query").GetString() ?? "";
        state.QueryRationale = obj.GetProperty("rationale").GetString() ?? "";

        // 通知
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
            maxResults: 2,
            cancellationToken: cancellationToken);
        state.Images.AddRange(searchResult.Images ?? new List<string>());
        state.SourcesGathered.Add(Formatting.FormatSources(searchResult));
        state.WebResearchResults.Add(Formatting.DeduplicateAndFormatSources(searchResult, _maxTokensPerSource));

        // 通知
        NotifyProgress(new WebResearchProgress
        {
            SearchWords = state.SearchQuery,
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

        // 通知
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
        var result = await _aiChatClient.CompleteChatAsync(messages);
        var textResult = result.Value.Content.First().Text;
        try
        {
            var obj = System.Text.Json.JsonDocument.Parse(textResult).RootElement;
            state.SearchQuery = obj.GetProperty("follow_up_query").GetString() ?? "";
            state.KnowledgeGap = obj.GetProperty("knowledge_gap").GetString() ?? "";
        }
        catch
        {
            state.SearchQuery = $"Tell me more about {state.ResearchTopic}";
            state.KnowledgeGap = "Unable to identify specific knowledge gap";
        }

        // 通知
        NotifyProgress(new ReflectionProgress
        {
            Query = state.SearchQuery,
            KnowledgeGap = state.KnowledgeGap
        });
    }

    private Task FinalizeSummaryAsync(ResearchState state, CancellationToken cancellationToken = default)
    {
        // テキストベースの最終まとめ（画像の描画はクライアント側の責任）
        var finalSummary = $"## Summary\n{state.RunningSummary}\n\n### Sources:\n";
        foreach (var source in state.SourcesGathered)
        {
            finalSummary += $"{source}\n";
        }
        state.RunningSummary = finalSummary;

        // 通知
        NotifyProgress(new FinalizeProgress
        {
            Summary = state.RunningSummary,
            Images = state.Images
        });

        return Task.CompletedTask;
    }

    // 型安全な進行状況通知のヘルパーメソッド
    private void NotifyProgress(ProgressBase progress)
    {
        _onProgressChanged?.Invoke(progress);
    }
}
