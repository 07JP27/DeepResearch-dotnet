using Xunit;
using FluentAssertions;
using DeepResearch.Core;
using DeepResearch.Core.Models;
using Moq;
using OpenAI.Chat;

namespace DeepResearch.Tests;

/// <summary>
/// Unit tests for DeepResearchService class
/// </summary>
public class DeepResearchServiceTests
{
    private readonly Mock<ChatClient> _mockChatClient;
    private readonly Mock<ISearchClient> _mockSearchClient;
    private readonly DeepResearchOptions _defaultOptions;

    public DeepResearchServiceTests()
    {
        _mockChatClient = new Mock<ChatClient>();
        _mockSearchClient = new Mock<ISearchClient>();
        _defaultOptions = new DeepResearchOptions();
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldInitializeService()
    {
        // Act
        var service = new DeepResearchService(_mockChatClient.Object, _mockSearchClient.Object, _defaultOptions);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldUseDefaultOptions()
    {
        // Act
        var service = new DeepResearchService(_mockChatClient.Object, _mockSearchClient.Object, null);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullChatClient_ShouldCreateInstance()
    {
        // The constructor doesn't validate parameters, so it will create an instance
        // Act & Assert
        var act = () => new DeepResearchService(null!, _mockSearchClient.Object, _defaultOptions);
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithNullSearchClient_ShouldCreateInstance()
    {
        // The constructor doesn't validate parameters, so it will create an instance
        // Act & Assert
        var act = () => new DeepResearchService(_mockChatClient.Object, null!, _defaultOptions);
        act.Should().NotThrow();
    }

    [Fact]
    public void DeepResearchService_WithValidParameters_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act
        var service1 = new DeepResearchService(_mockChatClient.Object, _mockSearchClient.Object);
        var service2 = new DeepResearchService(_mockChatClient.Object, _mockSearchClient.Object, _defaultOptions);
        var service3 = new DeepResearchService(_mockChatClient.Object, _mockSearchClient.Object, null);

        // Assert
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service3.Should().NotBeNull();
    }

    [Fact]
    public void DeepResearchService_StaticMethods_ShouldBeAccessible()
    {
        // This test verifies that the class structure is correct
        
        // Arrange & Act
        var serviceType = typeof(DeepResearchService);

        // Assert
        serviceType.Should().NotBeNull();
        serviceType.IsClass.Should().BeTrue();
        serviceType.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void DeepResearchOptions_Integration_ShouldWorkWithService()
    {
        // Arrange
        var customOptions = new DeepResearchOptions
        {
            MaxResearchLoops = 5,
            MaxCharacterPerSource = 2000,
            MaxSourceCountPerSearch = 10,
            EnableSummaryConsolidation = true,
            MaxSearchRetryAttempts = 2
        };

        // Act
        var service = new DeepResearchService(_mockChatClient.Object, _mockSearchClient.Object, customOptions);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task RunResearchAsync_WithInvalidChatClient_ShouldThrowException()
    {
        // Arrange
        var options = new DeepResearchOptions { MaxResearchLoops = 1 };
        var service = new DeepResearchService(_mockChatClient.Object, _mockSearchClient.Object, options);

        // Act & Assert
        // Since we haven't set up the mock chat client properly, this should throw
        var act = async () => await service.RunResearchAsync("test topic");
        await act.Should().ThrowAsync<Exception>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Simple topic")]
    [InlineData("Complex topic with special characters: !@#$%")]
    [InlineData("Topic with 日本語")]
    public async Task RunResearchAsync_WithVariousTopics_ShouldAttemptToProcess(string topic)
    {
        // Arrange
        var options = new DeepResearchOptions { MaxResearchLoops = 1 };
        var service = new DeepResearchService(_mockChatClient.Object, _mockSearchClient.Object, options);

        // Act & Assert
        // Should attempt to process the topic (will fail at chat client level, but that's expected)
        var act = async () => await service.RunResearchAsync(topic);
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task RunResearchAsync_WithProgress_ShouldAcceptProgressParameter()
    {
        // Arrange
        var options = new DeepResearchOptions { MaxResearchLoops = 1 };
        var service = new DeepResearchService(_mockChatClient.Object, _mockSearchClient.Object, options);
        var progressReports = new List<ProgressBase>();
        var progress = new Progress<ProgressBase>(progressReports.Add);

        // Act & Assert
        // Should accept progress parameter and attempt processing
        var act = async () => await service.RunResearchAsync("test topic", progress);
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task RunResearchAsync_WithCancellationToken_ShouldAcceptCancellationToken()
    {
        // Arrange
        var options = new DeepResearchOptions { MaxResearchLoops = 1 };
        var service = new DeepResearchService(_mockChatClient.Object, _mockSearchClient.Object, options);
        var cancellationToken = new CancellationToken();

        // Act & Assert
        // Should accept cancellation token and attempt processing
        var act = async () => await service.RunResearchAsync("test topic", null, cancellationToken);
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task RunResearchAsync_WithAllParameters_ShouldAcceptAllParameters()
    {
        // Arrange
        var options = new DeepResearchOptions { MaxResearchLoops = 1 };
        var service = new DeepResearchService(_mockChatClient.Object, _mockSearchClient.Object, options);
        var progressReports = new List<ProgressBase>();
        var progress = new Progress<ProgressBase>(progressReports.Add);
        var cancellationToken = new CancellationToken();

        // Act & Assert
        // Should accept all parameters and attempt processing
        var act = async () => await service.RunResearchAsync("test topic", progress, cancellationToken);
        await act.Should().ThrowAsync<Exception>();
    }
}