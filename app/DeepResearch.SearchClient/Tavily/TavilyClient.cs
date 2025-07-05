using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeepResearch.SearchClient.Tavily;

/// <summary>
/// Tavily web search API client implementation
/// </summary>
public class TavilyClient : ITavilyClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    private const string DefaultBaseUrl = "https://api.tavily.com";
    private const string ClientSource = "deepresearch-dotnet";

    /// <summary>
    /// Initializes a new instance of the TavilyClient
    /// </summary>
    /// <param name="httpClient">HTTP client instance</param>
    /// <param name="apiKey">Tavily API key (if null, will try to get from environment variable TAVILY_API_KEY)</param>
    /// <param name="baseUrl">Base URL for Tavily API</param>
    public TavilyClient(HttpClient httpClient, string? apiKey = null, string? baseUrl = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        
        _apiKey = apiKey ?? Environment.GetEnvironmentVariable("TAVILY_API_KEY") ?? 
                  throw new MissingApiKeyException();
        
        _baseUrl = baseUrl ?? DefaultBaseUrl;
        
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _httpClient.DefaultRequestHeaders.Add("X-Client-Source", ClientSource);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <inheritdoc />
    public async Task<TavilySearchResult> SearchAsync(
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
        CancellationToken cancellationToken = default)
    {
        var requestData = new Dictionary<string, object?>
        {
            ["query"] = query,
            ["search_depth"] = searchDepth?.ToString().ToLowerInvariant(),
            ["topic"] = topic?.ToString().ToLowerInvariant(),
            ["time_range"] = timeRange?.ToString().ToLowerInvariant(),
            ["days"] = days,
            ["max_results"] = maxResults,
            ["include_domains"] = includeDomains?.ToList(),
            ["exclude_domains"] = excludeDomains?.ToList(),
            ["include_answer"] = includeAnswer,
            ["include_raw_content"] = includeRawContent,
            ["include_images"] = includeImages,
            ["country"] = country,
            ["include_favicon"] = includeFavicon
        };

        // Remove null values
        var filteredData = requestData.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return await SendRequestAsync<TavilySearchResult>("/search", filteredData, Math.Min(timeout, 120), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string> GetSearchContextAsync(
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
        CancellationToken cancellationToken = default)
    {
        var searchResult = await SearchAsync(
            query,
            searchDepth,
            topic,
            days: days,
            maxResults: maxResults,
            includeDomains: includeDomains,
            excludeDomains: excludeDomains,
            includeAnswer: false,
            includeRawContent: false,
            includeImages: false,
            timeout: timeout,
            country: country,
            cancellationToken: cancellationToken);

        var context = searchResult.Results.Select(r => new { url = r.Url, content = r.Content }).ToList();
        
        // Simple implementation - in a real scenario, you might want to implement token limiting
        return JsonSerializer.Serialize(context, _jsonOptions);
    }

    /// <inheritdoc />
    public async Task<string> QnaSearchAsync(
        string query,
        TavilySearchDepth searchDepth = TavilySearchDepth.Advanced,
        TavilyTopic topic = TavilyTopic.General,
        int days = 7,
        int maxResults = 5,
        IEnumerable<string>? includeDomains = null,
        IEnumerable<string>? excludeDomains = null,
        int timeout = 60,
        string? country = null,
        CancellationToken cancellationToken = default)
    {
        var searchResult = await SearchAsync(
            query,
            searchDepth,
            topic,
            days: days,
            maxResults: maxResults,
            includeDomains: includeDomains,
            excludeDomains: excludeDomains,
            includeAnswer: true,
            includeRawContent: false,
            includeImages: false,
            timeout: timeout,
            country: country,
            cancellationToken: cancellationToken);

        return searchResult.Answer ?? string.Empty;
    }

    /// <inheritdoc />
    public async Task<TavilyExtractResult> ExtractAsync(
        IEnumerable<string> urls,
        bool? includeImages = null,
        TavilyExtractDepth? extractDepth = null,
        TavilyContentFormat? format = null,
        int timeout = 60,
        bool? includeFavicon = null,
        CancellationToken cancellationToken = default)
    {
        var requestData = new Dictionary<string, object?>
        {
            ["urls"] = urls.ToList(),
            ["include_images"] = includeImages,
            ["extract_depth"] = extractDepth?.ToString().ToLowerInvariant(),
            ["format"] = format?.ToString().ToLowerInvariant(),
            ["include_favicon"] = includeFavicon
        };

        var filteredData = requestData.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return await SendRequestAsync<TavilyExtractResult>("/extract", filteredData, Math.Min(timeout, 120), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TavilyCrawlResult> CrawlAsync(
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
        CancellationToken cancellationToken = default)
    {
        var requestData = new Dictionary<string, object?>
        {
            ["url"] = url,
            ["max_depth"] = maxDepth,
            ["max_breadth"] = maxBreadth,
            ["limit"] = limit,
            ["instructions"] = instructions,
            ["select_paths"] = selectPaths?.ToList(),
            ["select_domains"] = selectDomains?.ToList(),
            ["exclude_paths"] = excludePaths?.ToList(),
            ["exclude_domains"] = excludeDomains?.ToList(),
            ["allow_external"] = allowExternal,
            ["categories"] = categories?.Select(c => c.ToString().ToLowerInvariant()).ToList(),
            ["extract_depth"] = extractDepth?.ToString().ToLowerInvariant(),
            ["include_images"] = includeImages,
            ["format"] = format?.ToString().ToLowerInvariant(),
            ["include_favicon"] = includeFavicon
        };

        var filteredData = requestData.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return await SendRequestAsync<TavilyCrawlResult>("/crawl", filteredData, Math.Min(timeout, 120), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TavilyMapResult> MapAsync(
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
        CancellationToken cancellationToken = default)
    {
        var requestData = new Dictionary<string, object?>
        {
            ["url"] = url,
            ["max_depth"] = maxDepth,
            ["max_breadth"] = maxBreadth,
            ["limit"] = limit,
            ["instructions"] = instructions,
            ["select_paths"] = selectPaths?.ToList(),
            ["select_domains"] = selectDomains?.ToList(),
            ["exclude_paths"] = excludePaths?.ToList(),
            ["exclude_domains"] = excludeDomains?.ToList(),
            ["allow_external"] = allowExternal,
            ["include_images"] = includeImages,
            ["categories"] = categories?.Select(c => c.ToString().ToLowerInvariant()).ToList()
        };

        var filteredData = requestData.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return await SendRequestAsync<TavilyMapResult>("/map", filteredData, Math.Min(timeout, 120), cancellationToken);
    }

    private async Task<T> SendRequestAsync<T>(string endpoint, Dictionary<string, object?> data, int timeoutSeconds, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            using var response = await _httpClient.PostAsync(endpoint, content, cts.Token);
            var responseContent = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                return result ?? throw new InvalidOperationException("Failed to deserialize response");
            }

            // Handle error responses
            var exception = GetExceptionForResponse(response, responseContent);
            throw exception;
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new RequestTimeoutException(timeoutSeconds);
        }
    }

    private static Exception GetExceptionForResponse(HttpResponseMessage response, string responseContent)
    {
        string? errorDetail = null;
        
        try
        {
            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (errorResponse.TryGetProperty("detail", out var detailElement))
            {
                if (detailElement.TryGetProperty("error", out var errorElement))
                {
                    errorDetail = errorElement.GetString();
                }
                else if (detailElement.ValueKind == JsonValueKind.String)
                {
                    errorDetail = detailElement.GetString();
                }
            }
        }
        catch
        {
            // Ignore JSON parsing errors, use status code instead
        }

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => new InvalidApiKeyException(errorDetail),
            System.Net.HttpStatusCode.Forbidden => new ForbiddenException(errorDetail),
            System.Net.HttpStatusCode.BadRequest => new BadRequestException(errorDetail),
            System.Net.HttpStatusCode.TooManyRequests => new UsageLimitExceededException(errorDetail),
            _ => new HttpRequestException($"Request failed with status {response.StatusCode}: {errorDetail ?? responseContent}")
        };
    }
}
