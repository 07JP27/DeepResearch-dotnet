using Xunit;
using FluentAssertions;
using DeepResearch.Core;

namespace DeepResearch.Tests;

/// <summary>
/// Unit tests for SearchResult and SearchResultItem classes
/// </summary>
public class SearchResultTests
{
    [Fact]
    public void SearchResult_Constructor_ShouldInitializeEmptyLists()
    {
        // Act
        var searchResult = new SearchResult();

        // Assert
        searchResult.Results.Should().NotBeNull();
        searchResult.Results.Should().BeEmpty();
        searchResult.Images.Should().NotBeNull();
        searchResult.Images.Should().BeEmpty();
    }

    [Fact]
    public void SearchResult_Results_ShouldAllowAddingItems()
    {
        // Arrange
        var searchResult = new SearchResult();
        var item = new SearchResultItem { Title = "Test Title", Url = "https://example.com" };

        // Act
        searchResult.Results.Add(item);

        // Assert
        searchResult.Results.Should().HaveCount(1);
        searchResult.Results[0].Should().Be(item);
    }

    [Fact]
    public void SearchResult_Images_ShouldAllowAddingItems()
    {
        // Arrange
        var searchResult = new SearchResult();
        const string imageUrl = "https://example.com/image.jpg";

        // Act
        searchResult.Images.Add(imageUrl);

        // Assert
        searchResult.Images.Should().HaveCount(1);
        searchResult.Images[0].Should().Be(imageUrl);
    }

    [Fact]
    public void SearchResult_Results_ShouldAllowSettingNewList()
    {
        // Arrange
        var searchResult = new SearchResult();
        var newResults = new List<SearchResultItem>
        {
            new() { Title = "Title1", Url = "https://example1.com" },
            new() { Title = "Title2", Url = "https://example2.com" }
        };

        // Act
        searchResult.Results = newResults;

        // Assert
        searchResult.Results.Should().BeEquivalentTo(newResults);
        searchResult.Results.Should().HaveCount(2);
    }

    [Fact]
    public void SearchResult_Images_ShouldAllowSettingNewList()
    {
        // Arrange
        var searchResult = new SearchResult();
        var newImages = new List<string> { "image1.jpg", "image2.png" };

        // Act
        searchResult.Images = newImages;

        // Assert
        searchResult.Images.Should().BeEquivalentTo(newImages);
        searchResult.Images.Should().HaveCount(2);
    }
}

/// <summary>
/// Unit tests for SearchResultItem class
/// </summary>
public class SearchResultItemTests
{
    [Fact]
    public void SearchResultItem_Constructor_ShouldInitializeEmptyStrings()
    {
        // Act
        var item = new SearchResultItem();

        // Assert
        item.Title.Should().Be(string.Empty);
        item.Url.Should().Be(string.Empty);
        item.Content.Should().Be(string.Empty);
        item.RawContent.Should().Be(string.Empty);
    }

    [Fact]
    public void SearchResultItem_Title_ShouldAllowSettingAndGetting()
    {
        // Arrange
        var item = new SearchResultItem();
        const string title = "Test Title";

        // Act
        item.Title = title;

        // Assert
        item.Title.Should().Be(title);
    }

    [Fact]
    public void SearchResultItem_Url_ShouldAllowSettingAndGetting()
    {
        // Arrange
        var item = new SearchResultItem();
        const string url = "https://example.com";

        // Act
        item.Url = url;

        // Assert
        item.Url.Should().Be(url);
    }

    [Fact]
    public void SearchResultItem_Content_ShouldAllowSettingAndGetting()
    {
        // Arrange
        var item = new SearchResultItem();
        const string content = "Test content";

        // Act
        item.Content = content;

        // Assert
        item.Content.Should().Be(content);
    }

    [Fact]
    public void SearchResultItem_RawContent_ShouldAllowSettingAndGetting()
    {
        // Arrange
        var item = new SearchResultItem();
        const string rawContent = "Raw test content";

        // Act
        item.RawContent = rawContent;

        // Assert
        item.RawContent.Should().Be(rawContent);
    }

    [Fact]
    public void SearchResultItem_AllProperties_ShouldBeIndependent()
    {
        // Arrange
        var item = new SearchResultItem();

        // Act
        item.Title = "Title";
        item.Url = "https://example.com";
        item.Content = "Content";
        item.RawContent = "Raw Content";

        // Assert
        item.Title.Should().Be("Title");
        item.Url.Should().Be("https://example.com");
        item.Content.Should().Be("Content");
        item.RawContent.Should().Be("Raw Content");
    }

    [Theory]
    [InlineData("")]
    [InlineData("Simple title")]
    [InlineData("Title with 特殊文字 and emoji 🔍")]
    [InlineData("Very long title that might exceed normal limits and contains various characters including numbers 123")]
    public void SearchResultItem_Title_ShouldHandleVariousValues(string title)
    {
        // Arrange
        var item = new SearchResultItem();

        // Act
        item.Title = title;

        // Assert
        item.Title.Should().Be(title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("https://example.com")]
    [InlineData("https://example.com/path/to/resource?param=value")]
    [InlineData("ftp://ftp.example.com/file.txt")]
    public void SearchResultItem_Url_ShouldHandleVariousValues(string url)
    {
        // Arrange
        var item = new SearchResultItem();

        // Act
        item.Url = url;

        // Assert
        item.Url.Should().Be(url);
    }
}