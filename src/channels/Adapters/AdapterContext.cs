using Faactory.Channels.Buffers;

namespace Faactory.Channels.Adapters;

internal class AdapterContext( IChannel channel, IByteBufferPool? bufferPool ) : ChannelContext( channel, bufferPool ), IAdapterContext
{
    private readonly List<object> forwardedData = [];

    public void Forward( object data )
    {
        forwardedData.Add( data );
    }

    internal object[] Flush()
    {
        var result = forwardedData.ToArray();

        forwardedData.Clear();

        return result;
    }
}
