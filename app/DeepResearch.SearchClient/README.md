# Tavily Client for C# (.NET)

This is a C# implementation of the Tavily API client, providing comprehensive web search and content extraction capabilities. This client is compatible with the existing `IWebSearchClient` interface used in the DeepResearch project.

## Features

The C# Tavily client supports all major Tavily API features:

- **Search**: Comprehensive web search with advanced filtering and result options
- **Q&A Search**: Get direct answers to questions
- **Search Context**: Generate context optimized for RAG (Retrieval-Augmented Generation) applications
- **Extract**: Extract content from specific URLs
- **Crawl**: Crawl websites starting from a base URL (Beta feature)
- **Map**: Map website structure and discover pages (Beta feature)

## Installation

The Tavily client is included in the `DeepResearch.Core` project. Make sure you have the following NuGet packages:

```xml
<PackageReference Include="Microsoft.Extensions.Http" Version="9.0.0" />
```

## Quick Start

### 1. Set up your API Key

Set your Tavily API key as an environment variable:

```bash
export TAVILY_API_KEY="tvly-your-api-key-here"
```

Or pass it directly when creating the client.

### 2. Basic Usage

```csharp
using DeepResearch.Core.Clients.Tavily;

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

### 3. Using with IWebSearchClient Interface

The Tavily client can be used as a drop-in replacement for any existing `IWebSearchClient` implementation:

```csharp
using DeepResearch.Core.Clients;
using DeepResearch.Core.Clients.Tavily;

// Create Tavily-based web search client
var httpClient = new HttpClient();
var tavilyClient = new TavilyClient(httpClient);
var webSearchClient = new TavilyWebSearchClient(tavilyClient);

// Use with existing DeepResearch service
var service = new DeepResearchService(chatClient, webSearchClient);
```

## API Reference

### Search Methods

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

The client provides specific exception types for different error conditions:

```csharp
try
{
    var result = await tavilyClient.SearchAsync("query");
}
catch (TavilyInvalidApiKeyException)
{
    // Handle invalid API key
}
catch (TavilyUsageLimitExceededException)
{
    // Handle usage limit exceeded
}
catch (TavilyTimeoutException)
{
    // Handle request timeout
}
catch (TavilyBadRequestException)
{
    // Handle bad request
}
catch (TavilyForbiddenException)
{
    // Handle forbidden access
}
```

## Dependency Injection

For applications using dependency injection:

```csharp
services.AddHttpClient<TavilyClient>();
services.AddScoped<ITavilyClient, TavilyClient>();
services.AddScoped<IWebSearchClient, TavilyWebSearchClient>();
```

## Running the Demo

To see the Tavily client in action, run the demo:

```bash
# Set your API key
export TAVILY_API_KEY="tvly-your-api-key-here"

# Run the demo
dotnet run --project DeepResearch.Console -- --tavily-demo
```

The demo showcases:
1. Basic web search
2. Q&A search for direct answers
3. Context generation for RAG applications
4. Integration with IWebSearchClient interface
5. Content extraction from URLs