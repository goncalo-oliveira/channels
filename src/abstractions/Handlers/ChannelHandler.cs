using System.Collections;
using Faactory.Channels.Buffers;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels.Handlers;

/// <summary>
/// Base class for a channel handler
/// </summary>
/// <typeparam name="T">The data type</typeparam>
public abstract class ChannelHandler<T> : ChannelMiddleware<T>, IChannelHandler
{
    public ChannelHandler()
    {}

    public ChannelHandler( ILoggerFactory loggerFactory )
    : base( loggerFactory )
    { }

    protected override void OnDataNotSuitable( IChannelContext context, object data )
    {
        Logger?.LogDebug( $"Data type '{data.GetType().Name}' is not suitable for this handler." );
    }
}
