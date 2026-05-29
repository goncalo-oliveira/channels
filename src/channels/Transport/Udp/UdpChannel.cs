using Microsoft.Extensions.Logging;
using Faactory.Channels.Buffers;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;

namespace Faactory.Channels.Udp;

internal sealed class UdpChannel : Channel
{
    private readonly SemaphoreSlim pipelineLock = new( 1, 1 );
    private readonly ILogger logger;

    internal UdpChannel( 
          IServiceScope serviceScope
        , UdpRemote udpRemote
        , string channelName
        , ChannelOptions options
        , IChannelPipeline inputPipeline
        , IChannelPipeline outputPipeline
        , IEnumerable<IChannelService> channelServices )
        : base( serviceScope )
    {
        logger = serviceScope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<UdpChannel>();

        Remote = udpRemote;
        Name = channelName;
        Buffer = new WritableByteBuffer( options.BufferEndianness );
        Timeout = options.IdleTimeout;
        Input = inputPipeline;
        Output = outputPipeline;

        if ( channelServices != null )
        {
            ChannelServices = channelServices;
        }

        ScopeLogger(
            logger => logger.LogDebug( "Created" )
        );

        BeginInitialize();
    }

    internal UdpRemote Remote { get; init; }

    internal event Action<UdpChannel>? Closed;


    public override async Task CloseAsync()
    {
        await base.CloseAsync().ConfigureAwait( false );

        try
        {
            Closed?.Invoke( this );
        }
        catch ( Exception ex )
        {
            ScopeLogger(
                logger => logger.LogError( ex, "Closed handler failed. {Message}", ex.Message )
            );
        }
    }

    public override async Task WriteRawBytesAsync( byte[] data )
    {
        try
        {
            await Remote.SendAsync( data )
                .ConfigureAwait( false );

            // trigger data sent
            ScopeLogger(
                logger => logger.LogTrace( "sent {bytesSent} bytes", data.Length )
            );

            NotifyDataSent( data );
        }
        catch ( Exception ex ) when ( ex is ObjectDisposedException || ex is SocketException )
        {
            // socket is closed
            ScopeLogger(
                logger => logger.LogTrace( "Disconnected." )
            );

            _ = CloseAsync();
        }
    }

    internal async Task ExecuteInputPipelineAsync( byte[] data )
    {
        try
        {
            await pipelineLock.WaitAsync( LifetimeToken )
                .ConfigureAwait( false );
        }
        catch ( OperationCanceledException )
        {
            return;
        }

        try
        {
            NotifyDataReceived( data );

            Buffer.WriteBytes( data, 0, data.Length );

            var pipelineBuffer = Buffer.AsReadableView();

            ScopeLogger(
                logger => logger.LogDebug( "Executing input pipeline..." )
            );

            await Input.ExecuteAsync( this, pipelineBuffer, LifetimeToken )
                .ConfigureAwait( false );

            Buffer.Rebase( pipelineBuffer.Offset );
            Buffer.Compact();

            if ( Buffer.Length > 0 )
            {
                ScopeLogger(
                    logger => logger.LogDebug( "Remaining buffer length: {Length} byte(s).", Buffer.Length )
                );
            }
        }
        catch ( OperationCanceledException )
        {
            // Expected shutdown
        }
        catch ( Exception ex )
        {
            ScopeLogger(
                logger => logger.LogError( ex, "Pipeline execution failed. {Message}", ex.Message )
            );

            await CloseAsync().ConfigureAwait( false );
        }
        finally
        {
            pipelineLock.Release();
        }
    }
}
