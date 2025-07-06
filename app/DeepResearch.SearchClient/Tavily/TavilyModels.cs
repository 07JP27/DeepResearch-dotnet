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