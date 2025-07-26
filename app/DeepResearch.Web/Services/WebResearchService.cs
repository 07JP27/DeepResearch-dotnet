using DeepResearch.Core;
using DeepResearch.Core.Models;

namespace DeepResearch.Web.Services;

public class WebResearchService
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

    public async Task<ResearchResult?> StartResearchAsync(string topic, CancellationToken cancellationToken = default)
    {
        try
        {
            // 設定チェック
            if (_deepResearchService == null)
            {
                NotifyClient(CreateProgress<ErrorProgress>(p => 
                    p.Message = "DeepResearchService が初期化されていません。"));
                return null;
            }

            if (string.IsNullOrWhiteSpace(topic))
            {
                NotifyClient(CreateProgress<ErrorProgress>(p => 
                    p.Message = "トピックは必須です。"));
                return null;
            }

            NotifyClient(CreateProgress<ThinkingProgress>(p => 
                p.Message = "調査を開始します..."));

            // 進捗状況を追跡するプログレスオブジェクトを作成
            var progress = new Progress<ProgressBase>(async progress => await OnProgressChanged(progress));

            return await _deepResearchService.RunResearchAsync(topic, new() { MaxSourceCountPerSearch = 2 }, progress, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "調査中にエラーが発生しました。トピック: {Topic}", topic);
            NotifyClient(CreateProgress<ErrorProgress>(p => 
                p.Message = $"エラーが発生しました: {ex.Message}"));
            return null;
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
