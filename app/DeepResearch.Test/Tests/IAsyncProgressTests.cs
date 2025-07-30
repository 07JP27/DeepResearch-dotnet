using FluentAssertions;
using DeepResearch.Core.Models;

namespace DeepResearch.Tests;

/// <summary>
/// Unit tests for IAsyncProgress interface
/// </summary>
public class IAsyncProgressTests
{
    [Fact]
    public void IAsyncProgress_ShouldBeGenericInterface()
    {
        // Arrange & Act
        var interfaceType = typeof(IAsyncProgress<>);

        // Assert
        interfaceType.IsInterface.Should().BeTrue();
        interfaceType.IsGenericTypeDefinition.Should().BeTrue();
        interfaceType.GetGenericArguments().Length.Should().Be(1);
    }

    [Fact]
    public void IAsyncProgress_WithProgressBase_ShouldBeAssignable()
    {
        // Arrange & Act
        var progressInterfaceType = typeof(IAsyncProgress<ProgressBase>);

        // Assert
        progressInterfaceType.IsInterface.Should().BeTrue();
        progressInterfaceType.IsGenericType.Should().BeTrue();
        progressInterfaceType.GetGenericArguments()[0].Should().Be(typeof(ProgressBase));
    }

    private class TestAsyncProgress : IAsyncProgress<ProgressBase>
    {
        public List<ProgressBase> ReceivedProgress { get; } = new();
        public List<CancellationToken> ReceivedTokens { get; } = new();

        public ValueTask ReportAsync(ProgressBase value, CancellationToken cancellationToken = default)
        {
            ReceivedProgress.Add(value);
            ReceivedTokens.Add(cancellationToken);
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task TestAsyncProgress_ShouldReceiveProgressValues()
    {
        // Arrange
        var asyncProgress = new TestAsyncProgress();
        var progress1 = new ThinkingProgress { Message = "Test message 1" };
        var progress2 = new QueryGenerationProgress { Query = "Test query", Rationale = "Test rationale" };

        // Act
        await asyncProgress.ReportAsync(progress1);
        await asyncProgress.ReportAsync(progress2);

        // Assert
        asyncProgress.ReceivedProgress.Should().HaveCount(2);
        asyncProgress.ReceivedProgress[0].Should().BeOfType<ThinkingProgress>();
        asyncProgress.ReceivedProgress[1].Should().BeOfType<QueryGenerationProgress>();
        
        var thinkingProgress = (ThinkingProgress)asyncProgress.ReceivedProgress[0];
        thinkingProgress.Message.Should().Be("Test message 1");
        
        var queryProgress = (QueryGenerationProgress)asyncProgress.ReceivedProgress[1];
        queryProgress.Query.Should().Be("Test query");
        queryProgress.Rationale.Should().Be("Test rationale");
    }

    [Fact]
    public async Task TestAsyncProgress_ShouldReceiveCancellationTokens()
    {
        // Arrange
        var asyncProgress = new TestAsyncProgress();
        var progress = new ThinkingProgress { Message = "Test message" };
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await asyncProgress.ReportAsync(progress, token);

        // Assert
        asyncProgress.ReceivedTokens.Should().HaveCount(1);
        asyncProgress.ReceivedTokens[0].Should().Be(token);
    }

    [Fact]
    public async Task TestAsyncProgress_WithDefaultCancellationToken_ShouldReceiveDefaultToken()
    {
        // Arrange
        var asyncProgress = new TestAsyncProgress();
        var progress = new ThinkingProgress { Message = "Test message" };

        // Act
        await asyncProgress.ReportAsync(progress);

        // Assert
        asyncProgress.ReceivedTokens.Should().HaveCount(1);
        asyncProgress.ReceivedTokens[0].Should().Be(CancellationToken.None);
    }
}