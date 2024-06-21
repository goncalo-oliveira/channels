using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Faactory.Channels.Udp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Faactory.Channels;

public static class UdpListenerServiceExtensions
{
    public static IServiceCollection AddUdpListener( this IServiceCollection services )
    {
        services.AddHostedService<UdpListener>();
        //services.AddSingleton<IServiceChannelFactory, ServiceChannelFactory>();

        return services;
    }
}

internal sealed class UdpListener : IHostedService, IDisposable
{
    private readonly ILogger logger;
    private readonly ConcurrentDictionary<UdpRemote, UdpChannel> channels = new();

    private Task? receiveTask;
    private Task? monitorTask;
    private CancellationTokenSource? cancellationTokenSource;

    public UdpListener( ILoggerFactory loggerFactory, IServiceProvider serviceProvider )
    {
        logger = loggerFactory.CreateLogger<UdpListener>();

        ServiceProvider = serviceProvider;
        Socket = new UdpClient( 8080 );
    }
    
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
        monitorTask = MonitorAsync( cancellationTokenSource.Token );

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

        while ( !cancellationToken.IsCancellationRequested )
        {
            try
            {
                var receiveResult = await Socket.ReceiveAsync( cancellationToken );

                HandleReceivedData( receiveResult.Buffer, receiveResult.RemoteEndPoint );
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

    private async Task MonitorAsync( CancellationToken cancellationToken )
    {
        while ( !cancellationToken.IsCancellationRequested )
        {
            try
            {
                await Task.Delay( 1000, cancellationToken );

                foreach ( var channel in channels )
                {
                    if ( channel.Value.IsClosed )
                    {
                        channels.TryRemove( channel.Key, out _ );
                    }

                    // ts = most recent date (channel.LastReceived and channel.LastSent)
                    var ts = channel.Value.LastReceived > channel.Value.LastSent
                        ? channel.Value.LastReceived
                        : channel.Value.LastSent;

                    if ( ts?.AddSeconds( 30 ) < DateTimeOffset.UtcNow ) // TODO: should come from options
                    {
                        channels.TryRemove( channel.Key, out _ );

                        await channel.Value.CloseAsync();
                    }
                }
            }
            catch ( OperationCanceledException )
            { }
        }
    }

    private void HandleReceivedData( byte[] data, IPEndPoint remoteEndPoint )
    {
        var text = Encoding.UTF8.GetString( data ).TrimEnd( '\n', '\r', '\0' );

        logger.LogInformation( "Received from {EndPoint}: {Data}", remoteEndPoint, text );

        var channel = channels.GetOrAdd(
            new UdpRemote( Socket, remoteEndPoint ),
            CreateChannel
        );

        channel.Receive( data );
    }

    private UdpChannel CreateChannel( UdpRemote remote )
    {
        var name = ServiceChannelFactory.DefaultChannelName;
        var scope = ServiceProvider.CreateScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ServiceChannelOptions>>()
            .Get( name );

        var inputPipeline = CreateInputPipeline( scope.ServiceProvider, name );        
        var outputPipeline = CreateOutputPipeline( scope.ServiceProvider, name );
        var channelServices = scope.ServiceProvider.GetKeyedServices<IChannelService>( name );

        var channel = new UdpChannel(
              scope
            , remote
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
        var adapters = provider.GetAdapters<Adapters.IInputChannelAdapter>( name );
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

        var adapters = provider.GetAdapters<Adapters.IOutputChannelAdapter>( name );

        IChannelPipeline pipeline = new ChannelPipeline(
            provider.GetRequiredService<ILoggerFactory>(),
            adapters,
            [
                new Handlers.OutputChannelHandler( loggerFactory )
            ]
        );

        return pipeline;
    }
}
