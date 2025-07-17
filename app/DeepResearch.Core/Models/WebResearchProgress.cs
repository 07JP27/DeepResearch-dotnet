using DeepResearch.Core.SearchClient;

namespace DeepResearch.Core.Models;

public class WebResearchProgress : ProgressBase
{
    public WebResearchProgress() : base(ProgressTypes.WebResearch)
    {
    }

    public List<SearchResultItem> Sources { get; set; } = new();
    public List<string> Images { get; set; } = new();
}