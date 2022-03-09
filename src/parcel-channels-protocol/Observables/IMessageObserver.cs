using Parcel.Protocol;

namespace Parcel.Channels.Observables;

/// <summary>
/// An interface to observe message identifiers
/// </summary>
public interface IMessageObserver
{
    /// <summary>
    /// Waits for a message with the given identifier to be pushed
    /// </summary>
    /// <param name="messageId">The message identifier to wait for</param>
    /// <param name="timeout">The time to wait for the message before timing out</param>
    /// <returns>A message instance or null if timed out</returns>
    Task<Message?> WaitForAsync( string messageId, TimeSpan timeout );

    /// <summary>
    /// Pushes a message through the observer releasing and pending locks
    /// </summary>
    /// <param name="message">The message to deliver</param>
    void Push( Message message );
}
