namespace DeepResearch.Core.Events;

public class ResearchProgress
{
    public string Type { get; set; }
    public object Data { get; set; }
    public string Step { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
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
}
