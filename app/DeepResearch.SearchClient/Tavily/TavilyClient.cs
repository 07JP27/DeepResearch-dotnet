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
                  throw new ArgumentException("No API key provided. Please provide the API key or set the TAVILY_API_KEY environment variable.");

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
            throw new TimeoutException($"Request timed out after {timeoutSeconds} seconds.");
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
            System.Net.HttpStatusCode.Unauthorized => new UnauthorizedAccessException(errorDetail ?? "Invalid API key"),
            System.Net.HttpStatusCode.Forbidden => new UnauthorizedAccessException(errorDetail ?? "Access forbidden"),
            System.Net.HttpStatusCode.BadRequest => new ArgumentException(errorDetail ?? "Bad request"),
            System.Net.HttpStatusCode.TooManyRequests => new InvalidOperationException(errorDetail ?? "Usage limit exceeded"),
            _ => new HttpRequestException($"Request failed with status {response.StatusCode}: {errorDetail ?? responseContent}")
        };
    }
}
