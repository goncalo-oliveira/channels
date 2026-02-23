using System.Collections.Concurrent;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Faactory.Channels.Udp;

internal sealed class UdpListener( IServiceProvider serviceProvider, string channelName, UdpChannelListenerOptions options ) : IHostedService, IDisposable, IAsyncDisposable
{
    private readonly ILogger logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<UdpListener>();
    private readonly ConcurrentDictionary<UdpRemote, UdpChannel> channels = new();

    private Task? receiveTask;
    private CancellationTokenSource? cancellationTokenSource;

    private string Name { get; } = channelName;
    private IServiceProvider ServiceProvider { get; } = serviceProvider;
    private UdpClient Socket { get; } = new UdpClient(options.Port);

    public void Dispose()
    {
        _ = StopAsync( CancellationToken.None );
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync( CancellationToken.None )
            .ConfigureAwait( false );
    }

    public Task StartAsync( CancellationToken cancellationToken )
    {
        logger.LogInformation( "Starting service..." );

        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken );

        receiveTask = ExecuteAsync( cancellationTokenSource.Token );
        _ = receiveTask.ContinueWith(
            t => logger.LogError( t.Exception?.GetBaseException(), "Receive loop failed" ),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );        

        return Task.CompletedTask;
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

                logger.LogDebug( "Received {N} bytes from {EndPoint}.", receiveResult.Buffer.Length, receiveResult.RemoteEndPoint );

                var channel = channels.GetOrAdd(
                    new UdpRemote( Socket, receiveResult.RemoteEndPoint ),
                    CreateChannel
                );

                _ = channel.ExecuteInputPipelineAsync( receiveResult.Buffer );
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
