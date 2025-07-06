using DeepResearch.Core;
using DeepResearch.Core.Events;
using DeepResearch.SearchClient;
using OpenAI.Chat;
using System.Text.Json;

namespace DeepResearch.Web.Services;

public class WebResearchService
{
    private readonly ChatClient? _chatClient;
    private readonly ISearchClient? _searchClient;
    private readonly ILogger<WebResearchService> _logger;
    public event Action? OnProgressUpdated;

    public List<ProgressBase> ProgressHistory { get; private set; } = new List<ProgressBase>();

    public WebResearchService(
        ILogger<WebResearchService> logger,
        ChatClient? chatClient = null,
        ISearchClient? searchClient = null)
    {
        _logger = logger;
        _chatClient = chatClient;
        _searchClient = searchClient;
    }

    public async Task StartResearchAsync(string topic, CancellationToken cancellationToken = default)
    {
        try
        {
            // 設定チェック
            if (_chatClient == null)
            {
                NotifyClient(new ErrorProgress { Message = "Azure OpenAI設定が不完全です。appsettings.jsonを確認してください。" });
                return;
            }

            if (_searchClient == null)
            {
                NotifyClient(new ErrorProgress { Message = "Tavily API設定が不完全です。appsettings.jsonを確認してください。" });
                return;
            }

            if (string.IsNullOrWhiteSpace(topic))
            {
                NotifyClient(new ErrorProgress { Message = "トピックは必須です。" });
                return;
            }

            NotifyClient(new ThinkingProgress { Message = "調査を開始します..." });

            var researchService = new DeepResearchService(
                _chatClient,
                _searchClient,
                progress => OnProgressChanged(progress).Wait(),
                maxLoops: 3,
                maxTokensPerSource: 1000
            );

            await researchService.RunResearchAsync(topic, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "調査中にエラーが発生しました。トピック: {Topic}", topic);
            NotifyClient(new ErrorProgress { Message = $"エラーが発生しました: {ex.Message}" });
        }
    }

    private Task OnProgressChanged(ProgressBase progress)
    {
        try
        {
            NotifyClient(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "進行状況の通知中にエラーが発生しました");
        }

        return Task.CompletedTask;
    }

    private void NotifyClient(ProgressBase progress)
    {
        try
        {
            ProgressHistory.Add(progress);
            OnProgressUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "クライアント通知中にエラーが発生しました");
        }
    }
}
