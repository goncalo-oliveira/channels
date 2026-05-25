using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Faactory.Channels.Buffers;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal sealed class TcpChannel : Channel
{
    private const int DefaultBufferLength = 13312;

    private readonly ILogger logger;
    private Task receiveTask = Task.CompletedTask;

    internal TcpChannel( 
          IServiceScope serviceScope
        , Socket socket
        , string channelName
        , ChannelOptions options
        , IChannelPipeline inputPipeline
        , IChannelPipeline outputPipeline
        , IEnumerable<IChannelService>? channelServices = null
        )
        : base( serviceScope )
    {
        logger = serviceScope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<TcpChannel>();

        Buffer = new WritableByteBuffer( options.BufferEndianness );
        Timeout = options.IdleTimeout;
        Socket = socket;
        Name = channelName;
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

    public Socket Socket { get; init; }

    public override async Task CloseAsync()
    {
        await base.CloseAsync()
            .ConfigureAwait( false );

        try
        {
            Socket.Shutdown( SocketShutdown.Both );
        }
        catch ( Exception )
        { }

        try
        {
            Socket.Close();
        }
        catch ( Exception )
        {}

        try
        {
            await receiveTask.ConfigureAwait( false );
        }
        catch { }
    }

    protected override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await base.InitializeAsync( cancellationToken );

        receiveTask = ReceiveAsync( LifetimeToken );

        ScopeLogger(
            logger => logger.LogInformation( "Ready." )
        );
    }

    private async Task ReceiveAsync( CancellationToken cancellationToken )
    {
        var receiveBuffer = new byte[DefaultBufferLength];
        int bytesReceived;

        while ( !cancellationToken.IsCancellationRequested )
        {
            try
            {
                bytesReceived = await Socket.ReceiveAsync(
                    receiveBuffer,
                    SocketFlags.None,
                    cancellationToken
                )
                .ConfigureAwait( false );
            }
            catch ( OperationCanceledException )
            {
                break;
            }
            catch ( SocketException ex )
            {
                ScopeLogger(
                    logger => logger.LogError( ex, "Receive failed. {Message}", ex.Message )
                );

                _ = CloseAsync();

                break;
            }
            catch ( ObjectDisposedException )
            {
                break;
            }

            if ( bytesReceived <= 0 )
            {
                ScopeLogger(
                    logger => logger.LogTrace( "Disconnected." )
                );

                _ = CloseAsync();

                break;
            }

            var received = new byte[bytesReceived];
            System.Buffer.BlockCopy( receiveBuffer, 0, received, 0, bytesReceived );

            await ExecuteInputPipelineAsync( received ).ConfigureAwait( false );
        }
    }

    public override async Task WriteRawBytesAsync( byte[] data )
    {
        try
        {
            int totalBytesSent = 0;
            while ( totalBytesSent < data.Length )
            {
                var bytesSent = await Socket.SendAsync(
                    data.AsMemory( totalBytesSent ),
                    SocketFlags.None,
                    LifetimeToken
                )
                .ConfigureAwait( false );

                if ( bytesSent == 0 )
                {
                    throw new SocketException();
                }

                ScopeLogger(
                    logger => logger.LogTrace( "Sent {bytesSent} bytes", bytesSent )
                );

                totalBytesSent += bytesSent;
            }

            // trigger data sent
            NotifyDataSent( data );
        }
        catch ( Exception ex ) when ( ex is ObjectDisposedException || ex is SocketException )
        {
            // socket is closed
            ScopeLogger(
                logger => logger.LogTrace( "Disconnected." )
            );

            await CloseAsync();
        }
    }

    private async Task ExecuteInputPipelineAsync( byte[] data )
    {
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

            Buffer.Compact( pipelineBuffer.Offset );

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

            _ = CloseAsync();
        }
    }
}
