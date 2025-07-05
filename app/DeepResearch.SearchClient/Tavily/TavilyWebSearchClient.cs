namespace DeepResearch.SearchClient.Tavily;

/// <summary>
/// Web search client implementation using Tavily API
/// </summary>
public class TavilyWebSearchClient : ISearchClient
{
    private readonly ITavilyClient _tavilyClient;

    /// <summary>
    /// Initializes a new instance of the TavilyWebSearchClient
    /// </summary>
    /// <param name="tavilyClient">Tavily client instance</param>
    public TavilyWebSearchClient(ITavilyClient tavilyClient)
    {
        _tavilyClient = tavilyClient ?? throw new ArgumentNullException(nameof(tavilyClient));
    }

    /// <inheritdoc />
    public async Task<SearchResult> SearchAsync(string query, int maxResults = 5, CancellationToken cancellationToken = default)
    {
        try
        {
            var tavilyResult = await _tavilyClient.SearchAsync(
                query: query,
                maxResults: maxResults,
                searchDepth: TavilySearchDepth.Advanced,
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
        catch (TavilyInvalidApiKeyException ex)
        {
            throw new InvalidOperationException("Invalid Tavily API key", ex);
        }
        catch (TavilyUsageLimitExceededException ex)
        {
            throw new InvalidOperationException("Tavily usage limit exceeded", ex);
        }
        catch (TavilyTimeoutException ex)
        {
            throw new TimeoutException("Tavily request timed out", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to perform web search", ex);
        }
    }
}
