using System.Collections;
using Faactory.Channels.Buffers;

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

    /// <summary>
    /// Executes the middleware logic for the received data
    /// </summary>
    /// <param name="context">The channel context</param>
    /// <param name="data">The received data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public abstract Task ExecuteAsync( IChannelContext context, T data, CancellationToken cancellationToken );

    /// <summary>
    /// Executes the middleware logic for the received data, with type conversion and enumerable support
    /// </summary>
    /// <param name="context">The channel context</param>
    /// <param name="data">The received data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public Task ExecuteAsync( IChannelContext context, object data, CancellationToken cancellationToken )
    {
        if ( data == null )
        {
            // no data...
            return Task.CompletedTask;
        }

        if ( data is T t )
        {
            // execute type implementation
            return ExecuteAsync( context, t, cancellationToken );
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
                    ( previousTask, item ) => previousTask.ContinueWith( async t => await ExecuteAsync( context, item, cancellationToken ) ).Unwrap()
                );

            return aggregatedTask;
        }

        // T is an enumerable but data is enumerable.type
        // deliver data wrapped in an array
        if ( targetType.IsEnumerable() && sourceType.Equals( targetType.GetEnumerableElementType()! ) )
        {
            var array = Array.CreateInstance( targetType.GetEnumerableElementType()!, 1 );
            array.SetValue( data, 0 );

            return ExecuteAsync( context, (T)(object)array, cancellationToken );
        }

        // attempt to convert the data type
        if ( ConvertType( context, data, out var convertedData ) 
            && ( convertedData != null )
            && ( convertedData.GetType() != data.GetType() )
           )
        {
            return ExecuteAsync( context, convertedData, cancellationToken );
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

        // byte[] -> IReadableByteBuffer conversion
        // byte[] -> IByteBuffer conversion
        if ( data is byte[] bytes && ( typeof( IReadableByteBuffer ).IsAssignableFrom( type ) || typeof( IByteBuffer ).IsAssignableFrom( type ) ) )
        {
            result = (T)(object)new ReadableByteBuffer( bytes, context.BufferEndianness );

            return true;
        }

        // IByteBuffer -> T
        if ( data is IByteBuffer buffer )
        {
            // IByteBuffer -> byte[] conversion
            if ( type.IsArray && type.GetElementType() == typeof( byte ) )
            {
                result = (T)(object)buffer.ToArray();

                /*
                because a byte[] doesn't offer structured access to the data, at this point the buffer is discarded and considered consumed.
                we could use DiscardAll, but that does an extra allocation. Skipping the bytes just moves the offset and avoids the allocation.
                */
                if ( data is IReadableByteBuffer readableBuffer )
                {
                    readableBuffer.SkipBytes( readableBuffer.ReadableBytes );
                }

                return true;
            }

            // IByteBuffer pass-through conversion
            if ( typeof( IByteBuffer ).IsAssignableFrom( type ) )
            {
                result = (T)buffer;

                return true;
            }
        }

        result = default;

        return  false ;
    }
}
