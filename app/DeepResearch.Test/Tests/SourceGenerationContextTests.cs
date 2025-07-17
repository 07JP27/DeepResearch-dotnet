using Xunit;
using FluentAssertions;
using DeepResearch.Core;
using DeepResearch.Core.JsonSchema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeepResearch.Tests;

/// <summary>
/// Unit tests for SourceGenerationContext
/// </summary>
public class SourceGenerationContextTests
{
    [Fact]
    public void SourceGenerationContext_Default_ShouldNotBeNull()
    {
        // Act & Assert
        SourceGenerationContext.Default.Should().NotBeNull();
    }

    [Fact]
    public void SourceGenerationContext_Default_ShouldBeInstanceOfSourceGenerationContext()
    {
        // Act & Assert
        SourceGenerationContext.Default.Should().BeOfType<SourceGenerationContext>();
    }

    [Fact]
    public void SourceGenerationContext_Default_ShouldInheritFromJsonSerializerContext()
    {
        // Act & Assert
        SourceGenerationContext.Default.Should().BeAssignableTo<JsonSerializerContext>();
    }

    [Fact]
    public void SourceGenerationContext_ShouldHaveGenerateQueryResponseTypeInfo()
    {
        // Act
        var typeInfo = SourceGenerationContext.Default.GenerateQueryResponse;

        // Assert
        typeInfo.Should().NotBeNull();
        typeInfo.Type.Should().Be<GenerateQueryResponse>();
    }

    [Fact]
    public void SourceGenerationContext_ShouldHaveReflectionOnSummaryResponseTypeInfo()
    {
        // Act
        var typeInfo = SourceGenerationContext.Default.ReflectionOnSummaryResponse;

        // Assert
        typeInfo.Should().NotBeNull();
        typeInfo.Type.Should().Be<ReflectionOnSummaryResponse>();
    }

    [Fact]
    public void SourceGenerationContext_GenerateQueryResponseTypeInfo_ShouldHaveCorrectProperties()
    {
        // Act
        var typeInfo = SourceGenerationContext.Default.GenerateQueryResponse;

        // Assert
        typeInfo.Should().NotBeNull();
        typeInfo.Properties.Should().NotBeNull();
        typeInfo.Properties.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void SourceGenerationContext_ReflectionOnSummaryResponseTypeInfo_ShouldHaveCorrectProperties()
    {
        // Act
        var typeInfo = SourceGenerationContext.Default.ReflectionOnSummaryResponse;

        // Assert
        typeInfo.Should().NotBeNull();
        typeInfo.Properties.Should().NotBeNull();
        typeInfo.Properties.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void SourceGenerationContext_ShouldSupportSerializationOfGenerateQueryResponse()
    {
        // Arrange
        var response = new GenerateQueryResponse
        {
            Query = "test query",
            Rationale = "test rationale"
        };

        // Act
        var json = JsonSerializer.Serialize(response, SourceGenerationContext.Default.GenerateQueryResponse);
        var deserializedResponse = JsonSerializer.Deserialize(json, SourceGenerationContext.Default.GenerateQueryResponse);

        // Assert
        json.Should().NotBeNull();
        json.Should().NotBeEmpty();
        deserializedResponse.Should().NotBeNull();
        deserializedResponse.Query.Should().Be("test query");
        deserializedResponse.Rationale.Should().Be("test rationale");
    }

    [Fact]
    public void SourceGenerationContext_ShouldSupportSerializationOfReflectionOnSummaryResponse()
    {
        // Arrange
        var response = new ReflectionOnSummaryResponse
        {
            KnowledgeGap = "test gap",
            FollowUpQuery = "test follow-up query"
        };

        // Act
        var json = JsonSerializer.Serialize(response, SourceGenerationContext.Default.ReflectionOnSummaryResponse);
        var deserializedResponse = JsonSerializer.Deserialize(json, SourceGenerationContext.Default.ReflectionOnSummaryResponse);

        // Assert
        json.Should().NotBeNull();
        json.Should().NotBeEmpty();
        deserializedResponse.Should().NotBeNull();
        deserializedResponse.KnowledgeGap.Should().Be("test gap");
        deserializedResponse.FollowUpQuery.Should().Be("test follow-up query");
    }

    [Fact]
    public void SourceGenerationContext_SerializedJson_ShouldUseCamelCase()
    {
        // Arrange
        var response = new GenerateQueryResponse
        {
            Query = "test query",
            Rationale = "test rationale"
        };

        // Act
        var json = JsonSerializer.Serialize(response, SourceGenerationContext.Default.GenerateQueryResponse);

        // Assert
        json.Should().Contain("\"query\"");
        json.Should().Contain("\"rationale\"");
        json.Should().NotContain("\"Query\"");
        json.Should().NotContain("\"Rationale\"");
    }

    [Fact]
    public void SourceGenerationContext_ShouldBeCaseInsensitiveForDeserialization()
    {
        // Arrange
        var jsonWithPascalCase = """{"Query": "test query", "Rationale": "test rationale"}""";
        var jsonWithCamelCase = """{"query": "test query", "rationale": "test rationale"}""";

        // Act
        var fromPascalCase = JsonSerializer.Deserialize(jsonWithPascalCase, SourceGenerationContext.Default.GenerateQueryResponse);
        var fromCamelCase = JsonSerializer.Deserialize(jsonWithCamelCase, SourceGenerationContext.Default.GenerateQueryResponse);

        // Assert
        fromPascalCase.Should().NotBeNull();
        fromPascalCase.Query.Should().Be("test query");
        fromPascalCase.Rationale.Should().Be("test rationale");

        fromCamelCase.Should().NotBeNull();
        fromCamelCase.Query.Should().Be("test query");
        fromCamelCase.Rationale.Should().Be("test rationale");
    }
}