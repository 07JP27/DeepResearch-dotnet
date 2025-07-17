using Xunit;
using FluentAssertions;
using DeepResearch.Core;

namespace DeepResearch.Tests;

/// <summary>
/// Unit tests for Prompts static class
/// </summary>
public class PromptsTests
{
    [Fact]
    public void QueryWriterInstructions_ShouldNotBeNull()
    {
        // Act & Assert
        Prompts.QueryWriterInstructions.Should().NotBeNull();
    }

    [Fact]
    public void QueryWriterInstructions_ShouldNotBeEmpty()
    {
        // Act & Assert
        Prompts.QueryWriterInstructions.Should().NotBeEmpty();
    }

    [Fact]
    public void QueryWriterInstructions_ShouldContainPlaceholders()
    {
        // Act
        var instructions = Prompts.QueryWriterInstructions;

        // Assert
        instructions.Should().Contain("{0}"); // Date placeholder
        instructions.Should().Contain("{1}"); // Topic placeholder
    }

    [Fact]
    public void QueryWriterInstructions_ShouldContainRequiredSections()
    {
        // Act
        var instructions = Prompts.QueryWriterInstructions;

        // Assert
        instructions.Should().Contain("CONTEXT");
        instructions.Should().Contain("TOPIC");
        instructions.Should().Contain("FORMAT");
        instructions.Should().Contain("EXAMPLE");
    }

    [Fact]
    public void QueryWriterInstructions_ShouldContainJsonFormatRequirements()
    {
        // Act
        var instructions = Prompts.QueryWriterInstructions;

        // Assert
        instructions.Should().Contain("JSON object");
        instructions.Should().Contain("Query");
        instructions.Should().Contain("Rationale");
    }

    [Fact]
    public void SummarizerInstructions_ShouldNotBeNull()
    {
        // Act & Assert
        Prompts.SummarizerInstructions.Should().NotBeNull();
    }

    [Fact]
    public void SummarizerInstructions_ShouldNotBeEmpty()
    {
        // Act & Assert
        Prompts.SummarizerInstructions.Should().NotBeEmpty();
    }

    [Fact]
    public void SummarizerInstructions_ShouldContainRequiredSections()
    {
        // Act
        var instructions = Prompts.SummarizerInstructions;

        // Assert
        instructions.Should().Contain("GOAL");
        instructions.Should().Contain("REQUIREMENTS");
        instructions.Should().Contain("FORMATTING");
        instructions.Should().Contain("Task");
    }

    [Fact]
    public void SummarizerInstructions_ShouldMentionSummaryTypes()
    {
        // Act
        var instructions = Prompts.SummarizerInstructions;

        // Assert
        instructions.Should().Contain("NEW summary");
        instructions.Should().Contain("EXTENDING an existing summary");
    }

    [Fact]
    public void ReflectionInstructions_ShouldNotBeNull()
    {
        // Act & Assert
        Prompts.ReflectionInstructions.Should().NotBeNull();
    }

    [Fact]
    public void ReflectionInstructions_ShouldNotBeEmpty()
    {
        // Act & Assert
        Prompts.ReflectionInstructions.Should().NotBeEmpty();
    }

    [Fact]
    public void ReflectionInstructions_ShouldContainPlaceholder()
    {
        // Act
        var instructions = Prompts.ReflectionInstructions;

        // Assert
        instructions.Should().Contain("{0}"); // Topic placeholder
    }

    [Fact]
    public void ReflectionInstructions_ShouldContainRequiredSections()
    {
        // Act
        var instructions = Prompts.ReflectionInstructions;

        // Assert
        instructions.Should().Contain("GOAL");
        instructions.Should().Contain("REQUIREMENTS");
        instructions.Should().Contain("FORMAT");
        instructions.Should().Contain("Task");
    }

