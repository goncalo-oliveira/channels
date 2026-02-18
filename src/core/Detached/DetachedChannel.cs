using Faactory.Channels.Buffers;

namespace Faactory.Channels;

/// <summary>
/// A detached channel instance to be used outside a Channels context
/// </summary>
public sealed class DetachedChannel : IChannel
{
    private readonly List<IChannelService> services = [];

    /// <summary>
    /// Gets the unique identifier of the channel
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString( "N" );

    /// <summary>
    /// Gets the channel data, a dictionary of string keys and object values that can be used to store any relevant information about the channel
    /// </summary>
    public ChannelData Data { get; } = [];

    /// <summary>
    /// Gets the date and time when the channel was created
    /// </summary>
    public DateTimeOffset Created { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the date and time when the channel last received data. For a detached channel, this value is always null since it does not receive any data.
    /// </summary>
    public DateTimeOffset? LastReceived { get; }

    /// <summary>
    /// Gets the date and time when the channel last sent data. For a detached channel, this value is always null since it does not send any data.
    /// </summary>
    public DateTimeOffset? LastSent { get; }

    /// <summary>
    /// Gets a value indicating whether the channel is closed.
    /// </summary>
    public bool IsClosed { get; private set; }

    /// <summary>
    /// Closes the channel. For a detached channel, this method simply sets the IsClosed property to true and does not perform any other actions since it is not connected to any transport or context.
    /// </summary>
    public Task CloseAsync()
    {
        IsClosed = true;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Writes data to the channel. For a detached channel, this method does not perform any actual writing since it is not connected to any transport or context, but it can be used to simulate the behavior of a real channel and to test the functionality of channel services that depend on the WriteAsync method.
    /// </summary>
    public Task WriteAsync( object data )
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the channel.
    /// </summary>
    public void Dispose()
    { }

    /// <summary>
    /// Asynchronously disposes the channel.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    internal void AddChannelService( IChannelService service )
    {
        services.Add( service );
    }

    /// <summary>
    /// Gets a channel service of the specified type.
    /// </summary>
    public IChannelService? GetChannelService( Type serviceType )
        => services.SingleOrDefault( s => s.GetType() == serviceType );
}
