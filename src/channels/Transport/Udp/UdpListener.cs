using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Faactory.Channels.Udp;

internal sealed class UdpListener : IHostedService, IDisposable
{
    private readonly ILogger logger;
    private readonly ConcurrentDictionary<UdpRemote, UdpChannel> channels = new();

    private Task? receiveTask;
    private CancellationTokenSource? cancellationTokenSource;

    public UdpListener( IServiceProvider serviceProvider, string channelName, UdpChannelListenerOptions options )
    {
        logger = serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<UdpListener>();

        Name = channelName;
        ServiceProvider = serviceProvider;
        Socket = new UdpClient( options.Port );
    }
    
    private string Name { get; }
    private IServiceProvider ServiceProvider { get; }
    private UdpClient Socket { get; }

    public void Dispose()
    {
        cancellationTokenSource?.Cancel();

        Socket.Dispose();
    }

    public Task StartAsync( CancellationToken cancellationToken )
    {
        logger.LogInformation( "Starting service..." );

        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken );

        receiveTask = ExecuteAsync( cancellationTokenSource.Token );

        return receiveTask.IsCompleted
            ? receiveTask
            : Task.CompletedTask;
    }

    public async Task StopAsync( CancellationToken cancellationToken )
    {
        if ( receiveTask is null )
        {
            return;
        }

        try
        {
            cancellationTokenSource?.Cancel();

            // close all channels
            var closeTasks = channels.Values.Select( x => x.CloseAsync() )
                .ToArray();

            await Task.WhenAll( closeTasks )
                .ConfigureAwait( false );
        }
        finally
        {
            var taskCompletion = new TaskCompletionSource<object>();

            using CancellationTokenRegistration tokenRegistration = cancellationToken.Register(
                x => ( (TaskCompletionSource<object>?)x )?.SetCanceled(),
                taskCompletion
            );
            await Task.WhenAny( receiveTask, taskCompletion.Task )
                .ConfigureAwait( false );
        }

        try
        {
            Socket.Close();
        }
        catch { }
    }

    private async Task ExecuteAsync( CancellationToken cancellationToken )
    {
        logger.LogInformation( "Started service.  [PID {Id}]", Environment.ProcessId );
        logger.LogInformation( "Listening at {EndPoint}", Socket.Client.LocalEndPoint );

        while ( !cancellationToken.IsCancellationRequested )
        {
            try
            {
                var receiveResult = await Socket.ReceiveAsync( cancellationToken );

                /*
                this gets executed in a separate thread to avoid thread issues with
                the channel's scope, since the channel has asynchronous operations
                */
                await Task.Run(
                    () => HandleReceivedData( receiveResult.Buffer, receiveResult.RemoteEndPoint ),
                    cancellationToken
                );
            }
            catch ( OperationCanceledException )
            {
                break;
            }
            catch ( Exception ex )
            {
                logger.LogError( "Failed to received data. {Error}.", ex.Message );
            }
        }
    }

    private void HandleReceivedData( byte[] data, IPEndPoint remoteEndPoint )
    {
        logger.LogDebug( "Received {N} bytes from {EndPoint}.", data.Length, remoteEndPoint );

        var channel = channels.GetOrAdd(
            new UdpRemote( Socket, remoteEndPoint ),
            CreateChannel
        );

        channel.Receive( data );
    }

    private UdpChannel CreateChannel( UdpRemote remote )
    {
        var scope = ServiceProvider.CreateScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ChannelOptions>>()
            .Get( Name );

        var inputPipeline = ChannelPipeline.CreateInput( scope.ServiceProvider, Name );        
        var outputPipeline = ChannelPipeline.CreateOutput( scope.ServiceProvider, Name );
        var channelServices = scope.ServiceProvider.GetKeyedServices<IChannelService>( Name );

        var channel = new UdpChannel(
              scope
            , remote
            , options
            , inputPipeline
            , outputPipeline
            , channelServices
        );

        channel.Closed += OnChannelClosed;

        return channel;
    }

    private void OnChannelClosed( UdpChannel channel )
    {
        channels.TryRemove( channel.Remote, out _ );

        channel.Closed -= OnChannelClosed;
    }
}
