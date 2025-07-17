using Xunit;
using FluentAssertions;
using DeepResearch.Core;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;

namespace DeepResearch.Tests;

/// <summary>
/// Unit tests for JsonSchemaGenerator class
/// </summary>
public class JsonSchemaGeneratorTests
{
    [Fact]
    public void GenerateSchema_WithGenerateQueryResponse_ShouldReturnValidSchema()
    {
        // Arrange
        var typeInfo = SourceGenerationContext.Default.GenerateQueryResponse;

        // Act
        var schema = JsonSchemaGenerator.GenerateSchema(typeInfo);

        // Assert
        schema.Should().NotBeNull();
        schema.Should().NotBeEmpty();
        schema.Should().Contain("type");
    }

    [Fact]
    public void GenerateSchemaAsBinaryData_WithGenerateQueryResponse_ShouldReturnBinaryData()
    {
        // Arrange
        var typeInfo = SourceGenerationContext.Default.GenerateQueryResponse;

        // Act
        var binaryData = JsonSchemaGenerator.GenerateSchemaAsBinaryData(typeInfo);

        // Assert
        binaryData.Should().NotBeNull();
        binaryData.ToArray().Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateSchemaAsBinaryData_ShouldReturnSameContentAsGenerateSchema()
    {
        // Arrange
        var typeInfo = SourceGenerationContext.Default.GenerateQueryResponse;

        // Act
        var schema = JsonSchemaGenerator.GenerateSchema(typeInfo);
        var binaryData = JsonSchemaGenerator.GenerateSchemaAsBinaryData(typeInfo);
        var binaryDataAsString = binaryData.ToString();

        // Assert
        binaryDataAsString.Should().Be(schema);
    }

    [Fact]
    public void GenerateSchema_ShouldReturnValidJsonString()
    {
        // Arrange
        var typeInfo = SourceGenerationContext.Default.GenerateQueryResponse;

        // Act
        var schema = JsonSchemaGenerator.GenerateSchema(typeInfo);

        // Assert
        // Should be able to parse as JSON without throwing
        var act = () => JsonDocument.Parse(schema);
        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateSchema_WithReflectionOnSummaryResponse_ShouldReturnValidSchema()
    {
        // Arrange
        var typeInfo = SourceGenerationContext.Default.ReflectionOnSummaryResponse;

        // Act
        var schema = JsonSchemaGenerator.GenerateSchema(typeInfo);

        // Assert
        schema.Should().NotBeNull();
        schema.Should().NotBeEmpty();
        schema.Should().Contain("type");

        // Should be valid JSON
        var act = () => JsonDocument.Parse(schema);
        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateSchema_ShouldReturnSchemaWithObjectType()
    {
        // Arrange
        var typeInfo = SourceGenerationContext.Default.GenerateQueryResponse;

        // Act
        var schema = JsonSchemaGenerator.GenerateSchema(typeInfo);
        using var document = JsonDocument.Parse(schema);
        var root = document.RootElement;

        // Assert
        root.GetProperty("type").GetString().Should().Be("object");
    }
}