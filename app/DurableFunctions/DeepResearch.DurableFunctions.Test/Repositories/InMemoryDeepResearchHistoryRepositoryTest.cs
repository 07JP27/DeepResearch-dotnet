using DeepResearch.Core.Models;
using DeepResearch.DurableFunctions.Repositories;
using System.Threading;
using Xunit;

namespace DeepResearch.DurableFunctions.Test.Repositories;

public class InMemoryDeepResearchHistoryRepositoryTest
{
    [Fact]
    public async Task AddAndGetProgressRoundtripWorks()
    {
        var repo = new InMemoryDeepResearchHistoryRepository();
        var key = new ProgressKey("session-1", "progress-1");
        var progress = new ThinkingProgress { Message = "hello", Timestamp = DateTimeOffset.UtcNow };

        await repo.AddProgressAsync(key, progress);
        var loaded = await repo.GetProgressAsync(key);

        Assert.NotNull(loaded);
        Assert.IsType<ThinkingProgress>(loaded);
        Assert.Equal(progress.Timestamp, loaded!.Timestamp);
        Assert.Equal(progress.Type, loaded.Type);
    }

    [Fact]
    public async Task GetProgressReturnsNullForMissingKey()
    {
        var repo = new InMemoryDeepResearchHistoryRepository();
        var key = new ProgressKey("session-x", "progress-y");

        var loaded = await repo.GetProgressAsync(key);

        Assert.Null(loaded);
    }

    [Fact]
    public async Task GetProgressesBySessionReturnsItemsOrderedByTimestamp()
    {
        var repo = new InMemoryDeepResearchHistoryRepository();
        var sessionId = "session-ordered";
        var t0 = DateTimeOffset.UtcNow.AddMinutes(-3);
        var t1 = DateTimeOffset.UtcNow.AddMinutes(-2);
        var t2 = DateTimeOffset.UtcNow.AddMinutes(-1);

        await repo.AddProgressAsync(new ProgressKey(sessionId, "p2"), new ThinkingProgress { Message = "2", Timestamp = t2 });
        await repo.AddProgressAsync(new ProgressKey(sessionId, "p0"), new ThinkingProgress { Message = "0", Timestamp = t0 });
        await repo.AddProgressAsync(new ProgressKey(sessionId, "p1"), new ThinkingProgress { Message = "1", Timestamp = t1 });

        var list = await repo.GetProgressesBySessionIdAsync(sessionId);

        Assert.Equal(3, list.Length);
        Assert.True(list[0].Timestamp <= list[1].Timestamp);
        Assert.True(list[1].Timestamp <= list[2].Timestamp);
        Assert.Equal(t0, list[0].Timestamp);
        Assert.Equal(t1, list[1].Timestamp);
        Assert.Equal(t2, list[2].Timestamp);
    }

    [Fact]
    public async Task GetProgressesBySessionUnknownReturnsEmpty()
    {
        var repo = new InMemoryDeepResearchHistoryRepository();
        var list = await repo.GetProgressesBySessionIdAsync("unknown");
        Assert.NotNull(list);
        Assert.Empty(list);
    }

    [Fact]
    public async Task AddProgressUpsertsOnSameProgressId()
    {
        var repo = new InMemoryDeepResearchHistoryRepository();
        var sessionId = "session-upsert";
        var key = new ProgressKey(sessionId, "dup");

        var first = new ThinkingProgress { Message = "first", Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5) };
        var second = new ThinkingProgress { Message = "second", Timestamp = DateTimeOffset.UtcNow };

        await repo.AddProgressAsync(key, first);
        await repo.AddProgressAsync(key, second);

        var loaded = await repo.GetProgressAsync(key);
        Assert.NotNull(loaded);
        Assert.Equal(second.Timestamp, loaded!.Timestamp);

