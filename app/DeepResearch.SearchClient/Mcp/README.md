# MCP SearchClient

MCP (Model Context Protocol) SearchClient implementation for DeepResearch.

## Overview

The MCP SearchClient provides an implementation of `ISearchClient` that allows connecting to MCP-compatible search servers. This enables easy switching between different search sources without changing the core application logic.

## Usage

### Basic Setup

```csharp
using DeepResearch.SearchClient.Mcp;

// Configure the MCP server
var config = new McpServerConfig
{
    Name = "Search Server",
    Command = "python",  // or "node", "dotnet", etc.
    Arguments = ["search-server.py"],
    TimeoutMs = 30000
};

// Create the MCP client
var mcpClient = new SearchMcpClient(config);

// Create the search client
var searchClient = new McpSearchClient(mcpClient);

// Use the search client
var results = await searchClient.SearchAsync("your search query", maxResults: 10);
```

### Configuration Options

The `McpServerConfig` class provides the following configuration options:

- **Name**: A descriptive name for the MCP server
- **Command**: The command to execute the server (e.g., "python", "node", "dotnet")
- **Arguments**: Arguments to pass to the server command
- **TimeoutMs**: Timeout for server operations in milliseconds (default: 30000)

### Integration with Dependency Injection

```csharp
// Register in your DI container
services.AddSingleton<McpServerConfig>(new McpServerConfig
{
    Name = "Search Server",
    Command = "python",
    Arguments = ["search-server.py"]
});

services.AddSingleton<IMcpClient, SearchMcpClient>();
services.AddSingleton<ISearchClient, McpSearchClient>();
```

## MCP Server Requirements

The MCP server should provide a search tool that accepts the following parameters:

- `query` (string): The search query
- `max_results` or `maxResults` or `limit` (integer): Maximum number of results to return

The server should return results in a format that can be parsed into:

```json
{
  "results": [
    {
      "title": "Result Title",
      "url": "https://example.com",
      "content": "Result content/snippet",
      "raw_content": "Full content (optional)"
    }
  ],
  "images": [
    "https://example.com/image1.jpg",
    "https://example.com/image2.jpg"
  ]
}
```

## Error Handling

The MCP SearchClient provides proper error handling:

- Connection errors are wrapped in `InvalidOperationException`
- Invalid arguments throw `ArgumentException`
- General search failures throw `InvalidOperationException` with descriptive messages

## Dependencies

- ModelContextProtocol (>= 0.3.0-preview.3)
- Microsoft.Extensions.AI (>= 9.7.1)

## Development Notes

The current implementation includes a mock response mechanism for development and testing purposes. In a production environment, this should be replaced with actual MCP server communication once the server protocol is fully implemented.