using Microsoft.Extensions.Logging;
using Faactory.Channels.Buffers;

namespace Faactory.Channels.Adapters;

/// <summary>
/// Base class for a channel adapter
/// </summary>
/// <typeparam name="T">The data type</typeparam>
public abstract class ChannelAdapter<T> : IChannelAdapter, IInputChannelAdapter, IOutputChannelAdapter
{
    private ILogger? logger;

    public ChannelAdapter()
    {}

    public ChannelAdapter( ILoggerFactory loggerFactory )
    {
        logger = loggerFactory.CreateLogger( GetType() );
    }

    public abstract Task ExecuteAsync( IAdapterContext context, T data );

    public Task ExecuteAsync( IAdapterContext context, object data )
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

        // attempt to transform the data
        var convertedData = TransformData( data );

        if ( ( convertedData != null ) && ( convertedData.GetType() != data.GetType() ) )
        {
            return ExecuteAsync( context, convertedData );
        }

        // forward data when not suitable for this adapter
        logger.LogDebug( $"Data type '{data.GetType().Name}' is not suitable for this adapter." );
        
        context.Forward( data );

        return Task.CompletedTask;
    }

    protected virtual object? TransformData( object data )
    {
        var type = typeof( T );

        // attempt a byte[] to IByteBuffer transformation
        if ( type.IsAssignableFrom( typeof( IByteBuffer ) ) && ( data is byte[] ) )
        {
            logger?.LogDebug( "Transformed 'Byte[]' to 'WrappedByteBuffer'." );

            return new WrappedByteBuffer( (byte[])data );
        }

        // attempt an IByteBuffer to byte[] transformation
        if ( ( type.IsArray && type.GetElementType() == typeof( byte ) ) 
            && data.GetType().IsAssignableTo( typeof( IByteBuffer ) ) )
        {
            logger?.LogDebug( "Transformed 'WrappedByteBuffer' to 'Byte[]'." );

            return ((IByteBuffer)data).ToArray();
        }

        return ( null );
    }
}
