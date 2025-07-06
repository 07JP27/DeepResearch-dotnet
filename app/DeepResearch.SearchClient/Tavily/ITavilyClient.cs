namespace DeepResearch.SearchClient.Tavily;

/// <summary>
/// Interface for Tavily web search client
/// </summary>
public interface ITavilyClient
{
    /// <summary>
    /// Performs a search query and returns comprehensive results
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="searchDepth">Search depth (basic or advanced)</param>
    /// <param name="topic">Topic to focus on (general, news, finance)</param>
    /// <param name="timeRange">Time range for results</param>
    /// <param name="days">Number of days to search within</param>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <param name="includeDomains">Domains to include in search</param>
    /// <param name="excludeDomains">Domains to exclude from search</param>
    /// <param name="includeAnswer">Whether to include a generated answer</param>
    /// <param name="includeRawContent">Whether to include raw content</param>
    /// <param name="includeImages">Whether to include images</param>
    /// <param name="timeout">Timeout in seconds</param>
    /// <param name="country">Country code for localized results</param>
    /// <param name="includeFavicon">Whether to include favicons</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results</returns>
    Task<TavilySearchResult> SearchAsync(
        string query,
        TavilySearchDepth? searchDepth = null,
        TavilyTopic? topic = null,
        TavilyTimeRange? timeRange = null,
        int? days = null,
        int? maxResults = null,
        IEnumerable<string>? includeDomains = null,
        IEnumerable<string>? excludeDomains = null,
        bool? includeAnswer = null,
        bool? includeRawContent = null,
        bool? includeImages = null,
        int timeout = 60,
        string? country = null,
        bool? includeFavicon = null,
        CancellationToken cancellationToken = default);
}