using Microsoft.Extensions.Logging;
using Faactory.Channels.Buffers;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels.Udp;

internal sealed class UdpChannel : Channel, IChannel
{
    private readonly Task initializeTask;
    private readonly Task monitorTask;
    private readonly CancellationTokenSource cts = new();
    private readonly ILogger logger;
    private readonly IDisposable? loggerScope;

    internal UdpChannel( 
          IServiceScope serviceScope
        , UdpRemote udpRemote
        , Endianness bufferEndianness
        , IChannelPipeline inputPipeline
        , IChannelPipeline outputPipeline
        , IEnumerable<IChannelService> channelServices )
        : base( serviceScope )
    {
        logger = serviceScope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<IChannel>();

        loggerScope = logger.BeginScope( $"udp-{Id[..7]}" );

        Remote = udpRemote;
        Buffer = new WritableByteBuffer( bufferEndianness );
        Input = inputPipeline;
        Output = outputPipeline;

        if ( channelServices != null )
        {
            Services = channelServices;
        }

        initializeTask = InitializeAsync( cts.Token );
        monitorTask = MonitorAsync( cts.Token );
    }

    internal UdpRemote Remote { get; init; }

    internal event Action<UdpChannel>? Closed;

    public override Task CloseAsync()
    {
        try
        {
            cts.Cancel();
        }
        catch { }

        OnDisconnected();

        return Task.CompletedTask;
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
        Input.Dispose();
        Output.Dispose();

        logger.LogDebug( "Disposed." );

        loggerScope?.Dispose();

        base.Dispose();
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
        catch ( ObjectDisposedException )
        {
            // socket is closed
            logger.LogTrace( "Disconnected." );

            OnDisconnected();
        }
    }

    internal void Receive( byte[] data )
    {
        LastReceived = DateTimeOffset.UtcNow;

        this.NotifyDataReceived( data );

        Buffer.WriteBytes( data, 0, data.Length );

        var pipelineBuffer = Buffer.MakeReadOnly();

        logger.LogDebug( "Executing input pipeline..." );

        Task.Run( () => Input.ExecuteAsync( this, pipelineBuffer ) )
            .ConfigureAwait( false )
            .GetAwaiter()
            .GetResult();

        pipelineBuffer.DiscardReadBytes();

        Buffer = pipelineBuffer.MakeWritable();

        if ( Buffer.Length > 0 )
        {
            logger.LogDebug( "Remaining buffer length: {Length} byte(s).", Buffer.Length );
        }
    }

    private async void OnDisconnected()
    {
        if ( IsClosed )
        {
            return;
        }

        IsClosed = true;

        logger.LogInformation( "Closed." );
        
        try
        {
            this.NotifyChannelClosed();
        }
        catch ( Exception )
        { }

        await StopServicesAsync()
            .ConfigureAwait( false );

        Closed?.Invoke( this );

        Dispose();
    }

    private async Task MonitorAsync( CancellationToken cancellationToken )
    {
        LastReceived = DateTimeOffset.UtcNow;
        LastSent = DateTimeOffset.UtcNow;

        while ( !cancellationToken.IsCancellationRequested )
        {
            try
            {
                await Task.Delay( 1000, cancellationToken );

                var ts = LastReceived > LastSent ? LastReceived : LastSent;

                if ( ts?.AddSeconds( 30 ) < DateTimeOffset.UtcNow ) // TODO: should come from options
                {
                    await CloseAsync();

                    break;
                }
            }
            catch ( OperationCanceledException )
            {
                break;
            }
        }
    }
}
