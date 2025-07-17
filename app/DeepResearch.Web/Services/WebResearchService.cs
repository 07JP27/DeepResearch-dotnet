using DeepResearch.Core;
using DeepResearch.Core.Models;
using DeepResearch.Core.SearchClient;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace DeepResearch.Web.Services;

public class WebResearchService
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

    public async Task<ResearchResult?> StartResearchAsync(string topic, CancellationToken cancellationToken = default)
    {
        try
        {
            // 設定チェック
            if (_deepResearchService == null)
            {
                NotifyClient(new ErrorProgress { Message = "DeepResearchService が初期化されていません。" });
                return null;
            }

            if (string.IsNullOrWhiteSpace(topic))
            {
                NotifyClient(new ErrorProgress { Message = "トピックは必須です。" });
                return null;
            }

            NotifyClient(new ThinkingProgress { Message = "調査を開始します..." });

            var reseachOption = new DeepResearchOptions
            {
                MaxSourceCountPerSearch = 2,
            };

            // 進捗状況を追跡するプログレスオブジェクトを作成
            var progress = new Progress<ProgressBase>(async progress => await OnProgressChanged(progress));

            return await _deepResearchService.RunResearchAsync(topic, reseachOption, progress, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "調査中にエラーが発生しました。トピック: {Topic}", topic);
            NotifyClient(new ErrorProgress { Message = $"エラーが発生しました: {ex.Message}" });
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
