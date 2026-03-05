using Faactory.Channels.Buffers;

namespace Faactory.Channels;

internal class ChannelContext( IChannel channel, IByteBufferPool? bufferPool ) : IChannelContext
{
    public IByteBufferPool BufferPool => bufferPool ?? new ByteBufferPool();

    public IChannel Channel => channel;

    public Endianness BufferEndianness => Channel is Channel c
        ? c.Buffer.Endianness
        : Endianness.BigEndian;

    public IWritableBuffer Output { get; } = new WritableBuffer();

    public void NotifyCustomEvent( string name, object? data )
    {
        if ( Channel is Channel channel )
        {
            channel.NotifyCustomEvent( name, data );
        }
    }
}
