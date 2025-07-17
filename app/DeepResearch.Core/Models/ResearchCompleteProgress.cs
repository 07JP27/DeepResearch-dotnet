using DeepResearch.Core.SearchClient;

namespace DeepResearch.Core.Models;

public class ResearchCompleteProgress() : ProgressBase(ProgressTypes.ResearchComplete)
{
    public string? FinalSummary { get; set; }
    public List<SearchResultItem> Sources { get; set; } = [];
    public List<string> Images { get; set; } = [];
}