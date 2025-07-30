using DeepResearch.Core;
using DeepResearch.Core.Models;

namespace DeepResearch.Web.Services;

public class WebResearchService : IAsyncProgress<ProgressBase>
{
    private readonly ILogger<WebResearchService> _logger;
    private readonly DeepResearchService _deepResearchService;

    public event Action? OnProgressUpdated;

    public List<ProgressBase> ProgressHistory { get; private set; } = new List<ProgressBase>();

    public WebResearchService(
        ILogger<WebResearchService> logger,
        DeepResearchService deepResearchService)
    {
        _logger = logger;
        _deepResearchService = deepResearchService;
    }

    public async ValueTask ReportAsync(ProgressBase value, CancellationToken cancellationToken = default)
    {
        try
        {
            await NotifyClientAsync(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "進行状況の通知中にエラーが発生しました");
        }
    }

    public async Task<ResearchResult?> StartResearchAsync(string topic, CancellationToken cancellationToken = default)
    {
        try
        {
            // 設定チェック
            if (_deepResearchService == null)
            {
                await NotifyClientAsync(new ErrorProgress { Message = "DeepResearchService が初期化されていません。" });
                return null;
            }

            if (string.IsNullOrWhiteSpace(topic))
            {
                await NotifyClientAsync(new ErrorProgress { Message = "トピックは必須です。" });
                return null;
            }

            await NotifyClientAsync(new ThinkingProgress { Message = "調査を開始します..." });

            // Use the new async progress support
            return await _deepResearchService.RunResearchWithAsyncProgressAsync(
                topic, 
                new DeepResearchOptions { MaxSourceCountPerSearch = 2 }, 
                this, 
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "調査中にエラーが発生しました。トピック: {Topic}", topic);
            await NotifyClientAsync(new ErrorProgress { Message = $"エラーが発生しました: {ex.Message}" });
            return null;
        }
    }

    private async ValueTask NotifyClientAsync(ProgressBase progress)
    {
        try
        {
            ProgressHistory.Add(progress);
            OnProgressUpdated?.Invoke();
            
            // Allow for any async operations that might be needed by subscribers
            await Task.Delay(1, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "クライアント通知中にエラーが発生しました");
        }
    }
}
