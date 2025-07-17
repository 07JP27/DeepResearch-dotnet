using Xunit;
using FluentAssertions;
using DeepResearch.Core;
using DeepResearch.Core.SearchClient;

namespace DeepResearch.Tests;

/// <summary>
/// Unit tests for Formatting static utility class
/// </summary>
public class FormattingTests
{
    #region DeduplicateAndFormatSources Tests

    [Fact]
    public void DeduplicateAndFormatSources_WithNullSearchResult_ShouldReturnEmpty()
    {
        // Act
        var result = Formatting.DeduplicateAndFormatSources(null, 1000);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void DeduplicateAndFormatSources_WithNullResults_ShouldReturnEmpty()
    {
        // Arrange
        var searchResult = new SearchResult { Results = null };

        // Act
        var result = Formatting.DeduplicateAndFormatSources(searchResult, 1000);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void DeduplicateAndFormatSources_WithEmptyResults_ShouldReturnEmpty()
    {
        // Arrange
        var searchResult = new SearchResult { Results = new List<SearchResultItem>() };

        // Act
        var result = Formatting.DeduplicateAndFormatSources(searchResult, 1000);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void DeduplicateAndFormatSources_WithSingleSource_ShouldFormatCorrectly()
    {
        // Arrange
        var searchResult = new SearchResult
        {
            Results = new List<SearchResultItem>
            {
                new()
                {
                    Title = "Test Title",
                    Url = "https://example.com",
                    Content = "Test content",
                    RawContent = "Full test content"
                }
            }
        };

        // Act
        var result = Formatting.DeduplicateAndFormatSources(searchResult, 1000);

        // Assert
        result.Should().Contain("Sources:");
        result.Should().Contain("Source: Test Title");
        result.Should().Contain("URL: https://example.com");
        result.Should().Contain("Most relevant content from source: Test content");
        result.Should().Contain("Full source content limited to 1000 tokens: Full test content");
    }

    [Fact]
    public void DeduplicateAndFormatSources_WithDuplicateUrls_ShouldDeduplicateByUrl()
    {
        // Arrange
        var searchResult = new SearchResult
        {
            Results = new List<SearchResultItem>
            {
                new()
                {
                    Title = "First Title",
                    Url = "https://example.com",
                    Content = "First content"
                },
                new()
                {
                    Title = "Second Title",
                    Url = "https://example.com", // Same URL
                    Content = "Second content"
                }
            }
        };

        // Act
        var result = Formatting.DeduplicateAndFormatSources(searchResult, 1000);

        // Assert
        result.Should().Contain("First Title");
        result.Should().NotContain("Second Title");
        var sourceCount = result.Split("Source:", StringSplitOptions.RemoveEmptyEntries).Length - 1;
        sourceCount.Should().Be(1);
    }

    [Fact]
    public void DeduplicateAndFormatSources_WithLongContent_ShouldTruncate()
    {
        // Arrange
        var longContent = new string('A', 1500);
        var searchResult = new SearchResult
        {
            Results = new List<SearchResultItem>
            {
                new()
                {
                    Title = "Test Title",
                    Url = "https://example.com",
                    Content = "Test content",
                    RawContent = longContent
                }
            }
        };

        // Act
        var result = Formatting.DeduplicateAndFormatSources(searchResult, 1000);

        // Assert
        result.Should().Contain("... [truncated]");
        var truncatedContent = result.Substring(result.IndexOf("Full source content"));
        truncatedContent.Should().NotContain(new string('A', 1500));
    }

    [Fact]
    public void DeduplicateAndFormatSources_WithFetchFullPageFalse_ShouldNotIncludeRawContent()
    {
        // Arrange
        var searchResult = new SearchResult
        {
            Results = new List<SearchResultItem>
            {
                new()
                {
                    Title = "Test Title",
                    Url = "https://example.com",
                    Content = "Test content",
                    RawContent = "Full test content"
                }
            }
        };

        // Act
        var result = Formatting.DeduplicateAndFormatSources(searchResult, 1000, false);

        // Assert
        result.Should().Contain("Source: Test Title");
        result.Should().NotContain("Full source content limited to");
        result.Should().NotContain("Full test content");
    }

    [Fact]
    public void DeduplicateAndFormatSources_WithMissingProperties_ShouldUseDefaults()
    {
        // Arrange
        var searchResult = new SearchResult
        {
            Results = new List<SearchResultItem>
            {
                new()
                {
                    Title = null,
                    Url = "https://example.com", // Must have valid URL to not be filtered out
                    Content = null,
                    RawContent = null
                }
            }
        };

        // Act
        var result = Formatting.DeduplicateAndFormatSources(searchResult, 1000);

        // Assert
        result.Should().Contain("(no title)");
        result.Should().Contain("https://example.com");
    }

    #endregion

    #region FormatSources Tests

    [Fact]
    public void FormatSources_WithNullSearchResult_ShouldReturnEmpty()
    {
        // Act
        var result = Formatting.FormatSources(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FormatSources_WithNullResults_ShouldReturnEmpty()
    {
        // Arrange
        var searchResult = new SearchResult { Results = null };

        // Act
        var result = Formatting.FormatSources(searchResult);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FormatSources_WithEmptyResults_ShouldReturnEmpty()
    {
        // Arrange
        var searchResult = new SearchResult { Results = new List<SearchResultItem>() };

        // Act
        var result = Formatting.FormatSources(searchResult);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FormatSources_WithSingleSource_ShouldFormatAsBulletPoint()
    {
        // Arrange
        var searchResult = new SearchResult
        {
            Results = new List<SearchResultItem>
            {
                new()
                {
                    Title = "Test Title",
                    Url = "https://example.com"
                }
            }
        };

        // Act
        var result = Formatting.FormatSources(searchResult);

        // Assert
        result.Should().Be("* Test Title : https://example.com");
    }

    [Fact]
    public void FormatSources_WithMultipleSources_ShouldFormatAsMultipleBulletPoints()
    {
        // Arrange
        var searchResult = new SearchResult
        {
            Results = new List<SearchResultItem>
            {
                new() { Title = "Title 1", Url = "https://example1.com" },
                new() { Title = "Title 2", Url = "https://example2.com" },
                new() { Title = "Title 3", Url = "https://example3.com" }
            }
        };

        // Act
        var result = Formatting.FormatSources(searchResult);

        // Assert
        var lines = result.Split('\n');
        lines.Should().HaveCount(3);
        lines[0].Should().Be("* Title 1 : https://example1.com");
        lines[1].Should().Be("* Title 2 : https://example2.com");
        lines[2].Should().Be("* Title 3 : https://example3.com");
    }

    [Fact]
    public void FormatSources_WithMissingProperties_ShouldUseDefaults()
    {
        // Arrange
        var searchResult = new SearchResult
        {
            Results = new List<SearchResultItem>
            {
                new() { Title = null, Url = null },
                new() { Title = "", Url = "" }
            }
        };

        // Act
        var result = Formatting.FormatSources(searchResult);

        // Assert
        result.Should().Contain("* (no title) : (no url)");
        var lines = result.Split('\n');
        lines.Should().HaveCount(2);
        lines.Should().AllSatisfy(line => line.Should().Contain("(no title)").And.Contain("(no url)"));
    }

    #endregion

    #region DeduplicateAndCleanSources Tests

    [Fact]
    public void DeduplicateAndCleanSources_WithEmptyNewSources_ShouldReturnEmpty()
    {
        // Arrange
        var newSources = new List<SearchResultItem>();
        var existingSources = new List<SearchResultItem>();

        // Act
        var result = Formatting.DeduplicateAndCleanSources(newSources, existingSources);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void DeduplicateAndCleanSources_WithNoDuplicates_ShouldReturnAllNewSources()
    {
        // Arrange
        var newSources = new List<SearchResultItem>
        {
            new() { Title = "New Title 1", Url = "https://new1.com", Content = "New content 1" },
            new() { Title = "New Title 2", Url = "https://new2.com", Content = "New content 2" }
        };
        var existingSources = new List<SearchResultItem>
        {
            new() { Title = "Existing Title", Url = "https://existing.com", Content = "Existing content" }
        };

        // Act
        var result = Formatting.DeduplicateAndCleanSources(newSources, existingSources);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Url == "https://new1.com");
        result.Should().Contain(s => s.Url == "https://new2.com");
    }

    [Fact]
    public void DeduplicateAndCleanSources_WithDuplicates_ShouldFilterOutDuplicates()
    {
        // Arrange
        var newSources = new List<SearchResultItem>
        {
            new() { Title = "New Title", Url = "https://new.com", Content = "New content" },
            new() { Title = "Duplicate Title", Url = "https://existing.com", Content = "Duplicate content" }
        };
        var existingSources = new List<SearchResultItem>
        {
            new() { Title = "Existing Title", Url = "https://existing.com", Content = "Existing content" }
        };

        // Act
        var result = Formatting.DeduplicateAndCleanSources(newSources, existingSources);

        // Assert
        result.Should().HaveCount(1);
        result[0].Url.Should().Be("https://new.com");
    }

    [Fact]
    public void DeduplicateAndCleanSources_WithEmptyOrNullUrls_ShouldFilterOut()
    {
        // Arrange
        var newSources = new List<SearchResultItem>
        {
            new() { Title = "Valid Title", Url = "https://valid.com", Content = "Valid content" },
            new() { Title = "Empty URL", Url = "", Content = "Empty URL content" },
            new() { Title = "Null URL", Url = null, Content = "Null URL content" }
        };
        var existingSources = new List<SearchResultItem>();

        // Act
        var result = Formatting.DeduplicateAndCleanSources(newSources, existingSources);

        // Assert
        result.Should().HaveCount(1);
        result[0].Url.Should().Be("https://valid.com");
    }

    [Fact]
    public void DeduplicateAndCleanSources_WithGarbledText_ShouldCleanText()
    {
        // Arrange - Using text with actual Japanese characters to trigger cleaning
        var newSources = new List<SearchResultItem>
        {
            new()
            {
                Title = "日本語のTitleã with ã¯ garbled text",
                Url = "https://example.com",
                Content = "日本語のContentã with ã§ problems",
                RawContent = "日本語のRaw contentã needs cleaning"
            }
        };
        var existingSources = new List<SearchResultItem>();

        // Act
        var result = Formatting.DeduplicateAndCleanSources(newSources, existingSources);

        // Assert
        result.Should().HaveCount(1);
        result[0].Title.Should().NotContain("ã");
        result[0].Content.Should().NotContain("ã");
        result[0].RawContent.Should().NotContain("ã");
    }

    [Fact]
    public void DeduplicateAndCleanSources_WithDuplicatesInNewSources_ShouldPreventDuplicatesWithinBatch()
    {
        // Arrange
        var newSources = new List<SearchResultItem>
        {
            new() { Title = "First", Url = "https://same.com", Content = "First content" },
            new() { Title = "Second", Url = "https://same.com", Content = "Second content" }
        };
        var existingSources = new List<SearchResultItem>();

        // Act
        var result = Formatting.DeduplicateAndCleanSources(newSources, existingSources);

        // Assert
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("First");
    }

    #endregion

    #region Japanese Text Cleaning Tests

    [Theory]
    [InlineData("Normal text", "Normal text")]
    [InlineData("日本語でã¯ Japanese text", "日本語では Japanese text")]
    [InlineData("日本語でã§ Japanese text", "日本語でで Japanese text")]
    [InlineData("日本語のContent with ã» separator", "日本語のContent with ・ separator")]
    [InlineData("日本語のText with\0null chars", "日本語のText withnull chars")]
    [InlineData("日本語の   Text   with   spaces", "日本語の Text with spaces")]
    public void DeduplicateAndCleanSources_ShouldCleanSpecificPatterns(string input, string expected)
    {
        // Arrange
        var newSources = new List<SearchResultItem>
        {
            new() { Title = input, Url = "https://example.com", Content = input }
        };
        var existingSources = new List<SearchResultItem>();

        // Act
        var result = Formatting.DeduplicateAndCleanSources(newSources, existingSources);

        // Assert
        result.Should().HaveCount(1);
        result[0].Title.Should().Be(expected);
        result[0].Content.Should().Be(expected);
    }

    [Fact]
    public void DeduplicateAndCleanSources_WithJapaneseText_ShouldPreserveJapanese()
    {
        // Arrange
        var newSources = new List<SearchResultItem>
        {
            new()
            {
                Title = "これは日本語のテストです",
                Url = "https://example.com",
                Content = "日本語のコンテンツがあります。"
            }
        };
        var existingSources = new List<SearchResultItem>();

        // Act
        var result = Formatting.DeduplicateAndCleanSources(newSources, existingSources);

        // Assert
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("これは日本語のテストです");
        result[0].Content.Should().Be("日本語のコンテンツがあります。");
    }

    [Fact]
    public void DeduplicateAndCleanSources_WithNonJapaneseGarbledText_ShouldNotClean()
    {
        // Arrange - Text without Japanese characters should not be cleaned
        var newSources = new List<SearchResultItem>
        {
            new()
            {
                Title = "Titleã with ã¯ garbled text",
                Url = "https://example.com",
                Content = "Contentã with ã§ problems"
            }
        };
        var existingSources = new List<SearchResultItem>();

        // Act
        var result = Formatting.DeduplicateAndCleanSources(newSources, existingSources);

        // Assert
        result.Should().HaveCount(1);
        // Since there's no Japanese text, cleaning should not occur
        result[0].Title.Should().Be("Titleã with ã¯ garbled text");
        result[0].Content.Should().Be("Contentã with ã§ problems");
    }

    #endregion
}