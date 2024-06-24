using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Faactory.Channels.Tcp;

internal sealed class TcpListener : IHostedService, IDisposable
{
    private readonly ILogger logger;
    private readonly int so_backlog;
    private Task? receiveTask;
    private CancellationTokenSource? cancellationTokenSource;

    public TcpListener( IServiceProvider serviceProvider, string channelName, TcpChannelListenerOptions options )
    {
        logger = serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<TcpListener>();

        Name = channelName;
        ServiceProvider = serviceProvider;

        so_backlog = options.Backlog;

        var localEndPoint = new IPEndPoint( IPAddress.Any, options.Port );

        Socket = new Socket( localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp );

        Socket.Bind( localEndPoint );
    }

    private string Name { get; }
    private IServiceProvider ServiceProvider { get; }
    private Socket Socket { get; }

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
            // var closeTasks = channels.Values.Select( x => x.CloseAsync() )
            //     .ToArray();

            // await Task.WhenAll( closeTasks )
            //     .ConfigureAwait( false );
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

        Socket.Listen( so_backlog );

        while ( !cancellationToken.IsCancellationRequested )
        {
            try
            {
                var clientSocket = await Socket.AcceptAsync( cancellationToken );

                if ( Socket.SafeHandle.IsClosed )
                {
                    continue;
                }

                _ = CreateChannel( clientSocket );
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

    private TcpChannel CreateChannel( Socket channelSocket )
    {
        var scope = ServiceProvider.CreateScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ChannelOptions>>()
            .Get( Name );

        var inputPipeline = ChannelPipeline.CreateInput( scope.ServiceProvider, Name );        
        var outputPipeline = ChannelPipeline.CreateOutput( scope.ServiceProvider, Name );
        var channelServices = scope.ServiceProvider.GetKeyedServices<IChannelService>( Name );

        var channel = new TcpChannel(
              scope
            , channelSocket
            , options.BufferEndianness
            , inputPipeline
            , outputPipeline
            , channelServices
        );

        return channel;
    }
}
