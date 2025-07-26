using DeepResearch.Core.SearchClient;

namespace DeepResearch.Core.Models;

public class WebResearchProgress() : ProgressBase(ProgressTypes.WebResearch)
{
    public List<SearchResultItem> Sources { get; set; } = [];
    public List<string> Images { get; set; } = [];
}
