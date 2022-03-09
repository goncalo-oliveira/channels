using Microsoft.Extensions.Logging;

namespace Parcel.Channels.Handlers;

/// <summary>
/// Base class for a channel handler
/// </summary>
/// <typeparam name="T">The data type</typeparam>
public abstract class ChannelHandler<T> : IChannelHandler
{
    private ILogger? logger;

    public ChannelHandler()
    {}

    public ChannelHandler( ILoggerFactory loggerFactory )
    {
        logger = loggerFactory.CreateLogger( GetType() );
    }

    public abstract Task ExecuteAsync( IChannelContext context, T data );

    public Task ExecuteAsync( IChannelContext context, object data )
    {
        logger = logger ?? context.LoggerFactory.CreateLogger( GetType() );

        logger.LogDebug( $"Received '{data.GetType().Name}' data." );

        if ( data == null )
        {
            // no data...
            return Task.CompletedTask;
        }

        if ( data is T )
        {
            // execute type implementation
            return ExecuteAsync( context, (T)data );
        }

        // data when not suitable for this handler
        logger.LogDebug( $"Data type '{data.GetType().Name}' is not suitable for this handler." );
        
        return Task.CompletedTask;
    }
}
