using DeepResearch.Core;
using DeepResearch.Core.Events;
using DeepResearch.SearchClient;
using OpenAI.Chat;
using System.Text.Json;

namespace DeepResearch.Web.Services;

public class WebResearchService
{
    private readonly ResearchProgressService _progressService;
    private readonly ChatClient? _chatClient;
    private readonly ISearchClient? _searchClient;
    private readonly ILogger<WebResearchService> _logger;

    public WebResearchService(
        ResearchProgressService progressService,
        ILogger<WebResearchService> logger,
        ChatClient? chatClient = null,
        ISearchClient? searchClient = null)
    {
        _progressService = progressService;
        _logger = logger;
        _chatClient = chatClient;
        _searchClient = searchClient;
    }

    public async Task StartResearchAsync(string topic, string clientId, CancellationToken cancellationToken = default)
    {
        try
        {
            // 設定チェック
            if (_chatClient == null)
            {
                NotifyClient(clientId, "error", new { message = "Azure OpenAI設定が不完全です。appsettings.jsonを確認してください。" });
                return;
            }

            if (_searchClient == null)
            {
                NotifyClient(clientId, "error", new { message = "Tavily API設定が不完全です。appsettings.jsonを確認してください。" });
                return;
            }

            NotifyClient(clientId, "thinking", new { message = "調査を開始します..." });

            var researchService = new DeepResearchService(
                _chatClient,
                _searchClient,
                progress => OnProgressChanged(progress, clientId).Wait(),
                maxLoops: 3,
                maxTokensPerSource: 1000
            );

            var result = await researchService.RunResearchAsync(topic, cancellationToken);

            NotifyClient(clientId, "research_complete", new
            {
                status = "complete",
                final_summary = result.RunningSummary
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "調査中にエラーが発生しました。トピック: {Topic}", topic);
            NotifyClient(clientId, "error", new { message = $"エラーが発生しました: {ex.Message}" });
        }
    }

    private Task OnProgressChanged(ResearchProgress progress, string clientId)
    {
        try
        {
            NotifyClient(clientId, progress.Type, progress.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "進行状況の通知中にエラーが発生しました");
        }

        return Task.CompletedTask;
    }

    private void NotifyClient(string clientId, string type, object data)
    {
        try
        {
            _progressService.AddProgress(clientId, type, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "クライアント通知中にエラーが発生しました");
        }
    }
}
