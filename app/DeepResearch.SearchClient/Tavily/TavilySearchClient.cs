using DeepResearch.Core;

namespace DeepResearch.SearchClient.Tavily;

/// <summary>
/// Web search client implementation using Tavily API
/// </summary>
public class TavilySearchClient : ISearchClient
{
    private readonly ITavilyClient _tavilyClient;

    /// <summary>
    /// Initializes a new instance of the TavilySearchClient
    /// </summary>
    /// <param name="tavilyClient">Tavily client instance</param>
    public TavilySearchClient(ITavilyClient tavilyClient)
    {
        _tavilyClient = tavilyClient ?? throw new ArgumentNullException(nameof(tavilyClient));
    }

    /// <inheritdoc />
    public async Task<SearchResult> SearchAsync(string query, int maxResults, CancellationToken cancellationToken = default)
    {
        try
        {
            var tavilyResult = await _tavilyClient.SearchAsync(
                query: query,
                maxResults: maxResults,
                searchDepth: TavilySearchDepth.Basic,
                includeRawContent: true,
                includeImages: true,
                cancellationToken: cancellationToken);

            var searchResult = new SearchResult
            {
                Results = tavilyResult.Results.Select(r => new SearchResultItem
                {
                    Title = r.Title,
                    Url = r.Url,
                    Content = r.Content,
                    RawContent = r.RawContent ?? r.Content
                }).ToList(),
                Images = tavilyResult.Images
            };

            return searchResult;
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException("API key error", ex);
        }
        catch (TimeoutException ex)
        {
            throw new TimeoutException("Request timed out", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to perform web search", ex);
        }
    }
}
