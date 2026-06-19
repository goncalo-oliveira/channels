

namespace Faactory.Channels;

internal sealed class ChannelHandle( IChannel channel ) : IChannelHandle
{
    public string Id => channel.Id;

    public string Name => channel.Name;

    public DateTimeOffset Created => channel.Created;

    public DateTimeOffset LastReceived { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset LastSent { get; set; } = DateTimeOffset.UtcNow;

    public TimeSpan Duration => DateTimeOffset.UtcNow - Created;

    public TimeSpan IdleTime => DateTimeOffset.UtcNow - ( LastReceived > LastSent ? LastReceived : LastSent );

    public long BytesReceived { get; set; }

    public long BytesSent { get; set; }

    public IReadOnlyDictionary<string, object> Data => channel.Data.AsReadOnly();

    public Task CloseAsync()
        => channel.CloseAsync();

    public Task WriteAsync( object data )
        => channel.WriteAsync( data );
}
