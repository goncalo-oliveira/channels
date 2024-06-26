using System.Net.Sockets;
using Faactory.Channels.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels.Tcp;

internal sealed class TcpClient : IChannelsClient, IDisposable
{
    private readonly CancellationTokenSource cts = new();
    private readonly string channelName;
    private readonly ChannelsClientOptions options;

    private Socket? socket = null;
    private Task connectTask = Task.CompletedTask;

    public TcpClient( IServiceScope serviceScope, ChannelsClientOptions clientOptions, string clientChannelName )
    {
        options = clientOptions;

        channelName = clientChannelName;
        Scope = serviceScope;

        connectTask = ConnectAsync( cts.Token );
    }

    public IChannel Channel { get; private set; } = NullChannel.Instance;

    private IServiceScope Scope { get; }
    private IServiceProvider ServiceProvider => Scope.ServiceProvider;

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

    // public Task WriteAsync( object data )
    //     => Channel.WriteAsync( data );

    private async Task ConnectAsync( CancellationToken cancellationToken )
    {
        if ( !Channel.IsClosed )
        {
            return;
        }

        socket = new Socket( SocketType.Stream, ProtocolType.Tcp );

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

    private TcpChannel CreateChannel( Socket socket )
    {
        var inputPipeline = ChannelPipeline.CreateInput( ServiceProvider, channelName );
        var outputPipeline = ChannelPipeline.CreateOutput( ServiceProvider, channelName );
        var channelServices = ServiceProvider.GetKeyedServices<IChannelService>( channelName );

        return new TcpChannel(
            Scope,
            socket,
            options.ChannelOptions,
            inputPipeline,
            outputPipeline,
            channelServices
        );
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
}
