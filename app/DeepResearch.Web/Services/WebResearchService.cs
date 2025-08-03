using DeepResearch.Core;
using DeepResearch.Core.Models;

namespace DeepResearch.Web.Services;

public class WebResearchService
{
    private readonly ILogger<WebResearchService> _logger;
    private readonly DeepResearchService _deepResearchService;
    private readonly TimeProvider _timeProvider;

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

    public async Task<ResearchResult?> StartResearchAsync(string topic, IAsyncProgress<ProgressBase> asyncProgress, CancellationToken cancellationToken = default)
    {
        try
        {
            // 設定チェック
            if (_deepResearchService == null)
            {
                await NotifyClientAsync(asyncProgress, CreateProgress<ErrorProgress>(p =>
                    p.Message = "DeepResearchService が初期化されていません。"));
                return null;
            }

            if (string.IsNullOrWhiteSpace(topic))
            {
                await NotifyClientAsync(asyncProgress, CreateProgress<ErrorProgress>(p => p.Message = "トピックは必須です。"));
                return null;
            }

            await NotifyClientAsync(asyncProgress, CreateProgress<ThinkingProgress>(p => p.Message = "調査を開始します..."));

            // Use the new async progress support
            return await _deepResearchService.RunResearchAsync(
                topic, 
                new DeepResearchOptions { MaxSourceCountPerSearch = 2 }, 
                new AsyncProgress<ProgressBase>(async (progress, _) =>
                {
                    await NotifyClientAsync(asyncProgress, progress);
                }), 
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "調査中にエラーが発生しました。トピック: {Topic}", topic);
            await NotifyClientAsync(
                asyncProgress, 
                CreateProgress<ErrorProgress>(p => p.Message = $"エラーが発生しました: {ex.Message}"));
            return null;
        }
    }

    private async Task NotifyClientAsync(IAsyncProgress<ProgressBase> asyncProgress, ProgressBase progress)
    {
        try
        {
            ProgressHistory.Add(progress);
            await asyncProgress.ReportAsync(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "クライアント通知中にエラーが発生しました");
        }
    }
}
