
using System.Collections.Concurrent;

namespace Faactory.Channels;

internal sealed class ChannelLimiter( int connectionLimit ) : IChannelMonitor, IChannelLimiter
{
    private int currentConnections = 0;
    private readonly ConcurrentDictionary<string, bool> connections = new();

    private bool Accept()
    {
        Interlocked.Increment( ref currentConnections );

        if ( connectionLimit <= 0 )
        {
            return true;
        }

        return currentConnections <= connectionLimit;
    }

    public bool IsAdmitted( IChannel channel )
        => connections.TryGetValue( channel.Id, out var accepted ) && accepted;

    public void ChannelClosed( IChannelInfo channelInfo )
    {
        if ( currentConnections > 0 )
        {
            Interlocked.Decrement( ref currentConnections );
        }

        connections.TryRemove( channelInfo.Id, out _ );
    }

    public void ChannelCreated( IChannelInfo channelInfo )
    {
        connections.TryAdd( channelInfo.Id, Accept() );
    }

    public void CustomEvent( IChannelInfo channelInfo, string name, object? data )
    { }

    public void DataReceived( IChannelInfo channelInfo, ReadOnlySpan<byte> data )
    { }

    public void DataSent( IChannelInfo channelInfo, ReadOnlySpan<byte> data )
    { }
}
