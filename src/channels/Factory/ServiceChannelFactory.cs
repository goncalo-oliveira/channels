using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Faactory.Channels;

internal class ServiceChannelFactory( IServiceProvider serviceProvider ) : IServiceChannelFactory
{
    private readonly IServiceProvider provider = serviceProvider;

    internal const string DefaultChannelName = "__default";

    public IChannel CreateChannel( System.Net.Sockets.Socket socket, string? name )
    {
        name ??= DefaultChannelName;

        var scope = provider.CreateScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ServiceChannelOptions>>()
            .Get( name );

        var inputPipeline = CreateInputPipeline( scope.ServiceProvider, name );        
        var outputPipeline = CreateOutputPipeline( scope.ServiceProvider, name );
        var channelServices = scope.ServiceProvider.GetKeyedServices<IChannelService>( name );

        var channel = new ServiceChannel(
              scope
            , socket
            , options.BufferEndianness
            , inputPipeline
            , outputPipeline
            , channelServices
        );

        return channel;
    }

    // TODO: this code is duplicated in ClientChannelFactory.cs and WebSocketChannelFactory.cs
    //       consider moving it to a common location
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
