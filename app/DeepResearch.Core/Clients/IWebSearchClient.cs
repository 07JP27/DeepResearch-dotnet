namespace DeepResearch.Core.Clients;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class SearchResult
{
    public List<SearchResultItem> Results { get; set; } = new();
    public List<string> Images { get; set; } = new();
}

public class SearchResultItem
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string Content { get; set; }
    public string RawContent { get; set; }
}

public interface IWebSearchClient
{
    Task<SearchResult> SearchAsync(string query, int maxResults = 5, CancellationToken cancellationToken = default);
}
