using System.Net.Sockets;
using Faactory.Channels.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels.Tcp;

internal sealed class TcpClient : IChannelsClient
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

    private IChannel channel = NullChannel.Instance;
    public IChannel Channel => channel;

    private IServiceScope Scope { get; }
    private IServiceProvider ServiceProvider => Scope.ServiceProvider;

    public async Task CloseAsync()
    {
        cts.Cancel();

        await Channel.CloseAsync();

        try
        {
            socket?.Dispose();
        }
        catch { }

        try
        {
            await connectTask;
        }
        catch { }
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync().ConfigureAwait( false );
    }

    private async Task ConnectAsync( CancellationToken cancellationToken )
    {
        if ( !Channel.IsClosed )
        {
            return;
        }

        var reconnectDelay = options.ReconnectDelay;

        while ( Channel.IsClosed && !cancellationToken.IsCancellationRequested )
        {
            socket = new Socket( SocketType.Stream, ProtocolType.Tcp );

            try
            {
                await socket.ConnectAsync( options.Host, options.Port, cancellationToken )
                    .ConfigureAwait( false );

                if ( !socket.Connected )
                {
                    throw new SocketException( (int) SocketError.NotConnected );
                }

                var ch = CreateChannel( socket );

                var previousChannel = Interlocked.Exchange( ref channel, ch );

                if ( previousChannel != null )
                {
                    await previousChannel.DisposeAsync()
                        .ConfigureAwait( false );
                }
            }
            catch ( OperationCanceledException )
            {
                break;
            }
            catch
            {
                await Task.Delay( reconnectDelay, cancellationToken );

                // apply exponential backoff
                reconnectDelay = TimeSpan.FromMilliseconds( Math.Min( options.MaxReconnectDelay.TotalMilliseconds, reconnectDelay.TotalMilliseconds * 2 ) );

                continue;
            }
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
