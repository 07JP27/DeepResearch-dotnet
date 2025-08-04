using FluentAssertions;
using DeepResearch.Core.Models;
using DeepResearch.Core;

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

    [Fact]
    public void AsyncProgress_Empty_ShouldReturnSameInstance()
    {
        // Arrange & Act
        var empty1 = AsyncProgress<ProgressBase>.Empty;
        var empty2 = AsyncProgress<ProgressBase>.Empty;

        // Assert
        empty1.Should().BeSameAs(empty2);
        empty1.Should().NotBeNull();
    }

    [Fact]
    public async Task AsyncProgress_Empty_ShouldNotThrowWhenReporting()
    {
        // Arrange
        var empty = AsyncProgress<ProgressBase>.Empty;
        var progress = new ThinkingProgress { Message = "Test message" };

        // Act & Assert - Should not throw
        await empty.ReportAsync(progress);
        await empty.ReportAsync(progress, CancellationToken.None);
    }

    [Fact]
    public void ToAsyncProgress_WithNullProgress_ShouldReturnEmptyInstance()
    {
        // Arrange
        IProgress<ProgressBase>? nullProgress = null;

        // Act
        var asyncProgress = nullProgress.ToAsyncProgress();

        // Assert
        asyncProgress.Should().BeSameAs(AsyncProgress<ProgressBase>.Empty);
    }

    [Fact]
    public void ToAsyncProgress_WithValidProgress_ShouldReturnNewInstance()
    {
        // Arrange
        var receivedValues = new List<ProgressBase>();
        var progress = new Progress<ProgressBase>(receivedValues.Add);

        // Act
        var asyncProgress = progress.ToAsyncProgress();

        // Assert
        asyncProgress.Should().NotBeSameAs(AsyncProgress<ProgressBase>.Empty);
        asyncProgress.Should().NotBeNull();
    }

    [Fact]
    public async Task ToAsyncProgress_WithValidProgress_ShouldForwardCalls()
    {
        // Arrange
        var receivedValues = new List<ProgressBase>();
        var progress = new TestProgress<ProgressBase>(receivedValues.Add);
        var asyncProgress = progress.ToAsyncProgress();
        var testProgress = new ThinkingProgress { Message = "Test message" };

        // Act
        await asyncProgress.ReportAsync(testProgress);

        // Assert
        receivedValues.Should().HaveCount(1);
        receivedValues[0].Should().BeSameAs(testProgress);
    }

    private class TestProgress<T> : IProgress<T>
    {
        private readonly Action<T> _handler;

        public TestProgress(Action<T> handler)
        {
            _handler = handler;
        }

        public void Report(T value)
        {
            _handler(value);
        }
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