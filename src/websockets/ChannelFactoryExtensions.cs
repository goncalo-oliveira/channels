using System.Net.WebSockets;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
using Faactory.Channels.Handlers.WebSockets;
using Faactory.Channels.WebSockets.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Faactory.Channels.WebSockets;

/// <summary>
/// Extension methods for creating WebSocket channels from an <see cref="IChannelFactory"/> instance.
/// </summary>
public static class ChannelFactoryWebSocketExtensions
{
    /// <summary>
    /// Creates a WebSocket channel
    /// </summary>
    /// <param name="factory">The channel factory to create the channel from</param>
    /// <param name="webSocket">The WebSocket instance to use for the channel</param>
    /// <param name="channelName">The name of the channel (pipeline configuration) to use with the channel</param>
    /// <returns>>A WebSocket channel instance that gives access to the WebSocket and the channel pipelines for communication</returns>
    public static IWebSocketChannel CreateWebSocketChannel( this IChannelFactory factory, WebSocket webSocket, string channelName )
    {
        ArgumentException.ThrowIfNullOrEmpty( channelName, nameof( channelName ) );

        var scope = factory.ChannelServices.CreateScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ChannelOptions>>()
            .Get( channelName );

        var inputPipeline = ChannelPipeline.CreateInput( scope.ServiceProvider, channelName );
        var outputPipeline = CreateOutputPipeline( scope.ServiceProvider, channelName );
        var channelServices = scope.ServiceProvider.GetKeyedServices<IChannelService>( channelName );

        /*
        Create the channel instance
        */
        var channel = new WebSocketChannel(
            scope,
            webSocket,
            options,
            inputPipeline,
            outputPipeline,
            channelServices
        );

        return channel;
    }

    /// <summary>
    /// Creates the output channel pipeline for a WebSocket channel
    /// </summary>
    private static IChannelPipeline CreateOutputPipeline( IServiceProvider provider, string channelName )
    {
        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

        var adapters = provider.GetAdapters<IOutputChannelAdapter>( channelName )
            .Prepend( new WebSocketTextMessageAdapter( loggerFactory ) );

        IChannelPipeline pipeline = new ChannelPipeline(
            provider.GetRequiredService<ILoggerFactory>(),
            adapters,
            [
                new WebSocketChannelHandler( loggerFactory ),
                new OutputChannelHandler( loggerFactory )
            ]
        );

        return pipeline;
    }
}
