namespace Faactory.Channels.Correlation;

/// <summary>
/// Defines the contract for a registry of channel response awaiters.
/// </summary>
public interface IChannelResponseRegistry
{
    /// <summary>
    /// Creates a new channel response awaiter for the specified type and predicate.
    /// </summary>
    /// <typeparam name="T">The type of the message to await.</typeparam>
    /// <param name="predicate">A function that defines the condition to match the awaited message.</param>
    /// <returns>An instance of <see cref="IChannelResponseAwaiter{T}"/> that can be used to await a message of the specified type that matches the given predicate.</returns>
    IChannelResponseAwaiter<T> Create<T>( Func<T, bool> predicate );

    /// <summary>
    /// Pushes a message to the registry, which will be evaluated against the predicates of the registered awaiters. If a match is found, the corresponding awaiter will be completed with the message.
    /// </summary>
    /// <typeparam name="T">The type of the message being pushed.</typeparam>
    /// <param name="message">The message to push to the registry.</param>
    void Push<T>( T message );
}
