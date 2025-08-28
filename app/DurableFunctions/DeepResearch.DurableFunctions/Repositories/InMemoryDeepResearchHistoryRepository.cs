using DeepResearch.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DeepResearch.DurableFunctions.Repositories;
public class InMemoryDeepResearchHistoryRepository : IDeepResearchHistoryRepository
{
    // sessionId -> (progressId -> progress)
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ProgressBase>> _store =
        new(StringComparer.Ordinal);

    public ValueTask AddProgressAsync(string sessionId, string progressId, ProgressBase progress, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(progressId);
        ArgumentNullException.ThrowIfNull(progress);

        var sessionDict = _store.GetOrAdd(sessionId, static _ => new ConcurrentDictionary<string, ProgressBase>(StringComparer.Ordinal));
        // Upsert behavior: overwrite if same progressId exists
        sessionDict[progressId] = progress;

        return ValueTask.CompletedTask;
    }

    public ValueTask<ProgressBase> GetProgressAsync(string sessionId, string progressId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(progressId);

        if (!_store.TryGetValue(sessionId, out var sessionDict) ||
            !sessionDict.TryGetValue(progressId, out var progress))
        {
            throw new KeyNotFoundException($"Progress not found. sessionId='{sessionId}', progressId='{progressId}'.");
        }

        return ValueTask.FromResult(progress);
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
