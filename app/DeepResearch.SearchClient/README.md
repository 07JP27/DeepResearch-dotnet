[日本語はこちら](README_ja.md)

# DeepResearch.SearchClient

This library provides implementations of the search client interface (`ISearchClient`) defined in DeepResearch.Core for various web search services. Currently, it includes the Tavily API client implementation.

## Features

This search client library provides the following features:

- **Unified Interface**: `ISearchClient` (defined in DeepResearch.Core) enables consistent search operations across multiple providers
- **Tavily Implementation**: Tavily API client with advanced search capabilities
- **Highly Extensible Design**: Easy to add new search service implementations

## Installation

This library requires the following NuGet package:

```xml
<PackageReference Include="Microsoft.Extensions.Http" Version="9.0.0" />
```

## Quick Start

The recommended way to use the search functionality is through the `ISearchClient` interface:

```csharp
using DeepResearch.Core.SearchClient;
using DeepResearch.SearchClient.Tavily;

// Create a Tavily-based search client
var httpClient = new HttpClient();
var tavilyClient = new TavilyClient(httpClient);
var searchClient = new TavilySearchClient(tavilyClient);

// Use the unified interface
var result = await searchClient.SearchAsync(
    "AI trends in 2025",
    maxResults: 10);

Console.WriteLine($"{result.Results.Count} results found");
foreach (var item in result.Results)
{
    Console.WriteLine($"Title: {item.Title}");
    Console.WriteLine($"URL: {item.Url}");
    Console.WriteLine("---");
}
```

## ISearchClient Interface

The unified search interface provides a consistent API across different search providers:

```csharp
public interface ISearchClient
{
    Task<SearchResult> SearchAsync(string query, int maxResults, CancellationToken cancellationToken = default);
}
```

## Dependency Injection

For applications using dependency injection, you can register the search clients as follows:

```csharp
// Register Tavily as the search client implementation
services.AddHttpClient<TavilyClient>();
services.AddScoped<ITavilyClient, TavilyClient>();
services.AddScoped<ISearchClient, TavilySearchClient>();
```
