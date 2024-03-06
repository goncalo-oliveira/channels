namespace Faactory.Channels;

internal class ChannelContext( IChannel channel ) : IChannelContext
{
    public IChannel Channel { get; } = channel;

    public IWritableBuffer Output { get; } = new WritableBuffer();

    public void NotifyCustomEvent( string name, object? data )
    {
        if ( Channel is Channel channel )
        {
            channel.NotifyCustomEvent( name, data );
        }
    }
}
