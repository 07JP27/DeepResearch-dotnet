using DeepResearch.SearchClient.Mcp;
using FluentAssertions;
using Moq;

namespace DeepResearch.Test.Tests;

public class McpSearchClientTests
{
    [Fact]
    public void Constructor_WithValidMcpClient_ShouldSucceed()
    {
        // Arrange
        var mockMcpClient = new Mock<IMcpClient>();

        // Act
        var searchClient = new McpSearchClient(mockMcpClient.Object);

        // Assert
        searchClient.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullMcpClient_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new McpSearchClient(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("mcpClient");
    }

    [Fact]
    public async Task SearchAsync_WithValidParameters_ShouldReturnSearchResult()
    {
        // Arrange
        var mockMcpClient = new Mock<IMcpClient>();
        var mcpResult = new McpSearchResult
        {
            Results = [
                new McpSearchResultItem
                {
                    Title = "Test Title",
                    Url = "https://example.com",
                    Content = "Test content",
                    RawContent = "Test raw content"
                }
            ],
            Images = ["https://example.com/image.jpg"]
        };

        mockMcpClient.Setup(x => x.IsConnected).Returns(true);
        mockMcpClient.Setup(x => x.SearchAsync("test query", 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mcpResult);

        var searchClient = new McpSearchClient(mockMcpClient.Object);

        // Act
        var result = await searchClient.SearchAsync("test query", 10);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(1);
        result.Results[0].Title.Should().Be("Test Title");
        result.Results[0].Url.Should().Be("https://example.com");
        result.Results[0].Content.Should().Be("Test content");
        result.Results[0].RawContent.Should().Be("Test raw content");
        result.Images.Should().HaveCount(1);
        result.Images[0].Should().Be("https://example.com/image.jpg");
    }

    [Fact]
    public async Task SearchAsync_WhenNotConnected_ShouldCallConnectAsync()
    {
        // Arrange
        var mockMcpClient = new Mock<IMcpClient>();
        var mcpResult = new McpSearchResult { Results = [], Images = [] };

        mockMcpClient.SetupSequence(x => x.IsConnected)
            .Returns(false)
            .Returns(true);
        mockMcpClient.Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mockMcpClient.Setup(x => x.SearchAsync("test query", 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mcpResult);

        var searchClient = new McpSearchClient(mockMcpClient.Object);

        // Act
        await searchClient.SearchAsync("test query", 10);

        // Assert
        mockMcpClient.Verify(x => x.ConnectAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockMcpClient.Verify(x => x.SearchAsync("test query", 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WhenMcpClientThrowsException_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var mockMcpClient = new Mock<IMcpClient>();
        mockMcpClient.Setup(x => x.IsConnected).Returns(true);
        mockMcpClient.Setup(x => x.SearchAsync("test query", 10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("MCP error"));

        var searchClient = new McpSearchClient(mockMcpClient.Object);

        // Act & Assert
        var act = async () => await searchClient.SearchAsync("test query", 10);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to perform web search via MCP");
    }
}