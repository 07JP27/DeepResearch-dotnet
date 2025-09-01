using DeepResearch.Core.SearchClient;

namespace DeepResearch.SearchClient.Mcp;

/// <summary>
/// Search client implementation using MCP (Model Context Protocol)
/// </summary>
public class McpSearchClient : ISearchClient
{
    private readonly IMcpClient _mcpClient;

    /// <summary>
    /// Initializes a new instance of the McpSearchClient
    /// </summary>
    /// <param name="mcpClient">MCP client instance</param>
    public McpSearchClient(IMcpClient mcpClient)
    {
        _mcpClient = mcpClient ?? throw new ArgumentNullException(nameof(mcpClient));
    }

    /// <inheritdoc />
    public async Task<SearchResult> SearchAsync(string query, int maxResults, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure connection is established
            if (!_mcpClient.IsConnected)
            {
                await _mcpClient.ConnectAsync(cancellationToken);
            }

            var mcpResult = await _mcpClient.SearchAsync(query, maxResults, cancellationToken);

            // Convert MCP result to SearchResult
            var searchResult = new SearchResult
            {
                Results = mcpResult.Results.Select(r => new SearchResultItem
                {
                    Title = r.Title,
                    Url = r.Url,
                    Content = r.Content,
                    RawContent = r.RawContent
                }).ToList(),
                Images = mcpResult.Images
            };

            return searchResult;
        }
        catch (ArgumentException)
        {
            throw; // Re-throw argument exceptions as-is
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw invalid operation exceptions as-is
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation exceptions as-is
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to perform web search via MCP", ex);
        }
    }
}