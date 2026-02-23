using System.Net;
using Faactory.Channels.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels.Udp;

internal sealed class ChannelsUdpClient : IChannelsClient
{
    private readonly ILogger logger;
    private readonly CancellationTokenSource cts = new();
    private readonly string channelName;
    private readonly ChannelsClientOptions options;
    private readonly Task receiveTask;

    private readonly IPEndPoint remoteEndPoint;
    private System.Net.Sockets.UdpClient Socket { get; }
    private readonly UdpChannel udpChannel;

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

        udpChannel = CreateUdpChannel();

        receiveTask = ReceiveAsync( cts.Token );

        _ = receiveTask.ContinueWith(
            t => logger.LogError( t.Exception?.GetBaseException(), "Receive loop failed" ),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );
    }

    public IChannel Channel => udpChannel;

    private IServiceScope Scope { get; }
    private IServiceProvider ServiceProvider => Scope.ServiceProvider;

    public async Task CloseAsync()
    {
        cts.Cancel();

        try
        {
            await receiveTask.ConfigureAwait( false );
        }
        catch (OperationCanceledException)
        { }

        await Channel.CloseAsync()
            .ConfigureAwait( false );

        try
        {
            Socket.Dispose();
        }
        catch { }
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync()
            .ConfigureAwait( false );
    }

    private UdpChannel CreateUdpChannel()
    {
        var scope = ServiceProvider.CreateScope();

        var inputPipeline = ChannelPipeline.CreateInput( scope.ServiceProvider, channelName );        
        var outputPipeline = ChannelPipeline.CreateOutput( scope.ServiceProvider, channelName );
        var channelServices = scope.ServiceProvider.GetKeyedServices<IChannelService>( channelName );

        var channel = new UdpChannel(
              scope
            , new UdpRemote( Socket, null )
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

                var buffer = receiveResult.Buffer.ToArray();

                await udpChannel.ExecuteInputPipelineAsync( buffer );
            }
            catch ( OperationCanceledException )
            {
                break;
            }
            catch ( Exception ex )
            {
                logger.LogError( "Failed to received data. {Error}.", ex.Message );
                break;
            }
        }
    }
}
