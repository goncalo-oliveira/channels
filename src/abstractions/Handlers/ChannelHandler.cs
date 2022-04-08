using System.Collections;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels.Handlers;

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

        var type = typeof( T );
        var dataType = data.GetType();

        // T is not an enumerable but data is enumerable<T>
        // deliver enumerable<T>.T sequentially
        if ( !type.IsEnumerable() && dataType.IsEnumerable<T>() )
        {
            var tasks = ( (IEnumerable)data ).OfType<T>()
                .Select( x => ExecuteAsync( context, x ) );

            return Task.WhenAll( tasks );
        }

        // T is an enumerable but data is enumerable.type
        // deliver data wrapped in an array
        if ( type.IsEnumerable() && dataType.Equals( type.GetElementType()! ) )
        {
            var array = Array.CreateInstance( type.GetElementType()!, 1 );
            array.SetValue( data, 0 );

            return ExecuteAsync( context, array );
        }

        // data when not suitable for this handler
        logger.LogDebug( $"Data type '{data.GetType().Name}' is not suitable for this handler." );
        
        return Task.CompletedTask;
    }
}
