using DeepResearch.Core.Models;
using System.Collections.Concurrent;

namespace DeepResearch.DurableFunctions.Repositories;
public class InMemoryDeepResearchHistoryRepository : IDeepResearchHistoryRepository
{
    // sessionId -> (progressId -> progress)
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ProgressBase>> _store =
        new(StringComparer.Ordinal);

    public ValueTask AddProgressAsync(ProgressKey progressKey, ProgressBase progress, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentNullException.ThrowIfNull(progressKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(progressKey.SessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(progressKey.ProgressId);
        ArgumentNullException.ThrowIfNull(progress);

        var sessionDict = _store.GetOrAdd(progressKey.SessionId, static _ => new ConcurrentDictionary<string, ProgressBase>(StringComparer.Ordinal));
        // Upsert behavior: overwrite if same progressId exists
        sessionDict[progressKey.ProgressId] = progress;

        return ValueTask.CompletedTask;
    }

    public ValueTask<ProgressBase?> GetProgressAsync(ProgressKey progressKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentNullException.ThrowIfNull(progressKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(progressKey.SessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(progressKey.ProgressId);

        if (!_store.TryGetValue(progressKey.SessionId, out var sessionDict) ||
            !sessionDict.TryGetValue(progressKey.ProgressId, out var progress))
        {
            return ValueTask.FromResult<ProgressBase?>(null);
        }

        return ValueTask.FromResult<ProgressBase?>(progress);
    }

    public ValueTask<ProgressBase[]> GetProgressesBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        if (!_store.TryGetValue(sessionId, out var sessionDict))
        {
            return ValueTask.FromResult(Array.Empty<ProgressBase>());
        }

        // Order by timestamp to provide deterministic ordering
        var results = sessionDict.Values
            .OrderBy(p => p.Timestamp)
            .ToArray();

        return ValueTask.FromResult(results);
    }
}
