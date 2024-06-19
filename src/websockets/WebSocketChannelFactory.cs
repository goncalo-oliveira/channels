using System.Net.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Faactory.Channels.WebSockets;

internal sealed class WebSocketChannelFactory( IServiceProvider serviceProvider ) : IWebSocketChannelFactory
{
    private readonly IServiceProvider provider = serviceProvider;

    public async Task<IWebSocketChannel> CreateChannelAsync( WebSocket webSocket, IChannelPipelineBuilder? pipelineBuilder )
    {
        var scope = provider.CreateScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptions<ChannelOptions>>();

        /*
        Create the pipelines. If no pipeline builder is provided, use the default one.
        */

        pipelineBuilder ??= CreateDefaultPipeline( provider );

        var inputPipeline = pipelineBuilder.BuildInputPipeline();
        var outputPipeline = pipelineBuilder.BuildOutputPipeline();

        /*
        Create the channel instance
        */
        var channel = new WebSocketChannel(
            scope,
            webSocket,
            options.Value.BufferEndianness,
            inputPipeline,
            outputPipeline,
            pipelineBuilder.ChannelServices
        );

        /*
        initialize the channel
        */
        await channel.InitializeAsync();

        return channel;
    }

    private static ChannelPipelineBuilder CreateDefaultPipeline( IServiceProvider provider )
    {
        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
        var pipelineBuilder = new ChannelPipelineBuilder( provider );

        // add the WebSocket-specific middleware (output adapters and handlers)
        pipelineBuilder.AddWebSocketsMiddleware();

        // add all registered input adapters
        pipelineBuilder.AddRegisteredInputAdapters();

        // add all registered input handlers
        pipelineBuilder.AddRegisteredInputHandlers();

        // add all registered output adapters
        pipelineBuilder.AddRegisteredOutputAdapters();

        // use all registered channel services
        pipelineBuilder.AddRegisteredChannelServices();

        return pipelineBuilder;
    }
}
