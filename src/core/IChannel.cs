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
    /// Gets the name of the channel, which corresponds to the channel configuration name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets if the channel is closed
    /// </summary>
    bool IsClosed { get; }

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
    /// Gets the scoped service provider associated with the channel lifetime.
    /// Services resolved from this provider are scoped to the current channel instance.
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>
    /// Gets the channel services initialized for the current channel instance.
    /// </summary>
    public IEnumerable<IChannelService> ChannelServices { get; }

    /// <summary>
    /// Sends data through the Output pipeline
    /// </summary>
    Task WriteAsync( object data );

    /// <summary>
    /// Closes the channel
    /// </summary>
    Task CloseAsync();
}
