using System.Collections.Generic;
using DeepResearch.SearchClient;

namespace DeepResearch.Core.Events;

// 基底クラス
public abstract class ProgressBase
{
    protected ProgressBase(string type)
    {
        Type = type;
    }

    public string Type { get; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // 後方互換性のため
    public string Step => Type;
}

// クエリ生成プログレス
public class QueryGenerationProgress : ProgressBase
{
    public QueryGenerationProgress() : base(ProgressTypes.GenerateQuery)
    {
    }

    public string Query { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
}

// ウェブ検索プログレス
public class WebResearchProgress : ProgressBase
{
    public WebResearchProgress() : base(ProgressTypes.WebResearch)
    {
    }

    public string SearchWords { get; set; } = string.Empty;
    public List<SearchResultItem> Sources { get; set; } = new();
    public List<string> Images { get; set; } = new();
}

// 要約プログレス
public class SummarizeProgress : ProgressBase
{
    public SummarizeProgress() : base(ProgressTypes.Summarize)
    {
    }

    public string Summary { get; set; } = string.Empty;
}

// リフレクションプログレス
public class ReflectionProgress : ProgressBase
{
    public ReflectionProgress() : base(ProgressTypes.Reflection)
    {
    }

    public string Query { get; set; } = string.Empty;
    public string KnowledgeGap { get; set; } = string.Empty;
}

// 最終化プログレス
public class FinalizeProgress : ProgressBase
{
    public FinalizeProgress() : base(ProgressTypes.Finalize)
    {
    }

    public string Summary { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
}

// ルーティングプログレス
public class RoutingProgress : ProgressBase
{
    public RoutingProgress() : base(ProgressTypes.Routing)
    {
    }

    public string Decision { get; set; } = string.Empty;
    public int LoopCount { get; set; }
}

// 研究完了プログレス
public class ResearchCompleteProgress : ProgressBase
{
    public ResearchCompleteProgress() : base(ProgressTypes.ResearchComplete)
    {
    }

    public string Status { get; set; } = string.Empty;
    public string? FinalSummary { get; set; }
    public List<string> Images { get; set; } = new();
}

// 思考中プログレス
public class ThinkingProgress : ProgressBase
{
    public ThinkingProgress() : base(ProgressTypes.Thinking)
    {
    }

    public string Message { get; set; } = string.Empty;
}

// エラープログレス
public class ErrorProgress : ProgressBase
{
    public ErrorProgress() : base("error")
    {
    }

    public string Message { get; set; } = string.Empty;
}

public static class ProgressTypes
{
    public const string Thinking = "thinking";
    public const string GenerateQuery = "generate_query";
    public const string WebResearch = "web_research";
    public const string Summarize = "summarize";
    public const string Reflection = "reflection";
    public const string Finalize = "finalize";
    public const string Routing = "routing";
    public const string ResearchComplete = "research_complete";
    public const string Error = "error";
}
