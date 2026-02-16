using Microsoft.Extensions.Logging;
using Faactory.Channels.Buffers;
using Faactory.Channels.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;

namespace Faactory.Channels.Udp;

internal sealed class UdpChannel : Channel, IChannel, IAsyncDisposable
{
    private readonly Task initializeTask;
    private readonly CancellationTokenSource cts = new();
    private readonly SemaphoreSlim pipelineLock = new( 1, 1 );
    private readonly ILogger logger;
    private readonly IDisposable? loggerScope;

    internal UdpChannel( 
          IServiceScope serviceScope
        , UdpRemote udpRemote
        , ChannelOptions options
        , IChannelPipeline inputPipeline
        , IChannelPipeline outputPipeline
        , IEnumerable<IChannelService> channelServices )
        : base( serviceScope )
    {
        logger = serviceScope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<UdpChannel>();

        loggerScope = logger.BeginScope( $"udp-{Id[..7]}" );

        Remote = udpRemote;
        Buffer = new WritableByteBuffer( options.BufferEndianness );
        Timeout = options.IdleTimeout;
        Input = inputPipeline;
        Output = outputPipeline;

        if ( channelServices != null )
        {
            Services = channelServices;
        }

        initializeTask = InitializeAsync( cts.Token );

        _ = initializeTask.ContinueWith(
            _ => logger.LogInformation( "Ready." ),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );
            
        _ = initializeTask.ContinueWith(
            t => logger.LogError( t.Exception?.GetBaseException(), "Init failed" ),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );
    }

    internal UdpRemote Remote { get; init; }

    internal event Action<UdpChannel>? Closed;

    private int isClosing;

    public override async Task CloseAsync()
    {
        if ( Interlocked.Exchange( ref isClosing, 1 ) == 1 )
        {
            return;
        }

        IsClosed = true;

        try
        {
            cts.Cancel();
        }
        catch { }

        logger.LogInformation( "Closed." );

        try
        {
            this.NotifyChannelClosed();
        }
        catch ( Exception )
        { }

        logger.LogDebug( "Stopping services." );

        try
        {
            await StopServicesAsync()
                .ConfigureAwait( false );
        }
        catch ( Exception ex )
        {
            logger.LogError( ex, "Failed to stop services. {Message}", ex.Message );
        }

        logger.LogDebug( "Stopped services." );

        try
        {
            Closed?.Invoke( this );
        }
        catch ( Exception ex )
        {
            logger.LogError( ex, "Closed handler failed. {Message}", ex.Message );
        }

        initializeTask.TryDispose();

        loggerScope?.Dispose();

        base.Dispose();
    }

    public override Task WriteAsync( object data )
    {
        if ( IsClosed )
        {
            logger.LogWarning( "Can't write to a closed channel." );

            return Task.CompletedTask;
        }

        return base.WriteAsync( data );
    }

    public override void Dispose()
    {
        _ = CloseAsync();

        GC.SuppressFinalize( this );
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync().ConfigureAwait( false );

        GC.SuppressFinalize( this );
    }

    public override async Task WriteRawBytesAsync( byte[] data )
    {
        try
        {
            await Remote.SendAsync( data )
                .ConfigureAwait( false );

            // trigger data sent
            logger.LogTrace( "sent {bytesSent} bytes", data.Length );

            LastSent = DateTimeOffset.UtcNow;

            this.NotifyDataSent( data.Length );
        }
        catch ( Exception ex ) when ( ex is ObjectDisposedException || ex is SocketException )
        {
            // socket is closed
            logger.LogTrace( "Disconnected." );

            _ = CloseAsync();
        }
    }

    internal async Task ExecuteInputPipelineAsync( byte[] data )
    {
        try
        {
            await pipelineLock.WaitAsync( cts.Token )
                .ConfigureAwait( false );
        }
        catch ( OperationCanceledException )
        {
            return;
        }

        try
        {
            LastReceived = DateTimeOffset.UtcNow;

            this.NotifyDataReceived( data, data.Length );

            Buffer.WriteBytes( data, 0, data.Length );

            var pipelineBuffer = Buffer.MakeReadOnly();

            logger.LogDebug( "Executing input pipeline..." );

            await Input.ExecuteAsync( this, pipelineBuffer )
                .ConfigureAwait( false );

            pipelineBuffer.DiscardReadBytes();

            Buffer = pipelineBuffer.MakeWritable();

            if ( Buffer.Length > 0 )
            {
                logger.LogDebug( "Remaining buffer length: {Length} byte(s).", Buffer.Length );
            }
        }
        catch ( Exception ex )
        {
            logger.LogError( ex, "Pipeline execution failed. {Message}", ex.Message );

            if ( !IsClosed )
            {
                await CloseAsync().ConfigureAwait( false );
            }
        }
        finally
        {
            pipelineLock.Release();
        }
    }
}
