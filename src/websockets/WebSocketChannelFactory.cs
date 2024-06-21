using System.Net.WebSockets;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
using Faactory.Channels.Handlers.WebSockets;
using Faactory.Channels.WebSockets.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Faactory.Channels.WebSockets;

internal sealed class WebSocketChannelFactory( IServiceProvider serviceProvider ) : IWebSocketChannelFactory
{
    private readonly IServiceProvider provider = serviceProvider;

    internal const string DefaultChannelName = "__ws-default";

    public IWebSocketChannel CreateChannel( WebSocket webSocket, string? name )
    {
        name ??= DefaultChannelName;

        var scope = provider.CreateScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ChannelOptions>>()
            .Get( name );

        var inputPipeline = CreateInputPipeline( scope.ServiceProvider, name );        
        var outputPipeline = CreateOutputPipeline( scope.ServiceProvider, name );
        var channelServices = scope.ServiceProvider.GetKeyedServices<IChannelService>( name );

        /*
        Create the channel instance
        */
        var channel = new WebSocketChannel(
            scope,
            webSocket,
            options.BufferEndianness,
            inputPipeline,
            outputPipeline,
            channelServices
        );

        return channel;
    }

    private static IChannelPipeline CreateInputPipeline( IServiceProvider provider, string name )
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

    private static IChannelPipeline CreateOutputPipeline( IServiceProvider provider, string name )
    {
        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

        var adapters = provider.GetAdapters<IOutputChannelAdapter>( name )
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
