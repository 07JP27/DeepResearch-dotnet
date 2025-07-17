using Xunit;
using FluentAssertions;
using DeepResearch.Core;

namespace DeepResearch.Tests;

/// <summary>
/// Unit tests for ResearchState class
/// </summary>
public class ResearchStateTests
{
    [Fact]
    public void Constructor_ShouldInitializeEmptyProperties()
    {
        // Act
        var state = new ResearchState();

        // Assert
        state.ResearchTopic.Should().BeEmpty();
        state.SearchQuery.Should().BeEmpty();
        state.RunningSummary.Should().BeEmpty();
        state.ResearchLoopCount.Should().Be(0);
        state.SourcesGathered.Should().NotBeNull().And.BeEmpty();
        state.WebResearchResults.Should().NotBeNull().And.BeEmpty();
        state.Images.Should().NotBeNull().And.BeEmpty();
        state.KnowledgeGap.Should().BeEmpty();
        state.QueryRationale.Should().BeEmpty();
        state.SummariesGathered.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ResearchTopic_ShouldAllowSettingAndGetting()
    {
        // Arrange
        var state = new ResearchState();
        const string topic = "AI and Machine Learning";

        // Act
        state.ResearchTopic = topic;

        // Assert
        state.ResearchTopic.Should().Be(topic);
    }

    [Fact]
    public void SearchQuery_ShouldAllowSettingAndGetting()
    {
        // Arrange
        var state = new ResearchState();
        const string query = "artificial intelligence machine learning 2024";

        // Act
        state.SearchQuery = query;

        // Assert
        state.SearchQuery.Should().Be(query);
    }

    [Fact]
    public void RunningSummary_ShouldAllowSettingAndGetting()
    {
        // Arrange
        var state = new ResearchState();
        const string summary = "Current findings about AI...";

        // Act
        state.RunningSummary = summary;

        // Assert
        state.RunningSummary.Should().Be(summary);
    }

    [Fact]
    public void ResearchLoopCount_ShouldAllowIncrementing()
    {
        // Arrange
        var state = new ResearchState();

        // Act
        state.ResearchLoopCount++;

        // Assert
        state.ResearchLoopCount.Should().Be(1);
    }

    [Fact]
    public void SourcesGathered_ShouldAllowAddingItems()
    {
        // Arrange
        var state = new ResearchState();
        var source = new SearchResultItem
        {
            Title = "AI Research Paper",
            Url = "https://example.com/paper",
            Content = "Research content..."
        };

        // Act
        state.SourcesGathered.Add(source);

        // Assert
        state.SourcesGathered.Should().HaveCount(1);
        state.SourcesGathered[0].Should().Be(source);
    }

    [Fact]
    public void WebResearchResults_ShouldAllowAddingItems()
    {
        // Arrange
        var state = new ResearchState();
        const string result = "Formatted research result...";

        // Act
        state.WebResearchResults.Add(result);

        // Assert
        state.WebResearchResults.Should().HaveCount(1);
        state.WebResearchResults[0].Should().Be(result);
    }

    [Fact]
    public void Images_ShouldAllowAddingItems()
    {
        // Arrange
        var state = new ResearchState();
        const string imageUrl = "https://example.com/chart.png";

        // Act
        state.Images.Add(imageUrl);

        // Assert
        state.Images.Should().HaveCount(1);
        state.Images[0].Should().Be(imageUrl);
    }

    [Fact]
    public void KnowledgeGap_ShouldAllowSettingAndGetting()
    {
        // Arrange
        var state = new ResearchState();
        const string gap = "Need more information about recent developments...";

        // Act
        state.KnowledgeGap = gap;

        // Assert
        state.KnowledgeGap.Should().Be(gap);
    }

    [Fact]
    public void QueryRationale_ShouldAllowSettingAndGetting()
    {
        // Arrange
        var state = new ResearchState();
        const string rationale = "This query targets recent AI advancements...";

        // Act
        state.QueryRationale = rationale;

        // Assert
        state.QueryRationale.Should().Be(rationale);
    }

    [Fact]
    public void SummariesGathered_ShouldAllowAddingItems()
    {
        // Arrange
        var state = new ResearchState();
        const string summary = "First iteration summary...";

        // Act
        state.SummariesGathered.Add(summary);

        // Assert
        state.SummariesGathered.Should().HaveCount(1);
        state.SummariesGathered[0].Should().Be(summary);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void ResearchLoopCount_ShouldAcceptVariousValues(int count)
    {
        // Arrange
        var state = new ResearchState();

        // Act
        state.ResearchLoopCount = count;

        // Assert
        state.ResearchLoopCount.Should().Be(count);
    }

    [Fact]
    public void AllCollections_ShouldSupportMultipleItems()
    {
        // Arrange
        var state = new ResearchState();
        var sources = new List<SearchResultItem>
        {
            new() { Title = "Source 1", Url = "https://example1.com" },
            new() { Title = "Source 2", Url = "https://example2.com" }
        };
        var webResults = new List<string> { "Result 1", "Result 2" };
        var images = new List<string> { "image1.jpg", "image2.png" };
        var summaries = new List<string> { "Summary 1", "Summary 2" };

        // Act
        state.SourcesGathered.AddRange(sources);
        state.WebResearchResults.AddRange(webResults);
        state.Images.AddRange(images);
        state.SummariesGathered.AddRange(summaries);

        // Assert
        state.SourcesGathered.Should().HaveCount(2);
        state.WebResearchResults.Should().HaveCount(2);
        state.Images.Should().HaveCount(2);
        state.SummariesGathered.Should().HaveCount(2);
    }

    [Fact]
    public void CompleteWorkflow_ShouldUpdateAllProperties()
    {
        // Arrange
        var state = new ResearchState();

        // Act - Simulate a research workflow
        state.ResearchTopic = "Climate Change";
        state.SearchQuery = "climate change impacts 2024";
        state.QueryRationale = "Focus on recent climate data";
        state.ResearchLoopCount = 1;

        state.SourcesGathered.Add(new SearchResultItem
        {
            Title = "Climate Report",
            Url = "https://climate.gov/report"
        });

        state.WebResearchResults.Add("Formatted climate data...");
        state.RunningSummary = "Climate change is accelerating...";
        state.SummariesGathered.Add(state.RunningSummary);
        state.Images.Add("https://climate.gov/chart.png");
        state.KnowledgeGap = "Need regional impact data";

        // Assert
        state.ResearchTopic.Should().Be("Climate Change");
        state.SearchQuery.Should().Be("climate change impacts 2024");
        state.QueryRationale.Should().Be("Focus on recent climate data");
        state.ResearchLoopCount.Should().Be(1);
        state.SourcesGathered.Should().HaveCount(1);
        state.WebResearchResults.Should().HaveCount(1);
        state.RunningSummary.Should().Be("Climate change is accelerating...");
        state.SummariesGathered.Should().HaveCount(1);
        state.Images.Should().HaveCount(1);
        state.KnowledgeGap.Should().Be("Need regional impact data");
    }
}