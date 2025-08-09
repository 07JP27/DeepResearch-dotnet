namespace DeepResearch.Core;

/// <summary>
/// Provides support for asynchronous progress reporting with cancellation support.
/// </summary>
/// <typeparam name="T">The type of progress update value.</typeparam>
public interface IAsyncProgress<in T>
{
    /// <summary>
    /// Reports a progress update asynchronously.
    /// </summary>
    /// <param name="value">The value of the updated progress.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the progress reporting.</param>
    /// <returns>A task that represents the asynchronous progress reporting operation.</returns>
    ValueTask ReportAsync(T value, CancellationToken cancellationToken = default);
}