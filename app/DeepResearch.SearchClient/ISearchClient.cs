namespace DeepResearch.SearchClient;

public interface ISearchClient
{
    Task<SearchResult> SearchAsync(string query, int maxResults = 5, CancellationToken cancellationToken = default);
}
