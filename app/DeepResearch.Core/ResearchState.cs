using DeepResearch.Core.SearchClient;
using Microsoft.Extensions.AI;

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
    public List<SearchResultItem> SourcesGathered { get; set; } = [];
    public List<string> WebResearchResults { get; set; } = [];
    public List<string> Images { get; set; } = [];
    public string KnowledgeGap { get; set; } = string.Empty;
    public string QueryRationale { get; set; } = string.Empty;
    public List<string> SummariesGathered { get; set; } = [];
    public int QueryRetryCount { get; set; } = 0;
    public List<ChatMessage> QueryGenerationMessages { get; set; } = [];
    public List<ChatMessage> ReflectionMessages { get; set; } = [];
}
