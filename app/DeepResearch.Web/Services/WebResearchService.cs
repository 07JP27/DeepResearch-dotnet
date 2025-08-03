using DeepResearch.Core;
using DeepResearch.Core.Models;

namespace DeepResearch.Web.Services;

public class WebResearchService : IAsyncProgress<ProgressBase>
{
    private readonly ILogger<WebResearchService> _logger;
    private readonly DeepResearchService _deepResearchService;
    private readonly TimeProvider _timeProvider;

    public event Action? OnProgressUpdated;

    public List<ProgressBase> ProgressHistory { get; private set; } = new List<ProgressBase>();

    public WebResearchService(
        ILogger<WebResearchService> logger,
        DeepResearchService deepResearchService,
        TimeProvider timeProvider)
    {
        _logger = logger;
        _deepResearchService = deepResearchService;
        _timeProvider = timeProvider;
    }

    // Progress作成ヘルパーメソッド
    private T CreateProgress<T>() where T : ProgressBase, new()
    {
        var progress = new T();
        progress.Timestamp = _timeProvider.GetUtcNow().DateTime;
        return progress;
    }

    private T CreateProgress<T>(Action<T> configure) where T : ProgressBase, new()
    {
        var progress = CreateProgress<T>();
        configure(progress);
        return progress;
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
                await NotifyClientAsync(CreateProgress<ErrorProgress>(p =>
                    p.Message = "DeepResearchService が初期化されていません。"));
                return null;
            }

            if (string.IsNullOrWhiteSpace(topic))
            {
                await NotifyClientAsync(CreateProgress<ErrorProgress>(p => p.Message = "トピックは必須です。"));
                return null;
            }

            await NotifyClientAsync(CreateProgress<ThinkingProgress>(p => p.Message = "調査を開始します..."));

            // Use the new async progress support
            return await _deepResearchService.RunResearchAsync(
                topic, 
                new DeepResearchOptions { MaxSourceCountPerSearch = 2 }, 
                this, 
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "調査中にエラーが発生しました。トピック: {Topic}", topic);
            await NotifyClientAsync(CreateProgress<ErrorProgress>(p => p.Message = $"エラーが発生しました: {ex.Message}"));
            return null;
        }
    }

    private async ValueTask NotifyClientAsync(ProgressBase progress)
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
