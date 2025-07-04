namespace DeepResearch.Core;

using System.Collections.Generic;

public class ResearchState
{
    public string ResearchTopic { get; set; }
    public string SearchQuery { get; set; }
    public string RunningSummary { get; set; }
    public int ResearchLoopCount { get; set; } = 0;
    public List<string> SourcesGathered { get; set; } = new();
    public List<string> WebResearchResults { get; set; } = new();
    public List<string> Images { get; set; } = new();
    public string KnowledgeGap { get; set; }
    public string QueryRationale { get; set; }
}
