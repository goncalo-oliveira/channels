using System.Net;
using Faactory.Channels.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels.Udp;

internal sealed class ChannelsUdpClient : IChannelsClient, IDisposable
{
    private readonly ILogger logger;
    private readonly CancellationTokenSource cts = new();
    private readonly string channelName;
    private readonly ChannelsClientOptions options;
    private readonly Task receiveTask;

    private IPEndPoint remoteEndPoint;
    private System.Net.Sockets.UdpClient Socket { get; }
    private UdpChannel? udpChannel = null;

    public ChannelsUdpClient( IServiceScope serviceScope, ChannelsClientOptions clientOptions, string clientChannelName )
    {
        logger = serviceScope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<ChannelsUdpClient>();

        options = clientOptions;

        channelName = clientChannelName;
        Scope = serviceScope;

        if ( options.Host == "localhost" )
        {
            options.Host = "127.0.0.1";
        }

        remoteEndPoint = new IPEndPoint( IPAddress.Parse( options.Host ), options.Port );
        Socket = new System.Net.Sockets.UdpClient();
        Socket.Connect( remoteEndPoint );

        receiveTask = ReceiveAsync( cts.Token );
    }

    public IChannel Channel
    {
        get
        {
            udpChannel ??= CreateChannel( new UdpRemote( Socket, null ) );

            return udpChannel;
        }
    }

    private IServiceScope Scope { get; }
    private IServiceProvider ServiceProvider => Scope.ServiceProvider;

    public Task CloseAsync()
    {
        cts.Cancel();

        return Channel.CloseAsync();
    }

    public void Dispose()
    {
        cts.Cancel();

        Socket.Dispose();
    }

    private UdpChannel CreateChannel( UdpRemote remote )
    {
        var scope = ServiceProvider.CreateScope();

        var inputPipeline = ChannelPipeline.CreateInput( scope.ServiceProvider, channelName );        
        var outputPipeline = ChannelPipeline.CreateOutput( scope.ServiceProvider, channelName );
        var channelServices = scope.ServiceProvider.GetKeyedServices<IChannelService>( channelName );

        var channel = new UdpChannel(
              scope
            , remote
            , options.ChannelOptions
            , inputPipeline
            , outputPipeline
            , channelServices
        );

        return channel;
    }

    private async Task ReceiveAsync( CancellationToken cancellationToken )
    {
        while ( !cancellationToken.IsCancellationRequested )
        {
            try
            {
                var receiveResult = await Socket.ReceiveAsync( cancellationToken );

                logger.LogDebug( "Received {N} bytes from {EndPoint}.", receiveResult.Buffer.Length, receiveResult.RemoteEndPoint );

                udpChannel ??= CreateChannel( new UdpRemote( Socket, null ) );

                udpChannel.Receive( receiveResult.Buffer );
            }
            catch ( OperationCanceledException )
            { }
            catch ( Exception ex )
            {
                logger.LogError( "Failed to received data. {Error}.", ex.Message );
            }
        }
    }

    // public Task WriteAsync( object data )
    //     => Channel.WriteAsync( data );
}
