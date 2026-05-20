using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Faactory.Channels;

internal sealed class ChannelRegistry : IChannelRegistrar, IChannelRegistry, IChannelMonitor
{
    private readonly ConcurrentDictionary<string, ChannelHandle> channels = new();

    public IReadOnlyCollection<IChannelHandle> Channels => (IReadOnlyCollection<IChannelHandle>)channels.Values;

    public bool TryGet( string channelId, [MaybeNullWhen(false)] out IChannelHandle handle )
    {
        if ( channels.TryGetValue( channelId, out var channelHandle ) )
        {
            handle = channelHandle;

            return true;
        }

        handle = null;

        return false;
    }

    public void Register( IChannel channel )
    {
        channels.TryAdd( channel.Id, new ChannelHandle( channel ) );
    }

    #region IChannelMonitor

    public void ChannelClosed( IChannelInfo channelInfo )
    {
        channels.TryRemove( channelInfo.Id, out _ );
    }

    public void ChannelCreated( IChannelInfo channelInfo )
    {
        // do nothing.
        // registration is done explicitly since monitor events don't provide the channel instance.
    }

    public void CustomEvent( IChannelInfo channelInfo, string name, object? data )
    { }

    public void DataReceived( IChannelInfo channelInfo, ReadOnlySpan<byte> data )
    {
        if ( channels.TryGetValue( channelInfo.Id, out var handle ) )
        {
            handle.LastReceived = DateTimeOffset.UtcNow;
            handle.BytesReceived += data.Length;
        }
    }

    public void DataSent( IChannelInfo channelInfo, ReadOnlySpan<byte> data )
    {
        if ( channels.TryGetValue( channelInfo.Id, out var handle ) )
        {
            handle.LastSent = DateTimeOffset.UtcNow;
            handle.BytesSent += data.Length;
        }
    }

    #endregion
}
