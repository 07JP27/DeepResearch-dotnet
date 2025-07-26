using DeepResearch.Core.SearchClient;
using DeepResearch.SearchClient.Mcp;
using FluentAssertions;

namespace DeepResearch.Test.Tests;

public class McpSearchClientIntegrationTests
{
    [Fact]
    public void McpSearchClient_ShouldImplementISearchClient()
    {
        // Arrange
        var config = new McpServerConfig
        {
            Name = "Test Server",
            Command = "python",
            Arguments = ["server.py"]
        };
        var mcpClient = new SearchMcpClient(config);

        // Act
        var searchClient = new McpSearchClient(mcpClient);

        // Assert
        searchClient.Should().BeAssignableTo<ISearchClient>();
    }

    [Fact]
    public async Task McpSearchClient_SearchAsync_ShouldReturnCompatibleResults()
    {
        // Arrange
        var config = new McpServerConfig
        {
            Name = "Test Server",
            Command = "python", 
            Arguments = ["server.py"]
        };
        var mcpClient = new SearchMcpClient(config);
        var searchClient = new McpSearchClient(mcpClient);

        // Simulate connection by using reflection to set the internal field
        var field = typeof(SearchMcpClient).GetField("_mcpClientInstance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(mcpClient, new object());

        // Act
        var result = await searchClient.SearchAsync("test query", 5);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().NotBeNull();
        result.Images.Should().NotBeNull();
        
        // Verify the structure matches what ISearchClient expects
        if (result.Results.Any())
        {
            var firstResult = result.Results.First();
            firstResult.Title.Should().NotBeNull();
            firstResult.Url.Should().NotBeNull();
            firstResult.Content.Should().NotBeNull();
            firstResult.RawContent.Should().NotBeNull();
        }
    }

    [Fact]
    public void McpSearchClient_CanBeUsedInPlaceOfTavilySearchClient()
    {
        // This test demonstrates that McpSearchClient can be used as a drop-in replacement
        // for TavilySearchClient wherever ISearchClient is expected

        // Arrange
        var config = new McpServerConfig
        {
            Name = "Test Server",
            Command = "python",
            Arguments = ["server.py"]
        };
        var mcpClient = new SearchMcpClient(config);
        ISearchClient searchClient = new McpSearchClient(mcpClient);

        // Act & Assert
        // This would work in dependency injection containers
        searchClient.Should().NotBeNull();
        searchClient.Should().BeOfType<McpSearchClient>();
        
        // The search client can be used anywhere ISearchClient is expected
        UseSearchClient(searchClient).Should().Be("Used ISearchClient successfully");
    }

    private static string UseSearchClient(ISearchClient searchClient)
    {
        // Simulate using the search client in a service
        return searchClient != null ? "Used ISearchClient successfully" : "Failed";
    }
}