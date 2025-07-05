using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeepResearch.SearchClient.Tavily;

/// <summary>
/// Represents a search result item from Tavily
/// </summary>
public class TavilySearchResultItem
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("raw_content")]
    public string? RawContent { get; set; }

    [JsonPropertyName("score")]
    public double Score { get; set; }
}

/// <summary>
/// Represents the complete search result from Tavily
/// </summary>
public class TavilySearchResult
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("answer")]
    public string? Answer { get; set; }

    [JsonPropertyName("results")]
    public List<TavilySearchResultItem> Results { get; set; } = new();

    [JsonPropertyName("images")]
    public List<string> Images { get; set; } = new();

    [JsonPropertyName("response_time")]
    public double ResponseTime { get; set; }
}

/// <summary>
/// Represents an extracted content result from a URL
/// </summary>
public class TavilyExtractResultItem
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("raw_content")]
    public string RawContent { get; set; } = string.Empty;

    [JsonPropertyName("images")]
    public List<string> Images { get; set; } = new();
}

/// <summary>
/// Represents the result of extracting content from URLs
/// </summary>
public class TavilyExtractResult
{
    [JsonPropertyName("results")]
    public List<TavilyExtractResultItem> Results { get; set; } = new();

    [JsonPropertyName("failed_results")]
    public List<string> FailedResults { get; set; } = new();
}

/// <summary>
/// Represents a crawl result item
/// </summary>
public class TavilyCrawlResultItem
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("raw_content")]
    public string RawContent { get; set; } = string.Empty;

    [JsonPropertyName("images")]
    public List<string> Images { get; set; } = new();
}

/// <summary>
/// Represents the result of crawling a website
/// </summary>
public class TavilyCrawlResult
{
    [JsonPropertyName("results")]
    public List<TavilyCrawlResultItem> Results { get; set; } = new();

    [JsonPropertyName("failed_results")]
    public List<string> FailedResults { get; set; } = new();
}

/// <summary>
/// Represents a map result item showing website structure
/// </summary>
public class TavilyMapResultItem
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("depth")]
    public int Depth { get; set; }
}

/// <summary>
/// Represents the result of mapping a website structure
/// </summary>
public class TavilyMapResult
{
    [JsonPropertyName("results")]
    public List<TavilyMapResultItem> Results { get; set; } = new();
}

/// <summary>
/// Search depth options for Tavily API
/// </summary>
public enum TavilySearchDepth
{
    Basic,
    Advanced
}

/// <summary>
/// Topic options for search
/// </summary>
public enum TavilyTopic
{
    General,
    News,
    Finance
}

/// <summary>
/// Time range options for search
/// </summary>
public enum TavilyTimeRange
{
    Day,
    Week,
    Month,
    Year
}

/// <summary>
/// Extract depth options
/// </summary>
public enum TavilyExtractDepth
{
    Basic,
    Advanced
}

/// <summary>
/// Content format options
/// </summary>
public enum TavilyContentFormat
{
    Markdown,
    Text
}

/// <summary>
/// Available categories for website filtering
/// </summary>
public enum TavilyCategory
{
    Documentation,
    Blog,
    Blogs,
    Community,
    About,
    Contact,
    Privacy,
    Terms,
    Status,
    Pricing,
    Enterprise,
    Careers,
    ECommerce,
    Authentication,
    Developer,
    Developers,
    Solutions,
    Partners,
    Downloads,
    Media,
    Events,
    People
}
