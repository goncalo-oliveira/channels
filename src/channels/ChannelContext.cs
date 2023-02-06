namespace Faactory.Channels;

internal class ChannelContext : IChannelContext
{
    public ChannelContext( IChannel channel )
    {
        Channel = channel;
        Output = new WritableBuffer();
    }

    public IChannel Channel { get; }

    public IWritableBuffer Output { get; }

    public void NotifyCustomEvent( string name, object? data )
    {
        if ( Channel is Channel )
        {
            ((Channel)Channel).NotifyCustomEvent( name, data );
        }
    }
}
