using Microsoft.Extensions.Logging;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal class ChannelPipeline( ILoggerFactory loggerFactory, IEnumerable<IChannelAdapter> channelAdapters, IEnumerable<IChannelHandler>? channelHandlers = null ) : IChannelPipeline
{
    private readonly ILogger logger = loggerFactory.CreateLogger<ChannelPipeline>();
    private readonly IEnumerable<IChannelAdapter> adapters = channelAdapters;
    private readonly IEnumerable<IChannelHandler> handlers = channelHandlers ?? [];

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

    public async Task ExecuteAsync( IChannel channel, object data, CancellationToken cancellationToken )
    {
        var adapterContext = new AdapterContext( channel );

        adapterContext.Forward( data );

        var interrupted = !await ExecuteAdaptersAsync( adapterContext, cancellationToken )
            .ConfigureAwait( false );

        if ( interrupted )
        {
            // pipeline was interrupted
            return;
        }

        // execute handlers
        interrupted = !await ExecuteHandlersAsync( adapterContext, cancellationToken )
            .ConfigureAwait( false );

        if ( interrupted )
        {
            // pipeline was interrupted
            return;
        }

        // write output buffer data if any
        await ((WritableBuffer)adapterContext.Output).WriteAsync( channel )
            .ConfigureAwait( false );
    }

    private async Task<bool> ExecuteAdaptersAsync( AdapterContext context, CancellationToken cancellationToken )
    {
        IChannelAdapter? previousAdapter = null;
        foreach ( var adapter in adapters )
        {
            var adapterData = context.Flush();

            if ( adapterData.Length == 0 )
            {
                // no data available on the pipeline
                // interrupt the workflow

                logger.LogDebug( "No data forwarded from {Type} adapter. Pipeline interrupted.",
                    previousAdapter?.GetType().Name ?? "previous"
                );

                return false;
            }

            foreach ( var dataItem in adapterData )
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await adapter.ExecuteAsync( context, dataItem, cancellationToken )
                        .ConfigureAwait( false );
                }
                catch ( OperationCanceledException )
                {
                    // Expected shutdown
                    throw;
                }
                catch ( Exception ex )
                {
                    logger.LogError(
                        ex,
                        "Failed to execute '{TypeName}' adapter. Pipeline interrupted.",
                        adapter.GetType().Name
                    );

                    /*
                    If the middleware throws an exception, the channel is likely in an inconsistent state and should be closed.
                    */
                    _ = context.Channel.CloseAsync();

                    Metrics.MiddlewareExceptions.Add( 1 );

                    return false;
                }
            }

            previousAdapter = adapter;
        }

        return true;
    }

    private async Task<bool> ExecuteHandlersAsync( AdapterContext context, CancellationToken cancellationToken )
    {
        var handlerData = context.Flush();

        if ( handlerData.Length == 0)
        {
            // no data forwarded from the adapters
            logger.LogDebug( "No data forwarded from the adapters." );
            return true;
        }

        if ( !handlers.Any() )
        {
            // no handlers
            logger.LogWarning( "No data handlers were registered." );
            return true;
        }

        foreach ( var handler in handlers )
        {
            foreach ( var dataItem in handlerData )
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await handler.ExecuteAsync( context, dataItem, cancellationToken )
                        .ConfigureAwait( false );
                }
                catch ( OperationCanceledException )
                {
                    // Expected shutdown
                    throw;
                }
                catch ( ObjectDisposedException )
                {
                    // socket was disposed
                    logger.LogError( "Channel is closed." );

                    return false;
                }
                catch ( Exception ex )
                {
                    logger.LogError(
                        ex,
                        "Failed to execute '{TypeName}' handler. Pipeline interrupted.",
                        handler.GetType().Name
                    );

                    /*
                    If the middleware throws an exception, the channel is likely in an inconsistent state and should be closed.
                    */
                    _ = context.Channel.CloseAsync();

                    Metrics.MiddlewareExceptions.Add( 1 );

                    return false;
                }
            }
        }

        return true;
    }

    internal static IChannelPipeline CreateInput( IServiceProvider provider, string name )
    {
        var adapters = provider.GetAdapters<IInputChannelAdapter>( name );
        var handlers = provider.GetHandlers( name );

        IChannelPipeline pipeline = new ChannelPipeline(
            provider.GetRequiredService<ILoggerFactory>(),
            adapters,
            handlers
        );

        return pipeline;
    }

    internal static IChannelPipeline CreateOutput( IServiceProvider provider, string name )
    {
        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

        var adapters = provider.GetAdapters<IOutputChannelAdapter>( name );

        IChannelPipeline pipeline = new ChannelPipeline(
            provider.GetRequiredService<ILoggerFactory>(),
            adapters,
            [
                new OutputChannelHandler( loggerFactory )
            ]
        );

        return pipeline;
    }
}
