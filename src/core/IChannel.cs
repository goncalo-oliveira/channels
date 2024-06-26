using Faactory.Channels.Buffers;

namespace Faactory.Channels;

/// <summary>
/// A communication channel
/// </summary>
public interface IChannel : IDisposable
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
    /// Gets the channel's input buffer
    /// </summary>
    IByteBuffer Buffer { get; }

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
    /// Gets long-running channel services
    /// </summary>
    IEnumerable<IChannelService> Services { get; }

    /// <summary>
    /// Sends data through the Output pipeline
    /// </summary>
    Task WriteAsync( object data );

    /// <summary>
    /// Closes the channel
    /// </summary>
    Task CloseAsync();
}