    [Fact]
    public void ReflectionInstructions_ShouldContainJsonFormatRequirements()
    {
        // Act
        var instructions = Prompts.ReflectionInstructions;

        // Assert
        instructions.Should().Contain("JSON object");
        instructions.Should().Contain("KnowledgeGap");
        instructions.Should().Contain("FollowUpQuery");
    }

    [Fact]
    public void FinalizeInstructions_WithEmptyList_ShouldReturnValidInstructions()
    {
        // Arrange
        var emptySummaries = new List<string>();

        // Act
        var instructions = Prompts.FinalizeInstructions(emptySummaries);

        // Assert
        instructions.Should().NotBeNull();
        instructions.Should().NotBeEmpty();
        instructions.Should().Contain("TOPIC");
        instructions.Should().Contain("SUMMARIES");
    }

    [Fact]
    public void FinalizeInstructions_WithSingleSummary_ShouldIncludeSummary()
    {
        // Arrange
        var summaries = new List<string> { "Test summary content" };

        // Act
        var instructions = Prompts.FinalizeInstructions(summaries);

        // Assert
        instructions.Should().Contain("Test summary content");
        instructions.Should().Contain("<SUMMARY>");
        instructions.Should().Contain("</SUMMARY>");
    }

    [Fact]
    public void FinalizeInstructions_WithMultipleSummaries_ShouldIncludeAllSummaries()
    {
        // Arrange
        var summaries = new List<string> 
        { 
            "First summary",
            "Second summary",
            "Third summary"
        };

        // Act
        var instructions = Prompts.FinalizeInstructions(summaries);

        // Assert
        instructions.Should().Contain("First summary");
        instructions.Should().Contain("Second summary");
        instructions.Should().Contain("Third summary");
        
        // Should have three <SUMMARY> tags
        var summaryTagCount = instructions.Split("<SUMMARY>").Length - 1;
        summaryTagCount.Should().Be(3);
    }

    [Fact]
    public void FinalizeInstructions_ShouldContainRequiredGuidelines()
    {
        // Arrange
        var summaries = new List<string> { "Test summary" };

        // Act
        var instructions = Prompts.FinalizeInstructions(summaries);

        // Assert
        instructions.Should().Contain("synthesize");
        instructions.Should().Contain("coherent final report");
        instructions.Should().Contain("Do not use any knowledge other than");
        instructions.Should().Contain("comprehensive final report");
        instructions.Should().Contain("same language");
    }

    [Theory]
    [InlineData("Simple summary")]
    [InlineData("Summary with special characters: !@#$%^&*()")]
    [InlineData("Summary with 日本語 characters")]
    [InlineData("Very long summary that contains multiple sentences and detailed information about various topics that might be included in a research summary")]
    public void FinalizeInstructions_WithVariousSummaryContent_ShouldHandleCorrectly(string summaryContent)
    {
        // Arrange
        var summaries = new List<string> { summaryContent };

        // Act
        var instructions = Prompts.FinalizeInstructions(summaries);

        // Assert
        instructions.Should().Contain(summaryContent);
        instructions.Should().NotBeNull();
        instructions.Should().NotBeEmpty();
    }

    [Fact]
    public void FinalizeInstructions_WithEmptySummaryContent_ShouldHandleCorrectly()
    {
        // Arrange
        var summaries = new List<string> { "" };

        // Act
        var instructions = Prompts.FinalizeInstructions(summaries);

        // Assert
        instructions.Should().NotBeNull();
        instructions.Should().NotBeEmpty();
        instructions.Should().Contain("<SUMMARY></SUMMARY>");
    }

    [Fact]
    public void FinalizeInstructions_WithNullSummary_ShouldHandleGracefully()
    {
        // Arrange
        var summaries = new List<string> { null! };

        // Act
        var act = () => Prompts.FinalizeInstructions(summaries);

        // Assert
        // Should not throw exception, but behavior depends on implementation
        // In this case, it might include null in the template
        act.Should().NotThrow();
    }
}