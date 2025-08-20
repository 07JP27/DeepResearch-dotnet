namespace DeepResearch.SearchClient.Mcp;

/// <summary>
/// Interface for MCP client operations
/// </summary>
public interface IMcpClient : IDisposable
{
    /// <summary>
    /// Connect to the MCP server
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Search using the MCP server
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="maxResults">Maximum number of results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results</returns>
    Task<McpSearchResult> SearchAsync(string query, int maxResults, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the client is connected
    /// </summary>
    bool IsConnected { get; }
}