using Faactory.Channels.Buffers;
using Faactory.Collections;

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
    /// Gets a data holder available throughout the entire channel session
    /// </summary>
    IMetadata Data { get; }

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
    /// Sends data through the Output pipeline
    /// </summary>
    Task WriteAsync( object data );

    /// <summary>
    /// Closes the channel
    /// </summary>
    Task CloseAsync();
}
