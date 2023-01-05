using System.Collections;
using Faactory.Channels.Buffers;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels;

/// <summary>
/// Base class for channel middleware. Do not inherit from this class directly.
/// </summary>
/// <typeparam name="T">The data type</typeparam>
public abstract class ChannelMiddleware<T>
{
    public ChannelMiddleware()
    { }

    public ChannelMiddleware( ILoggerFactory loggerFactory )
    {
        Logger = loggerFactory.CreateLogger( GetType() );
    }

    public ChannelMiddleware( ILogger logger )
    {
        Logger = logger;
    }

    protected ILogger? Logger { get; private set;}

    protected virtual void OnDataNotSuitable( IChannelContext context, object data )
    {
        Logger?.LogDebug( $"Data type '{data.GetType().Name}' is not suitable for this middleware." );
    }

    public abstract Task ExecuteAsync( IChannelContext context, T data );

    public Task ExecuteAsync( IChannelContext context, object data )
    {
        Logger = Logger ?? context.LoggerFactory.CreateLogger( GetType() );

        if ( data == null )
        {
            // no data...
            Logger.LogDebug( $"Received 'null' data." );

            return Task.CompletedTask;
        }

        Logger.LogDebug( $"Received '{data.GetType().Name}' data." );

        if ( data is T )
        {
            // execute type implementation
            return ExecuteAsync( context, (T)data );
        }

        var targetType = typeof( T );
        var sourceType = data.GetType();

        // T is not an enumerable but data is enumerable<T>
        // deliver enumerable<T>.T spreaded
        if ( !targetType.IsEnumerable() && sourceType.IsEnumerable<T>() )
        {
            var tasks = ( (IEnumerable)data ).OfType<T>()
                .Select( x => ExecuteAsync( context, x ) );

            return Task.WhenAll( tasks );
        }

        // T is an enumerable but data is enumerable.type
        // deliver data wrapped in an array
        if ( targetType.IsEnumerable() && sourceType.Equals( targetType.GetEnumerableElementType()! ) )
        {
            var array = Array.CreateInstance( targetType.GetEnumerableElementType()!, 1 );
            array.SetValue( data, 0 );

            return ExecuteAsync( context, (T)(object)array );
        }

        // attempt to convert the data
        if ( ConvertData( context, data, out var convertedData ) 
            && ( convertedData != null )
            && ( convertedData.GetType() != data.GetType() )
           )
        {
            return ExecuteAsync( context, convertedData );
        }

        OnDataNotSuitable( context, data );

        return Task.CompletedTask;
    }

    /// <summary>
    /// Converts received data to expected type
    /// </summary>
    protected virtual bool ConvertData( IChannelContext context, object data, out T? result )
    {
        var type = typeof( T );

        // attempt a byte[] to IByteBuffer transformation
        if ( type.IsAssignableFrom( typeof( IByteBuffer ) ) && ( data is byte[] ) )
        {
            Logger?.LogDebug( "Transformed from 'Byte[]' to 'IByteBuffer'." );

            result = (T)(IByteBuffer)new WrappedByteBuffer( (byte[])data, context.Channel.Buffer.Endianness );

            return ( true );
        }

        // attempt an IByteBuffer to byte[] transformation
        if ( ( type.IsArray && type.GetElementType() == typeof( byte ) ) 
            && data.GetType().IsAssignableTo( typeof( IByteBuffer ) ) )
        {
            Logger?.LogDebug( "Transformed from 'IByteBuffer' to 'Byte[]'." );

            result = (T)(object)((IByteBuffer)data).ToArray();

            return ( true );
        }

        result = default( T? );

        return ( false );
    }
}
