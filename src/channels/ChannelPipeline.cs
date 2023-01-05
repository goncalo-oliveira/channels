using Microsoft.Extensions.Logging;
using Faactory.Channels.Buffers;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;

namespace Faactory.Channels;

internal class ChannelPipeline : IChannelPipeline
{
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger logger;
    private readonly IEnumerable<IChannelAdapter> adapters;
    private readonly IEnumerable<IChannelHandler>? handlers;

    public ChannelPipeline( ILoggerFactory loggerFactory
        , IEnumerable<IChannelAdapter> channelAdapters
        , IEnumerable<IChannelHandler>? channelHandlers )
    {
        this.loggerFactory = loggerFactory;
        logger = loggerFactory.CreateLogger<ChannelPipeline>();
        adapters = channelAdapters;
        handlers = channelHandlers;
    }

    public void Dispose()
    {
        foreach ( var adapter in adapters )
        {
            if ( typeof( IDisposable ).IsAssignableFrom( adapter.GetType() ) )
            {
                ((IDisposable)adapter).Dispose();
            }
        }
    }

    public async Task ExecuteAsync( IChannel channel, object data )
    {
        var adapterContext = new AdapterContext( loggerFactory, channel );

        adapterContext.Forward( data );

        var interrupted = !await ExecuteAdaptersAsync( adapterContext )
            .ConfigureAwait( false );

        if ( interrupted )
        {
            // pipeline was interrupted
            return;
        }

        // execute handlers
        interrupted = !await ExecuteHandlersAsync( adapterContext )
            .ConfigureAwait( false );

        if ( interrupted )
        {
            // pipeline was interrupted
            // TODO: this could potentially be an option
            // by default (as is now) the pipeline is interrupted if a handler crashes
            // interrupting here means that in the output buffer doesn't get written
            // maybe that isn't always the case and the output buffer should be written nonetheless
            // same thing for adapters
            return;
        }

        // write output buffer data if any
        await ((WritableBuffer)adapterContext.Output).WriteAsync( channel )
            .ConfigureAwait( false );
    }

    private async Task<bool> ExecuteAdaptersAsync( AdapterContext context )
    {
        foreach ( var adapter in adapters )
        {
            var adapterData = context.Flush();

            if ( !adapterData.Any() )
            {
                // no data available on the pipeline
                // interrupt the workflow
                logger.LogWarning( $"No data forwarded from the previous adapter. Pipeline interrupted." );

                return ( false );
            }

            foreach ( var dataItem in adapterData )
            {
                try
                {
                    await adapter.ExecuteAsync( context, dataItem )
                        .ConfigureAwait( false );
                }
                catch ( Exception ex )
                {
                    logger.LogError( ex, $"Failed to execute '{adapter.GetType().Name}' adapter. Pipeline interrupted." );

                    return ( false );
                }
            }
        }

        return ( true );
    }

    private async Task<bool> ExecuteHandlersAsync( AdapterContext adapterContext )
    {
        if ( handlers == null )
        {
            // this is true for output pipelines
            return ( true );
        }

        var handlerData = adapterContext.Flush();

        if ( !handlerData.Any() )
        {
            // no data forwarded from the adapters
            logger.LogWarning( $"No data forwarded from the adapters." );
            return ( true );
        }

        if ( !handlers.Any() )
        {
            // no handlers
            logger.LogWarning( $"No data handlers were registered." );
            return ( true );
        }

        foreach ( var handler in handlers )
        {
            foreach ( var dataItem in handlerData )
            {
                try
                {
                    await handler.ExecuteAsync( adapterContext, dataItem )
                        .ConfigureAwait( false );
                }
                catch ( Exception ex )
                {
                    logger.LogError( ex, $"Failed to execute '{handler.GetType().Name}' handler. Pipeline interrupted." );

                    return ( false );
                }
            }
        }

        return ( true );
    }
}
