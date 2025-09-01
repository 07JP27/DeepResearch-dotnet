using ModelContextProtocol.Client;
using System.Text.Json;

namespace DeepResearch.SearchClient.Mcp;

/// <summary>
/// MCP client implementation for search operations
/// </summary>
public class SearchMcpClient : IMcpClient
{
    private readonly McpServerConfig _config;
    private object? _mcpClientInstance;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the MCP client
    /// </summary>
    /// <param name="config">MCP server configuration</param>
    public SearchMcpClient(McpServerConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <inheritdoc />
    public bool IsConnected => _mcpClientInstance != null;

    /// <inheritdoc />
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_mcpClientInstance != null)
            return;

        var clientTransport = new StdioClientTransport(new()
        {
            Name = _config.Name,
            Command = _config.Command,
            Arguments = _config.Arguments,
        });

        _mcpClientInstance = await McpClientFactory.CreateAsync(clientTransport);
    }

    /// <inheritdoc />
    public Task<McpSearchResult> SearchAsync(string query, int maxResults, CancellationToken cancellationToken = default)
    {
        if (_mcpClientInstance == null)
            throw new InvalidOperationException("Client is not connected. Call ConnectAsync first.");

        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));

        try
        {
            // For now, return a mock result until we can access the MCP client properly
            // This will be updated once we resolve the type access issues
            return Task.FromResult(new McpSearchResult
            {
                Results = [
                    new McpSearchResultItem
                    {
                        Title = $"MCP Search Result for: {query}",
                        Url = "https://example.com",
                        Content = $"Search results for '{query}' (max {maxResults} results)",
                        RawContent = $"Raw content for '{query}'"
                    }
                ],
                Images = []
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to perform search via MCP: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_mcpClientInstance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _disposed = true;
        }
    }
}