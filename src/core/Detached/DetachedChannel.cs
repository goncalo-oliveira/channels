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

    public IByteBuffer Buffer { get; } = new WrappedByteBuffer( [] );

    public bool IsClosed { get; private set; }

    public IEnumerable<IChannelService> Services => services.ToArray();

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

    internal void AddChannelService( IChannelService service )
    {
        services.Add( service );
    }
}
