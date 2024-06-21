using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Faactory.Channels.Buffers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Faactory.Channels.Udp;

namespace Faactory.Channels;

internal sealed class UdpChannel : Channel, IChannel
{
    private readonly Task initializeTask;
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
    }

    public UdpRemote Remote { get; init; }

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
    }

    protected void OnDataReceived( byte[] data )
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

    protected void OnDataSent( int bytesSent )
    {
        LastSent = DateTimeOffset.UtcNow;

        this.NotifyDataSent( bytesSent );
    }

    protected async void OnDisconnected()
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

        Dispose();
    }
}
