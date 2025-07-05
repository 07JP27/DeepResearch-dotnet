# DeepResearch.SearchClient

This library provides a unified search client interface (`ISearchClient`) and implementations for various web search services. Currently includes Tavily API client implementation, with support for additional search providers planned.

## Features

The search client library provides:

- **Unified Interface**: `ISearchClient` for consistent search operations across different providers
- **Tavily Implementation**: Comprehensive Tavily API client with advanced search capabilities
- **Extensible Design**: Easy to add new search service implementations

### Tavily Client Features

- **Search**: Comprehensive web search with advanced filtering and result options
- **Q&A Search**: Get direct answers to questions
- **Search Context**: Generate context optimized for RAG (Retrieval-Augmented Generation) applications
- **Extract**: Extract content from specific URLs
- **Crawl**: Crawl websites starting from a base URL (Beta feature)
- **Map**: Map website structure and discover pages (Beta feature)

## Installation

This library is part of the DeepResearch project. Make sure you have the following NuGet packages:

```xml
<PackageReference Include="Microsoft.Extensions.Http" Version="9.0.0" />
```

## Quick Start

### Using the Unified Interface

The recommended way to use search functionality is through the `ISearchClient` interface:

```csharp
using DeepResearch.SearchClient;
using DeepResearch.SearchClient.Tavily;

// Create Tavily-based search client
var httpClient = new HttpClient();
var tavilyClient = new TavilyClient(httpClient);
var searchClient = new TavilySearchClient(tavilyClient);

// Use the unified interface
var result = await searchClient.SearchAsync(
    "artificial intelligence trends 2024", 
    maxResults: 10);

Console.WriteLine($"Found {result.Results.Count} results");
foreach (var item in result.Results)
{
    Console.WriteLine($"Title: {item.Title}");
    Console.WriteLine($"URL: {item.Url}");
    Console.WriteLine("---");
}
```

### Using Tavily Client Directly

For advanced Tavily-specific features, you can use the Tavily client directly:

```csharp
using DeepResearch.SearchClient.Tavily;

// Set up your API Key
Environment.SetEnvironmentVariable("TAVILY_API_KEY", "tvly-your-api-key-here");
// Or pass it directly: new TavilyClient(httpClient, "your-api-key")

// Create HTTP client and Tavily client
var httpClient = new HttpClient();
var tavilyClient = new TavilyClient(httpClient);

// Perform a basic search
var result = await tavilyClient.SearchAsync(
    "latest developments in artificial intelligence 2024",
    maxResults: 5,
    searchDepth: TavilySearchDepth.Advanced);

Console.WriteLine($"Found {result.Results.Count} results");
foreach (var item in result.Results)
{
    Console.WriteLine($"Title: {item.Title}");
    Console.WriteLine($"URL: {item.Url}");
    Console.WriteLine($"Content: {item.Content}");
    Console.WriteLine("---");
}
```

## API Reference

### ISearchClient Interface

The unified search interface provides a consistent API across different search providers:

```csharp
public interface ISearchClient
{
    Task<SearchResult> SearchAsync(string query, int maxResults = 5, CancellationToken cancellationToken = default);
}
```

### Tavily-Specific Methods

#### SearchAsync
Performs a comprehensive web search with various filtering options.

```csharp
var result = await tavilyClient.SearchAsync(
    query: "C# programming best practices",
    searchDepth: TavilySearchDepth.Advanced,
    topic: TavilyTopic.General,
    maxResults: 10,
    includeDomains: new[] { "docs.microsoft.com", "stackoverflow.com" },
    includeAnswer: true,
    includeRawContent: true,
    includeImages: true);
```

#### QnaSearchAsync
Get a direct answer to a question.

```csharp
var answer = await tavilyClient.QnaSearchAsync(
    "What are the benefits of using .NET 9?",
    searchDepth: TavilySearchDepth.Advanced);
```

#### GetSearchContextAsync
Generate context optimized for RAG applications.

```csharp
var context = await tavilyClient.GetSearchContextAsync(
    "machine learning algorithms",
    maxResults: 5,
    maxTokens: 4000);
```

### Content Extraction

#### ExtractAsync
Extract content from specific URLs.

```csharp
var urls = new[] 
{
    "https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12",
    "https://devblogs.microsoft.com/dotnet/"
};

var result = await tavilyClient.ExtractAsync(
    urls: urls,
    includeImages: false,
    extractDepth: TavilyExtractDepth.Advanced,
    format: TavilyContentFormat.Markdown);
```

### Website Exploration (Beta Features)

#### CrawlAsync
Crawl a website starting from a base URL.

```csharp
var result = await tavilyClient.CrawlAsync(
    url: "https://docs.microsoft.com",
    maxDepth: 3,
    limit: 50,
    instructions: "Find documentation about C# features",
    includeImages: false);
```

#### MapAsync
Map the structure of a website.

```csharp
var result = await tavilyClient.MapAsync(
    url: "https://docs.microsoft.com",
    maxDepth: 2,
    limit: 30,
    instructions: "Map the documentation structure");
```

## Configuration Options

### Search Depth
- `TavilySearchDepth.Basic`: Faster, fewer sources
- `TavilySearchDepth.Advanced`: More comprehensive, higher quality

### Topics
- `TavilyTopic.General`: General web search
- `TavilyTopic.News`: News-focused search
- `TavilyTopic.Finance`: Finance-focused search

### Time Ranges
- `TavilyTimeRange.Day`: Last 24 hours
- `TavilyTimeRange.Week`: Last week
- `TavilyTimeRange.Month`: Last month
- `TavilyTimeRange.Year`: Last year

### Content Formats
- `TavilyContentFormat.Text`: Plain text
- `TavilyContentFormat.Markdown`: Markdown format

## Error Handling

When using `ISearchClient` implementations, standard .NET exceptions may be thrown:

```csharp
try
{
    var result = await searchClient.SearchAsync("query");
}
catch (InvalidOperationException ex)
{
    // Handle configuration or API key issues
}
catch (HttpRequestException ex)
{
    // Handle network-related errors
}
catch (TaskCanceledException ex)
{
    // Handle timeout errors
}
```

### Search Client-Specific Exceptions

When using search clients directly, you can catch provider-specific exceptions:

```csharp
try
{
    var result = await tavilyClient.SearchAsync("query");
}
catch (MissingApiKeyException)
{
    // Handle missing API key
}
catch (InvalidApiKeyException)
{
    // Handle invalid API key
}
catch (UsageLimitExceededException)
{
    // Handle usage limit exceeded
}
catch (RequestTimeoutException)
{
    // Handle request timeout
}
catch (BadRequestException)
{
    // Handle bad request
}
catch (ForbiddenException)
{
    // Handle forbidden access
}
```

## Dependency Injection

For applications using dependency injection, you can register the search clients:

```csharp
// Register Tavily as the search client implementation
services.AddHttpClient<TavilyClient>();
services.AddScoped<ITavilyClient, TavilyClient>();
services.AddScoped<ISearchClient, TavilySearchClient>();

// Or register multiple implementations if needed
services.AddScoped<TavilySearchClient>();
// services.AddScoped<OtherSearchClient>(); // Future implementations
```