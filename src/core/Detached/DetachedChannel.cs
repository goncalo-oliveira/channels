using Faactory.Channels.Buffers;

namespace Faactory.Channels;

/// <summary>
/// A detached channel instance to be used outside a Channels context
/// </summary>
public sealed class DetachedChannel : IChannel
{
    private readonly List<IChannelService> services = [];

    public string Id { get; } = Guid.NewGuid().ToString( "N" );

    public ChannelData Data { get; } = [];

    public DateTimeOffset Created { get; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastReceived { get; }

    public DateTimeOffset? LastSent { get; }

    public bool IsClosed { get; private set; }

    public Task CloseAsync()
    {
        IsClosed = true;

        return Task.CompletedTask;
    }

    public Task WriteAsync( object data )
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    { }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    internal void AddChannelService( IChannelService service )
    {
        services.Add( service );
    }

    public IChannelService? GetChannelService( Type serviceType )
        => services.SingleOrDefault( s => s.GetType() == serviceType );
}
