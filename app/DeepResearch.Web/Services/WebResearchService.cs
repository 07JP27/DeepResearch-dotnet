using DeepResearch.Core;
using DeepResearch.Core.Events;
using DeepResearch.SearchClient;
using DeepResearch.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using OpenAI.Chat;
using System.Text.Json;

namespace DeepResearch.Web.Services;

public class WebResearchService
{
    private readonly IHubContext<ResearchHub> _hubContext;
    private readonly ChatClient? _chatClient;
    private readonly ISearchClient? _searchClient;
    private readonly ILogger<WebResearchService> _logger;

    public WebResearchService(
        IHubContext<ResearchHub> hubContext,
        ILogger<WebResearchService> logger,
        ChatClient? chatClient = null,
        ISearchClient? searchClient = null)
    {
        _hubContext = hubContext;
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
                await NotifyClient(clientId, "error", new { message = "Azure OpenAI設定が不完全です。appsettings.jsonを確認してください。" });
                return;
            }

            if (_searchClient == null)
            {
                await NotifyClient(clientId, "error", new { message = "Tavily API設定が不完全です。appsettings.jsonを確認してください。" });
                return;
            }

            await NotifyClient(clientId, "thinking", new { message = "研究を開始します..." });

            var researchService = new DeepResearchService(
                _chatClient,
                _searchClient,
                progress => OnProgressChanged(progress, clientId).Wait(),
                maxLoops: 3,
                maxTokensPerSource: 1000
            );

            var result = await researchService.RunResearchAsync(topic, cancellationToken);

            await NotifyClient(clientId, "research_complete", new
            {
                status = "complete",
                final_summary = result.RunningSummary
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "研究中にエラーが発生しました。トピック: {Topic}", topic);
            await NotifyClient(clientId, "error", new { message = $"エラーが発生しました: {ex.Message}" });
        }
    }

    private async Task OnProgressChanged(ResearchProgress progress, string clientId)
    {
        try
        {
            await NotifyClient(clientId, progress.Type, progress.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "進行状況の通知中にエラーが発生しました");
        }
    }

    private async Task NotifyClient(string clientId, string type, object data)
    {
        try
        {
            await _hubContext.Clients.Group(clientId).SendAsync("ReceiveProgress", new
            {
                type = type,
                data = data,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "クライアント通知中にエラーが発生しました");
        }
    }
}
