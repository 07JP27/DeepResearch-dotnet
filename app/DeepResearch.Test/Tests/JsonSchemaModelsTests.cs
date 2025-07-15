using Xunit;
using FluentAssertions;
using DeepResearch.Core;
using DeepResearch.Core.JsonSchema;

namespace DeepResearch.Core.Tests;

/// <summary>
/// Unit tests for JsonSchema model classes
/// </summary>
public class JsonSchemaModelsTests
{
    [Fact]
    public void GenerateQueryResponse_ShouldHaveDefaultValues()
    {
        // Act
        var response = new GenerateQueryResponse();

        // Assert
        response.Query.Should().Be(string.Empty);
        response.Rationale.Should().Be(string.Empty);
    }

    [Fact]
    public void GenerateQueryResponse_ShouldAllowSettingQuery()
    {
        // Arrange
        var response = new GenerateQueryResponse();
        const string testQuery = "climate change impacts 2024";

        // Act
        response.Query = testQuery;

        // Assert
        response.Query.Should().Be(testQuery);
    }

    [Fact]
    public void GenerateQueryResponse_ShouldAllowSettingRationale()
    {
        // Arrange
        var response = new GenerateQueryResponse();
        const string testRationale = "This query focuses on recent climate data for comprehensive analysis";

        // Act
        response.Rationale = testRationale;

        // Assert
        response.Rationale.Should().Be(testRationale);
    }

    [Fact]
    public void GenerateQueryResponse_ShouldAllowSettingBothProperties()
    {
        // Arrange
        var response = new GenerateQueryResponse();
        const string testQuery = "AI research trends 2024";
        const string testRationale = "Need to understand current AI developments";

        // Act
        response.Query = testQuery;
        response.Rationale = testRationale;

        // Assert
        response.Query.Should().Be(testQuery);
        response.Rationale.Should().Be(testRationale);
    }

    [Fact]
    public void ReflectionOnSummaryResponse_ShouldHaveDefaultValues()
    {
        // Act
        var response = new ReflectionOnSummaryResponse();

        // Assert
        response.FollowUpQuery.Should().Be(string.Empty);
        response.KnowledgeGap.Should().Be(string.Empty);
    }

    [Fact]
    public void ReflectionOnSummaryResponse_ShouldAllowSettingFollowUpQuery()
    {
        // Arrange
        var response = new ReflectionOnSummaryResponse();
        const string testQuery = "renewable energy adoption rates";

        // Act
        response.FollowUpQuery = testQuery;

        // Assert
        response.FollowUpQuery.Should().Be(testQuery);
    }

    [Fact]
    public void ReflectionOnSummaryResponse_ShouldAllowSettingKnowledgeGap()
    {
        // Arrange
        var response = new ReflectionOnSummaryResponse();
        const string testGap = "Missing information about regional variations";

        // Act
        response.KnowledgeGap = testGap;

        // Assert
        response.KnowledgeGap.Should().Be(testGap);
    }

    [Fact]
    public void ReflectionOnSummaryResponse_ShouldAllowSettingBothProperties()
    {
        // Arrange
        var response = new ReflectionOnSummaryResponse();
        const string testQuery = "solar panel efficiency improvements";
        const string testGap = "Lack of data on cost-effectiveness comparisons";

        // Act
        response.FollowUpQuery = testQuery;
        response.KnowledgeGap = testGap;

        // Assert
        response.FollowUpQuery.Should().Be(testQuery);
        response.KnowledgeGap.Should().Be(testGap);
    }

    [Theory]
    [InlineData("")]
    [InlineData("simple query")]
    [InlineData("複雑な日本語クエリ")]
    [InlineData("very long query with multiple words and complex technical terminology")]
    public void GenerateQueryResponse_ShouldAcceptVariousQueryValues(string queryValue)
    {
        // Arrange
        var response = new GenerateQueryResponse();

        // Act
        response.Query = queryValue;

        // Assert
        response.Query.Should().Be(queryValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short gap")]
    [InlineData("情報不足について")]
    [InlineData("detailed knowledge gap explaining what information is missing and why it's important")]
    public void ReflectionOnSummaryResponse_ShouldAcceptVariousKnowledgeGapValues(string gapValue)
    {
        // Arrange
        var response = new ReflectionOnSummaryResponse();

        // Act
        response.KnowledgeGap = gapValue;

        // Assert
        response.KnowledgeGap.Should().Be(gapValue);
    }
}