namespace Faactory.Channels.Handlers;

/// <summary>
/// Base class for a channel handler
/// </summary>
/// <typeparam name="T">The data type</typeparam>
public abstract class ChannelHandler<T> : ChannelMiddleware<T>, IChannelHandler
{
    /// <summary>
    /// Called when the data is not suitable for this handler
    /// </summary>
    protected override void OnDataNotSuitable( IChannelContext context, object data )
    {
        // do nothing
    }
}
