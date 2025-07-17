namespace DeepResearch.Core.SearchClient;

public class SearchResult
{
    public List<SearchResultItem> Results { get; set; } = [];
    public List<string> Images { get; set; } = [];
}

public class SearchResultItem
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string RawContent { get; set; } = string.Empty;
}