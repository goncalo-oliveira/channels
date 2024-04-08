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

    /// <summary>
    /// Triggered when the received data is not suitable for the middleware
    /// </summary>
    protected abstract void OnDataNotSuitable( IChannelContext context, object data );

    public abstract Task ExecuteAsync( IChannelContext context, T data );

    public Task ExecuteAsync( IChannelContext context, object data )
    {
        if ( data == null )
        {
            // no data...
            return Task.CompletedTask;
        }

        if ( data is T t )
        {
            // execute type implementation
            return ExecuteAsync( context, t );
        }

        var targetType = typeof( T );
        var sourceType = data.GetType();

        // T is not an enumerable but data is enumerable<T>
        // deliver spreaded enumerable<T>.T as individual items
        if ( !targetType.IsEnumerable() && sourceType.IsEnumerable<T>() )
        {
            var aggregatedTask = ( (IEnumerable)data ).OfType<T>()
                .Aggregate(
                    Task.CompletedTask,
                    ( previousTask, item ) => previousTask.ContinueWith( async t => await ExecuteAsync( context, item ) ).Unwrap()
                );

            return aggregatedTask;
        }

        // T is an enumerable but data is enumerable.type
        // deliver data wrapped in an array
        if ( targetType.IsEnumerable() && sourceType.Equals( targetType.GetEnumerableElementType()! ) )
        {
            var array = Array.CreateInstance( targetType.GetEnumerableElementType()!, 1 );
            array.SetValue( data, 0 );

            return ExecuteAsync( context, (T)(object)array );
        }

        // attempt to convert the data type
        if ( ConvertType( context, data, out var convertedData ) 
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
    /// Converts received data type to the expected type
    /// </summary>
    protected virtual bool ConvertType( IChannelContext context, object data, out T? result )
    {
        var type = typeof( T );

        // attempt a byte[] to IByteBuffer transformation
        if ( type.IsAssignableFrom( typeof( IByteBuffer ) ) && ( data is byte[] ) )
        {
            result = (T)(IByteBuffer)new WrappedByteBuffer( (byte[])data, context.Channel.Buffer.Endianness );

            return ( true );
        }

        // attempt an IByteBuffer to byte[] transformation
        if (
            type.IsArray && type.GetElementType() == typeof( byte )  
            &&
            data.GetType().IsAssignableTo( typeof( IByteBuffer ) )
        )
        {
            var buffer = (IByteBuffer)data;
            result = (T)(object)buffer.ReadBytes( buffer.ReadableBytes );

            return ( true );
        }

        result = default;

        return  false ;
    }
}
