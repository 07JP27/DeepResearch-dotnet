using DeepResearch.SearchClient.Mcp;
using FluentAssertions;

namespace DeepResearch.Test.Tests;

public class SearchMcpClientTests
{
    [Fact]
    public void Constructor_WithValidConfig_ShouldSucceed()
    {
        // Arrange
        var config = new McpServerConfig
        {
            Name = "Test Server",
            Command = "python",
            Arguments = ["server.py"]
        };

        // Act
        var client = new SearchMcpClient(config);

        // Assert
        client.Should().NotBeNull();
        client.IsConnected.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithNullConfig_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new SearchMcpClient(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("config");
    }

    [Fact]
    public async Task SearchAsync_WithoutConnection_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var config = new McpServerConfig
        {
            Name = "Test Server",
            Command = "python",
            Arguments = ["server.py"]
        };
        var client = new SearchMcpClient(config);

        // Act & Assert
        var act = async () => await client.SearchAsync("test query", 10);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Client is not connected. Call ConnectAsync first.");
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ShouldThrowArgumentException()
    {
        // Arrange
        var config = new McpServerConfig
        {
            Name = "Test Server",
            Command = "python",
            Arguments = ["server.py"]
        };
        var client = new SearchMcpClient(config);

        // Mock the connection state by using reflection to set the internal field
        var field = typeof(SearchMcpClient).GetField("_mcpClientInstance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(client, new object()); // Set a dummy object to simulate connection

        // Act & Assert
        var act = async () => await client.SearchAsync("", 10);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("query");
    }

    [Fact]
    public void McpServerConfig_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var config = new McpServerConfig();

        // Assert
        config.Name.Should().Be(string.Empty);
        config.Command.Should().Be(string.Empty);
        config.Arguments.Should().BeEmpty();
        config.TimeoutMs.Should().Be(30000);
    }

    [Fact]
    public void McpSearchResult_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var result = new McpSearchResult();

        // Assert
        result.Results.Should().BeEmpty();
        result.Images.Should().BeEmpty();
    }

    [Fact]
    public void McpSearchResultItem_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var item = new McpSearchResultItem();

        // Assert
        item.Title.Should().Be(string.Empty);
        item.Url.Should().Be(string.Empty);
        item.Content.Should().Be(string.Empty);
        item.RawContent.Should().Be(string.Empty);
    }
}