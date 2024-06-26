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

    public IWebSocketChannel CreateChannel( WebSocket webSocket, string name )
    {
        ArgumentException.ThrowIfNullOrEmpty( name, nameof( name ) );

        var scope = provider.CreateScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ChannelOptions>>()
            .Get( name );

        var inputPipeline = ChannelPipeline.CreateInput( scope.ServiceProvider, name );
        var outputPipeline = CreateOutputPipeline( scope.ServiceProvider, name );
        var channelServices = scope.ServiceProvider.GetKeyedServices<IChannelService>( name );

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
