using Xunit;
using FluentAssertions;
using DeepResearch.SearchClient;

namespace DeepResearch.Core.Tests;

/// <summary>
/// Unit tests for ResearchResult class
/// </summary>
public class ResearchResultTests
{
    [Fact]
    public void Constructor_ShouldInitializeEmptyProperties()
    {
        // Act
        var result = new ResearchResult();

        // Assert
        result.ResearchTopic.Should().BeEmpty();
        result.Summary.Should().BeEmpty();
        result.Sources.Should().NotBeNull().And.BeEmpty();
        result.Images.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ResearchTopic_ShouldAllowSettingAndGetting()
    {
        // Arrange
        var result = new ResearchResult();
        const string topic = "Test Research Topic";

        // Act
        result.ResearchTopic = topic;

        // Assert
        result.ResearchTopic.Should().Be(topic);
    }

    [Fact]
    public void Summary_ShouldAllowSettingAndGetting()
    {
        // Arrange
        var result = new ResearchResult();
        const string summary = "This is a test summary of research findings.";

        // Act
        result.Summary = summary;

        // Assert
        result.Summary.Should().Be(summary);
    }

    [Fact]
    public void Sources_ShouldAllowAddingItems()
    {
        // Arrange
        var result = new ResearchResult();
        var source = new SearchResultItem
        {
            Title = "Test Title",
            Url = "https://example.com",
            Content = "Test content"
        };

        // Act
        result.Sources.Add(source);

        // Assert
        result.Sources.Should().HaveCount(1);
        result.Sources[0].Should().Be(source);
    }

    [Fact]
    public void Images_ShouldAllowAddingItems()
    {
        // Arrange
        var result = new ResearchResult();
        const string imageUrl = "https://example.com/image.jpg";

        // Act
        result.Images.Add(imageUrl);

        // Assert
        result.Images.Should().HaveCount(1);
        result.Images[0].Should().Be(imageUrl);
    }

    [Fact]
    public void Sources_ShouldAllowMultipleItems()
    {
        // Arrange
        var result = new ResearchResult();
        var sources = new List<SearchResultItem>
        {
            new() { Title = "Title 1", Url = "https://example1.com" },
            new() { Title = "Title 2", Url = "https://example2.com" },
            new() { Title = "Title 3", Url = "https://example3.com" }
        };

        // Act
        result.Sources.AddRange(sources);

        // Assert
        result.Sources.Should().HaveCount(3);
        result.Sources.Should().BeEquivalentTo(sources);
    }

    [Fact]
    public void Images_ShouldAllowMultipleItems()
    {
        // Arrange
        var result = new ResearchResult();
        var images = new List<string>
        {
            "https://example.com/image1.jpg",
            "https://example.com/image2.png",
            "https://example.com/image3.gif"
        };

        // Act
        result.Images.AddRange(images);

        // Assert
        result.Images.Should().HaveCount(3);
        result.Images.Should().BeEquivalentTo(images);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Simple topic")]
    [InlineData("Complex topic with special characters: !@#$%^&*()")]
    [InlineData("非常に長いトピック名のテストケースです。これは日本語のテキストを含んでいます。")]
    public void ResearchTopic_ShouldHandleVariousInputs(string topic)
    {
        // Arrange
        var result = new ResearchResult();

        // Act
        result.ResearchTopic = topic;

        // Assert
        result.ResearchTopic.Should().Be(topic);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Short summary.")]
    [InlineData("Very long summary with multiple sentences. This contains detailed information about the research findings. It includes various aspects of the topic and provides comprehensive insights.")]
    public void Summary_ShouldHandleVariousInputs(string summary)
    {
        // Arrange
        var result = new ResearchResult();

        // Act
        result.Summary = summary;

        // Assert
        result.Summary.Should().Be(summary);
    }
}