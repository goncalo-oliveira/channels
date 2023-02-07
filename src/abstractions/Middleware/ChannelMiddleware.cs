using System.Collections;
using Faactory.Channels.Buffers;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels;

/// <summary>
/// Base class for channel middleware. Inherited by handler and adapter base classes.
/// </summary>
/// <typeparam name="T">The data type</typeparam>
public abstract class ChannelMiddleware<T>
{
    internal ChannelMiddleware()
    { }

    protected abstract void OnDataNotSuitable( IChannelContext context, object data );

    public abstract Task ExecuteAsync( IChannelContext context, T data );

    public Task ExecuteAsync( IChannelContext context, object data )
    {
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
            result = (T)(IByteBuffer)new WrappedByteBuffer( (byte[])data, context.Channel.Buffer.Endianness );

            return ( true );
        }

        // attempt an IByteBuffer to byte[] transformation
        if ( ( type.IsArray && type.GetElementType() == typeof( byte ) ) 
            && data.GetType().IsAssignableTo( typeof( IByteBuffer ) ) )
        {
            var buffer = (IByteBuffer)data;
            result = (T)(object)buffer.ReadBytes( buffer.ReadableBytes );

            return ( true );
        }

        result = default( T? );

        return ( false );
    }
}