        var all = await repo.GetProgressesBySessionIdAsync(sessionId);
        Assert.Single(all);
        Assert.Equal(second.Timestamp, all[0].Timestamp);
    }

    // When either SessionId or ProgressId is null, expect ArgumentNullException
    [Theory]
    [InlineData(null, "p1")]
    [InlineData("s1", null)]
    public async Task AddProgressThrowsOnInvalidKeyWhenValueIsNull(string? sessionId, string? progressId)
    {
        var repo = new InMemoryDeepResearchHistoryRepository();
        var key = new ProgressKey(sessionId!, progressId!);
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await repo.AddProgressAsync(key, new ThinkingProgress()));
    }

    // When either SessionId or ProgressId is empty or whitespace, expect ArgumentException
    [Theory]
    [InlineData("", "p1")]
    [InlineData(" ", "p1")]
    [InlineData("s1", "")]
    [InlineData("s1", " ")]
    public async Task AddProgressThrowsOnInvalidKeyWhenValueIsEmptyOrWhitespace(string? sessionId, string? progressId)
    {
        var repo = new InMemoryDeepResearchHistoryRepository();
        var key = new ProgressKey(sessionId!, progressId!);
        await Assert.ThrowsAsync<ArgumentException>(async () => await repo.AddProgressAsync(key, new ThinkingProgress()));
    }

    [Fact]
    public async Task AddProgressThrowsOnNullKey()
    {
        var repo = new InMemoryDeepResearchHistoryRepository();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await repo.AddProgressAsync(null!, new ThinkingProgress()));
    }

    [Fact]
    public async Task AddProgressThrowsOnNullProgress()
    {
        var repo = new InMemoryDeepResearchHistoryRepository();
        var key = new ProgressKey("s1", "p1");
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await repo.AddProgressAsync(key, null!));
    }

    // GetProgress: null -> ArgumentNullException, empty/whitespace -> ArgumentException
    [Theory]
    [InlineData(null, "p1")]
    [InlineData("s1", null)]
    public async Task GetProgressThrowsOnInvalidKeyWhenValueIsNull(string? sessionId, string? progressId)
    {
        var repo = new InMemoryDeepResearchHistoryRepository();
        var key = new ProgressKey(sessionId!, progressId!);
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await repo.GetProgressAsync(key));
    }

    [Theory]
    [InlineData("", "p1")]
    [InlineData(" ", "p1")]
    [InlineData("s1", "")]
    [InlineData("s1", " ")]
    public async Task GetProgressThrowsOnInvalidKeyWhenValueIsEmptyOrWhitespace(string? sessionId, string? progressId)
    {
        var repo = new InMemoryDeepResearchHistoryRepository();
        var key = new ProgressKey(sessionId!, progressId!);
        await Assert.ThrowsAsync<ArgumentException>(async () => await repo.GetProgressAsync(key));
    }

    [Fact]
    public async Task GetProgressThrowsOnNullKey()
    {
        var repo = new InMemoryDeepResearchHistoryRepository();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await repo.GetProgressAsync(null!));
    }

    // null -> ArgumentNullException, empty/whitespace -> ArgumentException
    [Theory]
    [InlineData(null)]
    public async Task GetProgressesBySessionThrowsOnInvalidSessionWhenValueIsNull(string? sessionId)
    {
        var repo = new InMemoryDeepResearchHistoryRepository();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await repo.GetProgressesBySessionIdAsync(sessionId!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetProgressesBySessionThrowsOnInvalidSessionWhenValueIsEmptyOrWhitespace(string? sessionId)
    {
        var repo = new InMemoryDeepResearchHistoryRepository();
        await Assert.ThrowsAsync<ArgumentException>(async () => await repo.GetProgressesBySessionIdAsync(sessionId!));
    }

    [Fact]
    public async Task MethodsRespectCancellationToken()
    {
        var repo = new InMemoryDeepResearchHistoryRepository();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await repo.AddProgressAsync(new ProgressKey("s", "p"), new ThinkingProgress(), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await repo.GetProgressAsync(new ProgressKey("s", "p"), cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await repo.GetProgressesBySessionIdAsync("s", cts.Token));
    }
}
