namespace DeepResearch.Core;

public interface ISearchClient
{
    Task<SearchResult> SearchAsync(string query, int maxResults, CancellationToken cancellationToken = default);
}