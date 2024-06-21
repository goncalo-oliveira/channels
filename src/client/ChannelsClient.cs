using System.Net.Sockets;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels.Client;

internal sealed class ChannelsClient : IChannelsClient
{
    private readonly CancellationTokenSource cts = new();
    private readonly ChannelsClientOptions options;

    private Socket? socket = null;
    private Task connectTask = Task.CompletedTask;

    public ChannelsClient( IServiceScope serviceScope, ChannelsClientOptions clientOptions, string channelName )
    {
        options = clientOptions;

        Name = channelName;
        Scope = serviceScope;

        connectTask = ConnectAsync( cts.Token );
    }

    private IServiceScope Scope { get; }
    private IServiceProvider ServiceProvider => Scope.ServiceProvider;

    public IChannel Channel { get; private set; } = NullChannel.Instance;
    public string Name { get; }

    public Task CloseAsync()
    {
        cts.Cancel();

        return Channel.CloseAsync();
    }

    public void Dispose()
    {
        try
        {
            cts.Cancel();
        }
        catch { }

        try
        {
            if ( !connectTask.IsCompleted && !connectTask.IsCanceled && !connectTask.IsFaulted )
            {
                connectTask.Wait();
            }
        }
        catch { }

        try
        {
            socket?.Dispose();
        }
        catch { }
    }

    private async Task ConnectAsync( CancellationToken cancellationToken )
    {
        if ( !Channel.IsClosed )
        {
            return;
        }

        socket = CreateSocket( options.ChannelOptions.TransportMode );

        var reconnectDelay = options.ReconnectDelay;

        while ( Channel.IsClosed && !cancellationToken.IsCancellationRequested )
        {
            await socket.ConnectAsync( options.Host, options.Port, cancellationToken )
                .ConfigureAwait( false );

            if ( cancellationToken.IsCancellationRequested )
            {
                break;
            }

            if ( !socket.Connected )
            {
                await Task.Delay( reconnectDelay, cancellationToken );

                // apply exponential backoff
                reconnectDelay = TimeSpan.FromMilliseconds( Math.Min( options.MaxReconnectDelay.TotalMilliseconds, reconnectDelay.TotalMilliseconds * 2 ) );

                continue;
            }

            Channel = CreateChannel( socket );
        }

        /*
        unless the cancellation token has been requested, start monitoring the channel
        this will reconnect the channel if it is closed
        */
        if ( !cancellationToken.IsCancellationRequested )
        {
            connectTask = MonitorAsync( cancellationToken );
        }
    }

    private async Task MonitorAsync( CancellationToken cancellationToken )
    {
        while ( !Channel.IsClosed && !cancellationToken.IsCancellationRequested )
        {
            await Task.Delay( 1000, cancellationToken );
        }

        /*
        unless the cancellation token has been requested, reconnect the channel
        if the connection was lost
        */
        if ( !cancellationToken.IsCancellationRequested )
        {
            // ensure the channel is closed and resourced are released
            await Channel.CloseAsync()
                .ConfigureAwait( false );

            socket = null;

            await Task.Delay( options.ReconnectDelay, cancellationToken );

            if ( !cancellationToken.IsCancellationRequested )
            {
                connectTask = ConnectAsync( cancellationToken );
            }
        }
    }

    private ClientChannel CreateChannel( Socket socket )
    {
        var inputPipeline = CreateInputPipeline();
        var outputPipeline = CreateOutputPipeline();
        var channelServices = ServiceProvider.GetKeyedServices<IChannelService>( Name );

        return new ClientChannel(
            Scope,
            socket,
            options.ChannelOptions.BufferEndianness,
            inputPipeline,
            outputPipeline,
            channelServices
        );
    }

    // TODO: this code is duplicated in ClientChannelFactory, ServiceChannelFactory.cs and WebSocketChannelFactory.cs
    //       consider moving it to a common location
    private IChannelPipeline CreateInputPipeline()
    {
        var adapters = ServiceProvider.GetAdapters<IInputChannelAdapter>( Name );
        var handlers = ServiceProvider.GetHandlers( Name );

        IChannelPipeline pipeline = new ChannelPipeline(
            ServiceProvider.GetRequiredService<ILoggerFactory>(),
            adapters,
            handlers
        );

        return pipeline;
    }

    private IChannelPipeline CreateOutputPipeline()
    {
        var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

        var adapters = ServiceProvider.GetAdapters<IOutputChannelAdapter>( Name );

        IChannelPipeline pipeline = new ChannelPipeline(
            ServiceProvider.GetRequiredService<ILoggerFactory>(),
            adapters,
            [
                new OutputChannelHandler( loggerFactory )
            ]
        );

        return pipeline;
    }

    private static Socket CreateSocket( ChannelTransportMode transportMode )
    {
        SocketType socketType = transportMode switch
        {
            ChannelTransportMode.Tcp => SocketType.Stream,
            ChannelTransportMode.Udp => SocketType.Dgram,
            _ => throw new NotSupportedException( "Only TCP and UDP transport modes are supported." )
        };

        ProtocolType protocolType = transportMode switch
        {
            ChannelTransportMode.Tcp => ProtocolType.Tcp,
            ChannelTransportMode.Udp => ProtocolType.Udp,
            _ => throw new NotSupportedException( "Only TCP and UDP transport modes are supported." )
        };

        return new Socket( socketType, protocolType );
    }
}
