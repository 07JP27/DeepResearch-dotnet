using FluentAssertions;
using DeepResearch.Core.Models;

namespace DeepResearch.Tests;

/// <summary>
/// Unit tests for Progress model classes
/// </summary>
public class ProgressModelsTests
{
    [Fact]
    public void ProgressBase_ShouldBeAbstractBase()
    {
        // This test verifies the inheritance structure
        var progressType = typeof(ProgressBase);
        progressType.IsAbstract.Should().BeTrue(); // It is abstract in the implementation
        progressType.IsClass.Should().BeTrue();
    }

    [Fact]
    public void ProgressBase_Timestamp_ShouldBeSettable()
    {
        // Arrange
        var progress = new ErrorProgress(); // Using concrete implementation
        var testTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        progress.Timestamp = testTime;

        // Assert
        progress.Timestamp.Should().Be(testTime);
    }

    [Fact]
    public void ProgressBase_Timestamp_ShouldDefaultToMinValue()
    {
        // Arrange & Act
        var progress = new ErrorProgress(); // Using concrete implementation

        // Assert
        // Since we removed DateTime.UtcNow default initialization, it should be DateTime.MinValue
        progress.Timestamp.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void QueryGenerationProgress_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var progress = new QueryGenerationProgress();

        // Assert
        progress.Should().BeAssignableTo<ProgressBase>();
        progress.Query.Should().Be("");
        progress.Rationale.Should().Be("");
    }

    [Fact]
    public void QueryGenerationProgress_ShouldAllowSettingProperties()
    {
        // Arrange
        var progress = new QueryGenerationProgress();
        const string query = "climate change impacts 2024";
        const string rationale = "Focus on recent climate data for comprehensive analysis";

        // Act
        progress.Query = query;
        progress.Rationale = rationale;

        // Assert
        progress.Query.Should().Be(query);
        progress.Rationale.Should().Be(rationale);
    }

    [Fact]
    public void WebResearchProgress_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var progress = new WebResearchProgress();

        // Assert
        progress.Should().BeAssignableTo<ProgressBase>();
        progress.Sources.Should().NotBeNull().And.BeEmpty();
        progress.Images.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void SummarizeProgress_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var progress = new SummarizeProgress();

        // Assert
        progress.Should().BeAssignableTo<ProgressBase>();
        progress.Summary.Should().Be("");
    }

    [Fact]
    public void ReflectionProgress_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var progress = new ReflectionProgress();

        // Assert
        progress.Should().BeAssignableTo<ProgressBase>();
        progress.Query.Should().Be("");
        progress.KnowledgeGap.Should().Be("");
    }

    [Fact]
    public void RoutingProgress_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var progress = new RoutingProgress();

        // Assert
        progress.Should().BeAssignableTo<ProgressBase>();
        progress.Decision.Should().Be(default(RoutingDecision));
        progress.LoopCount.Should().Be(0);
    }

    [Fact]
    public void FinalizeProgress_ShouldInheritFromProgressBase()
    {
        // Arrange & Act
        var progress = new FinalizeProgress();

        // Assert
        progress.Should().BeAssignableTo<ProgressBase>();
    }

    [Fact]
    public void ResearchCompleteProgress_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var progress = new ResearchCompleteProgress();

        // Assert
        progress.Should().BeAssignableTo<ProgressBase>();
        progress.FinalSummary.Should().BeNull();
        progress.Sources.Should().NotBeNull().And.BeEmpty();
        progress.Images.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ErrorProgress_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var progress = new ErrorProgress();

        // Assert
        progress.Should().BeAssignableTo<ProgressBase>();
        progress.Message.Should().Be("");
    }

    [Fact]
    public void ThinkingProgress_ShouldInheritFromProgressBase()
    {
        // Arrange & Act
        var progress = new ThinkingProgress();

        // Assert
        progress.Should().BeAssignableTo<ProgressBase>();
    }

    [Theory]
    [InlineData(RoutingDecision.Continue)]
    [InlineData(RoutingDecision.Finalize)]
    public void RoutingProgress_ShouldAcceptValidDecisions(RoutingDecision decision)
    {
        // Arrange
        var progress = new RoutingProgress();

        // Act
        progress.Decision = decision;

        // Assert
        progress.Decision.Should().Be(decision);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void RoutingProgress_ShouldAcceptValidLoopCounts(int loopCount)
    {
        // Arrange
        var progress = new RoutingProgress();

        // Act
        progress.LoopCount = loopCount;

        // Assert
        progress.LoopCount.Should().Be(loopCount);
    }
}