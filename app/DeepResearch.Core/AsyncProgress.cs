namespace DeepResearch.Core;

/// <summary>
/// Provides static factory methods for creating AsyncProgress instances with type inference.
/// </summary>
public static class AsyncProgress
{
    /// <summary>
    /// Creates an AsyncProgress instance that wraps a Func&lt;T, Task&gt; delegate.
    /// </summary>
    /// <typeparam name="T">The type of progress update value.</typeparam>
    /// <param name="progressFunc">The async function to wrap.</param>
    /// <returns>An AsyncProgress instance that wraps the provided function.</returns>
    public static AsyncProgress<T> FromFunc<T>(Func<T, Task> progressFunc)
    {
        ArgumentNullException.ThrowIfNull(progressFunc);
        return new AsyncProgress<T>((value, _) => progressFunc(value));
    }

    /// <summary>
    /// Creates an AsyncProgress instance that wraps a Func&lt;T, CancellationToken, Task&gt; delegate.
    /// </summary>
    /// <typeparam name="T">The type of progress update value.</typeparam>
    /// <param name="progressFunc">The async function to wrap.</param>
    /// <returns>An AsyncProgress instance that wraps the provided function.</returns>
    public static AsyncProgress<T> FromFunc<T>(Func<T, CancellationToken, Task> progressFunc)
    {
        ArgumentNullException.ThrowIfNull(progressFunc);
        return new AsyncProgress<T>(progressFunc);
    }

    /// <summary>
    /// Creates an AsyncProgress instance that wraps a Func&lt;T, CancellationToken, Task&gt; delegate.
    /// </summary>
    /// <typeparam name="T">The type of progress update value.</typeparam>
    /// <param name="progressFunc">The async function to wrap.</param>
    /// <returns>An AsyncProgress instance that wraps the provided function.</returns>
    public static AsyncProgress<T> Create<T>(Func<T, CancellationToken, Task> progressFunc)
    {
        return new AsyncProgress<T>(progressFunc);
    }
}

/// <summary>
/// Extension methods for IProgress&lt;T&gt; to convert to IAsyncProgress&lt;T&gt;.
/// </summary>
public static class ProgressExtensions
{
    /// <summary>
    /// Converts an IProgress&lt;T&gt; to an IAsyncProgress&lt;T&gt; for efficient synchronous progress reporting.
    /// </summary>
    /// <typeparam name="T">The type of progress update value.</typeparam>
    /// <param name="progress">The IProgress instance to convert. Can be null.</param>
    /// <returns>An IAsyncProgress instance that efficiently handles IProgress reporting.</returns>
    public static IAsyncProgress<T> ToAsyncProgress<T>(this IProgress<T>? progress)
    {
        if (progress == null)
        {
            return AsyncProgress<T>.Empty;
        }

        // Efficient wrapper for IProgress - no async overhead
        return new AsyncProgress<T>((value, _) =>
        {
            progress.Report(value);
            return Task.CompletedTask;
        });
    }
}

/// <summary>
/// Provides asynchronous progress reporting by wrapping a Func&lt;T, CancellationToken, Task&gt; delegate.
/// </summary>
/// <typeparam name="T">The type of progress update value.</typeparam>
public class AsyncProgress<T> : IAsyncProgress<T>
{
    private readonly Func<T, CancellationToken, Task>? _progressFunc;

    /// <summary>
    /// A shared empty instance that performs no operation when progress is reported.
    /// This instance is used for null progress scenarios to avoid unnecessary allocations.
    /// </summary>
    public static readonly IAsyncProgress<T> Empty = new AsyncProgress<T>(null);

    /// <summary>
    /// Initializes a new instance of the AsyncProgress class.
    /// </summary>
    /// <param name="progressFunc">The async function to invoke for progress reporting. Can be null.</param>
    public AsyncProgress(Func<T, CancellationToken, Task>? progressFunc)
    {
        _progressFunc = progressFunc;
    }

    /// <summary>
    /// Reports a progress update by invoking the wrapped async function.
    /// </summary>
    /// <param name="value">The value of the updated progress.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the progress reporting operation.</param>
    /// <returns>A ValueTask representing the asynchronous progress reporting operation.</returns>
    public async ValueTask ReportAsync(T value, CancellationToken cancellationToken = default)
    {
        if (_progressFunc != null)
        {
            await _progressFunc(value, cancellationToken).ConfigureAwait(false);
        }
    }
}