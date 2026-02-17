namespace Faactory.Channels;

/// <summary>
/// A communication channel
/// </summary>
public interface IChannel : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the identifier of the channel
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets if the channel is closed
    /// </summary>
    bool IsClosed { get; }

    /// <summary>
    /// Gets the channel's buffer endianness
    /// </summary>
    Buffers.Endianness BufferEndianness { get; }

    /// <summary>
    /// Gets a data holder available throughout the entire channel session
    /// </summary>
    ChannelData Data { get; }

    /// <summary>
    /// Gets channel creation date
    /// </summary>
    DateTimeOffset Created { get; }

    /// <summary>
    /// Gets last received data timestamp
    /// </summary>
    DateTimeOffset? LastReceived { get; }

    /// <summary>
    /// Gets last sent data timestamp
    /// </summary>
    DateTimeOffset? LastSent { get; }

    /// <summary>
    /// Gets a channel service of the specified type, or null if not found
    /// </summary>
    /// <param name="serviceType">The type of the channel service to retrieve</param>
    /// <returns>>The channel service instance if found, or null if not found</returns>
    IChannelService? GetChannelService( Type serviceType );

    /// <summary>
    /// Sends data through the Output pipeline
    /// </summary>
    Task WriteAsync( object data );

    /// <summary>
    /// Closes the channel
    /// </summary>
    Task CloseAsync();
}
