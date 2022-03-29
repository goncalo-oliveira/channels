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

        var interrupted = !await ExecuteAdaptersAsync( adapterContext );

        if ( interrupted )
        {
            // pipeline was interrupted
            return;
        }

        // execute handlers
        await ExecuteHandlersAsync( adapterContext );

        // write output buffer data if any
        await ((WritableBuffer)adapterContext.Output).WriteAsync( channel );
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
                await adapter.ExecuteAsync( context, dataItem );
            }
        }

        return ( true );
    }

    private async Task ExecuteHandlersAsync( AdapterContext adapterContext )
    {
        if ( handlers == null )
        {
            // this is true for output pipelines
            return;
        }

        var handlerData = adapterContext.Flush();

        if ( !handlerData.Any() )
        {
            // no data forwarded from the adapters
            logger.LogWarning( $"No data forwarded from the adapters. Pipeline interrupted." );
            return;
        }

        if ( !handlers.Any() )
        {
            // no handlers
            logger.LogWarning( $"No data handlers were registered." );
            return;
        }

        //var context = new ChannelContext( loggerFactory, adapterContext.Channel );

        foreach ( var handler in handlers )
        {
            foreach ( var dataItem in handlerData )
            {
                await handler.ExecuteAsync( adapterContext, dataItem );
            }
        }
    }
}
