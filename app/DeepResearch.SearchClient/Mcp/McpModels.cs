namespace DeepResearch.SearchClient.Mcp;

/// <summary>
/// Configuration for MCP server connection
/// </summary>
public class McpServerConfig
{
    /// <summary>
    /// Server name or identifier
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Command to execute the server (e.g., "python", "node", "dotnet")
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Arguments to pass to the server command
    /// </summary>
    public string[] Arguments { get; set; } = [];

    /// <summary>
    /// Timeout for server operations in milliseconds
    /// </summary>
    public int TimeoutMs { get; set; } = 30000;
}

/// <summary>
/// Result from MCP search operation
/// </summary>
public class McpSearchResult
{
    /// <summary>
    /// Search results
    /// </summary>
    public List<McpSearchResultItem> Results { get; set; } = [];

    /// <summary>
    /// Associated images
    /// </summary>
    public List<string> Images { get; set; } = [];
}

/// <summary>
/// Individual search result item from MCP
/// </summary>
public class McpSearchResultItem
{
    /// <summary>
    /// Title of the result
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// URL of the result
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Content/snippet of the result
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Raw content if available
    /// </summary>
    public string RawContent { get; set; } = string.Empty;
}