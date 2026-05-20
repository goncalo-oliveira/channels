namespace Faactory.Channels;

/// <summary>
/// Interface for a channel handle, representing an active channel in the registry.
/// </summary>
public interface IChannelHandle
{
    /// <summary>
    /// Gets the unique identifier of the channel.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the name of the channel, which corresponds to the channel configuration name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the timestamp of when the channel was created
    /// </summary>
    DateTimeOffset Created { get; }

    /// <summary>
    /// Gets the timestamp of the last received message
    /// </summary>
    DateTimeOffset LastReceived { get; }

    /// <summary>
    /// Gets the timestamp of the last sent message
    /// </summary>
    DateTimeOffset LastSent { get; }

    /// <summary>
    /// Gets the total duration of the channel's existence
    /// </summary>
    TimeSpan Duration { get; }

    /// <summary>
    /// Gets the duration since the last received message
    /// </summary>
    TimeSpan IdleTime { get; }

    /// <summary>
    /// Gets the total number of bytes received
    /// </summary>
    long BytesReceived { get; }

    /// <summary>
    /// Gets the total number of bytes sent
    /// </summary>
    long BytesSent { get; }

    /// <summary>
    /// Gets the channel's ephemeral data
    /// </summary>
    IReadOnlyDictionary<string, object> Data { get; }

    /// <summary>
    /// Closes the underlying channel and releases any associated resources.
    /// </summary>
    /// <returns>A task that represents the asynchronous close operation.</returns>
    Task CloseAsync();
}
