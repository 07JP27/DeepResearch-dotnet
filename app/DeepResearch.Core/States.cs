namespace DeepResearch.Core;

// SummaryState: レポート作成のための状態を保持するクラス
public class SummaryState
{
    /// <summary>レポートトピック</summary>
    public string? ResearchTopic { get; set; }
    /// <summary>検索クエリ</summary>
    public string? SearchQuery { get; set; }
    /// <summary>検索クエリの根拠</summary>
    public string? Rationale { get; set; }
    /// <summary>Webリサーチ結果</summary>
    public List<object> WebResearchResults { get; set; } = new();
    /// <summary>収集したソース</summary>
    public List<object> SourcesGathered { get; set; } = new();
    /// <summary>リサーチループ回数</summary>
    public int ResearchLoopCount { get; set; } = 0;
    /// <summary>最終レポート</summary>
    public string? RunningSummary { get; set; }
    /// <summary>知識ギャップ</summary>
    public string? KnowledgeGap { get; set; }
    /// <summary>WebSocket ID</summary>
    public string? WebsocketId { get; set; }
    /// <summary>モデルの思考</summary>
    public string? Thoughts { get; set; }
}

// SummaryStateInput: 入力用状態クラス
public class SummaryStateInput
{
    /// <summary>レポートトピック</summary>
    public string? ResearchTopic { get; set; }
    /// <summary>WebSocket ID</summary>
    public string? WebsocketId { get; set; }
}

// SummaryStateOutput: 出力用状態クラス
public class SummaryStateOutput
{
    /// <summary>最終レポート</summary>
    public string? RunningSummary { get; set; }
}
