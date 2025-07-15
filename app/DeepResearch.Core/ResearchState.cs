using DeepResearch.SearchClient;
using OpenAI.Chat;

namespace DeepResearch.Core;

/// <summary>
/// Internal state used during the research process. Contains all intermediate data and processing state.
/// </summary>
internal class ResearchState
{
    public string ResearchTopic { get; set; } = string.Empty;
    public string SearchQuery { get; set; } = string.Empty;
    public string RunningSummary { get; set; } = string.Empty;
    public int ResearchLoopCount { get; set; } = 0;
    public List<SearchResultItem> SourcesGathered { get; set; } = new();
    public List<string> WebResearchResults { get; set; } = new();
    public List<string> Images { get; set; } = new();
    public string KnowledgeGap { get; set; } = string.Empty;
    public string QueryRationale { get; set; } = string.Empty;
    public List<string> SummariesGathered { get; set; } = new();
    public int QueryRetryCount { get; set; } = 0;
    public List<ChatMessage> QueryGenerationMessages { get; set; } = new();
    public List<ChatMessage> ReflectionMessages { get; set; } = new();
}
