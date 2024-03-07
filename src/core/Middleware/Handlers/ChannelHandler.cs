namespace Faactory.Channels.Handlers;

/// <summary>
/// Base class for a channel handler
/// </summary>
/// <typeparam name="T">The data type</typeparam>
public abstract class ChannelHandler<T> : ChannelMiddleware<T>, IChannelHandler
{
    public ChannelHandler()
    {}

    protected override void OnDataNotSuitable( IChannelContext context, object data )
    {
        // do nothing
    }
}
