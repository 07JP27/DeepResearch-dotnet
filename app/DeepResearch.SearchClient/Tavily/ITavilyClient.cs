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

    /// <summary>
    /// Gets search context optimized for RAG applications
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="searchDepth">Search depth</param>
    /// <param name="topic">Topic to focus on</param>
    /// <param name="days">Number of days to search within</param>
    /// <param name="maxResults">Maximum number of results</param>
    /// <param name="includeDomains">Domains to include</param>
    /// <param name="excludeDomains">Domains to exclude</param>
    /// <param name="maxTokens">Maximum tokens for context</param>
    /// <param name="timeout">Timeout in seconds</param>
    /// <param name="country">Country code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search context as JSON string</returns>
    Task<string> GetSearchContextAsync(
        string query,
        TavilySearchDepth searchDepth = TavilySearchDepth.Basic,
        TavilyTopic topic = TavilyTopic.General,
        int days = 7,
        int maxResults = 5,
        IEnumerable<string>? includeDomains = null,
        IEnumerable<string>? excludeDomains = null,
        int maxTokens = 4000,
        int timeout = 60,
        string? country = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a quick answer to a question
    /// </summary>
    /// <param name="query">The question</param>
    /// <param name="searchDepth">Search depth</param>
    /// <param name="topic">Topic to focus on</param>
    /// <param name="days">Number of days to search within</param>
    /// <param name="maxResults">Maximum number of results</param>
    /// <param name="includeDomains">Domains to include</param>
    /// <param name="excludeDomains">Domains to exclude</param>
    /// <param name="timeout">Timeout in seconds</param>
    /// <param name="country">Country code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Answer as string</returns>
    Task<string> QnaSearchAsync(
        string query,
        TavilySearchDepth searchDepth = TavilySearchDepth.Advanced,
        TavilyTopic topic = TavilyTopic.General,
        int days = 7,
        int maxResults = 5,
        IEnumerable<string>? includeDomains = null,
        IEnumerable<string>? excludeDomains = null,
        int timeout = 60,
        string? country = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts content from one or more URLs
    /// </summary>
    /// <param name="urls">URLs to extract content from</param>
    /// <param name="includeImages">Whether to include images</param>
    /// <param name="extractDepth">Extract depth</param>
    /// <param name="format">Content format</param>
    /// <param name="timeout">Timeout in seconds</param>
    /// <param name="includeFavicon">Whether to include favicons</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted content</returns>
    Task<TavilyExtractResult> ExtractAsync(
        IEnumerable<string> urls,
        bool? includeImages = null,
        TavilyExtractDepth? extractDepth = null,
        TavilyContentFormat? format = null,
        int timeout = 60,
        bool? includeFavicon = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Crawls a website starting from a base URL
    /// </summary>
    /// <param name="url">Starting URL</param>
    /// <param name="maxDepth">Maximum crawl depth</param>
    /// <param name="maxBreadth">Maximum breadth per level</param>
    /// <param name="limit">Maximum number of pages to crawl</param>
    /// <param name="instructions">Crawling instructions</param>
    /// <param name="selectPaths">Paths to include</param>
    /// <param name="selectDomains">Domains to include</param>
    /// <param name="excludePaths">Paths to exclude</param>
    /// <param name="excludeDomains">Domains to exclude</param>
    /// <param name="allowExternal">Whether to allow external links</param>
    /// <param name="categories">Categories to focus on</param>
    /// <param name="extractDepth">Extract depth</param>
    /// <param name="includeImages">Whether to include images</param>
    /// <param name="format">Content format</param>
    /// <param name="timeout">Timeout in seconds</param>
    /// <param name="includeFavicon">Whether to include favicons</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Crawl results</returns>
    Task<TavilyCrawlResult> CrawlAsync(
        string url,
        int? maxDepth = null,
        int? maxBreadth = null,
        int? limit = null,
        string? instructions = null,
        IEnumerable<string>? selectPaths = null,
        IEnumerable<string>? selectDomains = null,
        IEnumerable<string>? excludePaths = null,
        IEnumerable<string>? excludeDomains = null,
        bool? allowExternal = null,
        IEnumerable<TavilyCategory>? categories = null,
        TavilyExtractDepth? extractDepth = null,
        bool? includeImages = null,
        TavilyContentFormat? format = null,
        int timeout = 60,
        bool? includeFavicon = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps the structure of a website
    /// </summary>
    /// <param name="url">Starting URL</param>
    /// <param name="maxDepth">Maximum map depth</param>
    /// <param name="maxBreadth">Maximum breadth per level</param>
    /// <param name="limit">Maximum number of pages to map</param>
    /// <param name="instructions">Mapping instructions</param>
    /// <param name="selectPaths">Paths to include</param>
    /// <param name="selectDomains">Domains to include</param>
    /// <param name="excludePaths">Paths to exclude</param>
    /// <param name="excludeDomains">Domains to exclude</param>
    /// <param name="allowExternal">Whether to allow external links</param>
    /// <param name="includeImages">Whether to include images</param>
    /// <param name="categories">Categories to focus on</param>
    /// <param name="timeout">Timeout in seconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Map results</returns>
    Task<TavilyMapResult> MapAsync(
        string url,
        int? maxDepth = null,
        int? maxBreadth = null,
        int? limit = null,
        string? instructions = null,
        IEnumerable<string>? selectPaths = null,
        IEnumerable<string>? selectDomains = null,
        IEnumerable<string>? excludePaths = null,
        IEnumerable<string>? excludeDomains = null,
        bool? allowExternal = null,
        bool? includeImages = null,
        IEnumerable<TavilyCategory>? categories = null,
        int timeout = 60,
        CancellationToken cancellationToken = default);
}
