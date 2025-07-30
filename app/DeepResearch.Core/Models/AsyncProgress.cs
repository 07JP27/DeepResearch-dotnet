namespace DeepResearch.Core.Models;

/// <summary>
/// Wraps an IProgress&lt;T&gt; instance to provide IAsyncProgress&lt;T&gt; functionality.
/// This allows existing synchronous progress handlers to work with async progress reporting.
/// </summary>
/// <typeparam name="T">The type of progress update value.</typeparam>
public class AsyncProgress<T> : IAsyncProgress<T>
{
    private readonly IProgress<T>? _progress;

    /// <summary>
    /// Initializes a new instance of the AsyncProgress class.
    /// </summary>
    /// <param name="progress">The IProgress instance to wrap. Can be null.</param>
    public AsyncProgress(IProgress<T>? progress)
    {
        _progress = progress;
    }

    /// <summary>
    /// Reports a progress update by calling the wrapped IProgress.Report method.
    /// </summary>
    /// <param name="value">The value of the updated progress.</param>
    /// <param name="cancellationToken">A cancellation token (not used in this implementation).</param>
    /// <returns>A completed ValueTask.</returns>
    public ValueTask ReportAsync(T value, CancellationToken cancellationToken = default)
    {
        _progress?.Report(value);
        return ValueTask.CompletedTask;
    }
}