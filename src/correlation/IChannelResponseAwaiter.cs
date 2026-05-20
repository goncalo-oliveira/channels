namespace Faactory.Channels.Correlation;

/// <summary>
/// Defines an interface for awaiting a response from a channel based on a correlation predicate.
/// </summary>
/// <typeparam name="T">The type of the message to await.</typeparam>
public interface IChannelResponseAwaiter<T>
{
    /// <summary>
    /// Waits asynchronously for a message that matches the correlation predicate.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the wait operation.</param>
    /// <returns>A task that represents the asynchronous wait operation. The task result contains the awaited message.</returns>
    Task<T> WaitAsync( CancellationToken cancellationToken = default );
}
