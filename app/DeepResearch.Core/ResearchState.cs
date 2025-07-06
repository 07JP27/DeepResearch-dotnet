namespace DeepResearch.Core;

using System.Collections.Generic;

public class ResearchState
{
    public string ResearchTopic { get; set; } = string.Empty;
    public string SearchQuery { get; set; } = string.Empty;
    public string RunningSummary { get; set; } = string.Empty;
    public int ResearchLoopCount { get; set; } = 0;
    public List<string> SourcesGathered { get; set; } = new();
    public List<string> WebResearchResults { get; set; } = new();
    public List<string> Images { get; set; } = new();
    public string KnowledgeGap { get; set; } = string.Empty;
    public string QueryRationale { get; set; } = string.Empty;
}
