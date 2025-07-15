using Xunit;
using FluentAssertions;
using DeepResearch.Core;

namespace DeepResearch.Tests;

/// <summary>
/// Unit tests for DeepResearchOptions class
/// </summary>
public class DeepResearchOptionsTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var options = new DeepResearchOptions();

        // Assert
        options.MaxResearchLoops.Should().Be(3);
        options.MaxCharacterPerSource.Should().Be(4000);
        options.MaxSourceCountPerSearch.Should().Be(5);
        options.EnableSummaryConsolidation.Should().BeFalse();
    }

    [Fact]
    public void MaxResearchLoops_ShouldAllowCustomValue()
    {
        // Arrange
        var options = new DeepResearchOptions();
        const int customValue = 10;

        // Act
        options.MaxResearchLoops = customValue;

        // Assert
        options.MaxResearchLoops.Should().Be(customValue);
    }

    [Fact]
    public void MaxCharacterPerSource_ShouldAllowCustomValue()
    {
        // Arrange
        var options = new DeepResearchOptions();
        const int customValue = 2000;

        // Act
        options.MaxCharacterPerSource = customValue;

        // Assert
        options.MaxCharacterPerSource.Should().Be(customValue);
    }

    [Fact]
    public void MaxSourceCountPerSearch_ShouldAllowCustomValue()
    {
        // Arrange
        var options = new DeepResearchOptions();
        const int customValue = 10;

        // Act
        options.MaxSourceCountPerSearch = customValue;

        // Assert
        options.MaxSourceCountPerSearch.Should().Be(customValue);
    }

    [Fact]
    public void EnableSummaryConsolidation_ShouldAllowCustomValue()
    {
        // Arrange
        var options = new DeepResearchOptions();

        // Act
        options.EnableSummaryConsolidation = true;

        // Assert
        options.EnableSummaryConsolidation.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void MaxResearchLoops_ShouldAcceptVariousValues(int value)
    {
        // Arrange
        var options = new DeepResearchOptions();

        // Act
        options.MaxResearchLoops = value;

        // Assert
        options.MaxResearchLoops.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1000)]
    [InlineData(10000)]
    public void MaxCharacterPerSource_ShouldAcceptVariousValues(int value)
    {
        // Arrange
        var options = new DeepResearchOptions();

        // Act
        options.MaxCharacterPerSource = value;

        // Assert
        options.MaxCharacterPerSource.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void MaxSourceCountPerSearch_ShouldAcceptVariousValues(int value)
    {
        // Arrange
        var options = new DeepResearchOptions();

        // Act
        options.MaxSourceCountPerSearch = value;

        // Assert
        options.MaxSourceCountPerSearch.Should().Be(value);
    }
}