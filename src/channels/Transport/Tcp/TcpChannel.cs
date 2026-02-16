using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Faactory.Channels.Buffers;
using Faactory.Channels.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal sealed class TcpChannel : Channel, IChannel, IAsyncDisposable
{
    private const int DefaultBufferLength = 13312;

    private readonly ILogger logger;
    private readonly IDisposable? loggerScope;

    private readonly byte[] socketBuffer = new byte[DefaultBufferLength];

    private readonly Task initializeTask;
    private readonly CancellationTokenSource cts = new();

    internal TcpChannel( 
          IServiceScope serviceScope
        , Socket socket
        , ChannelOptions options
        , IChannelPipeline inputPipeline
        , IChannelPipeline outputPipeline
        , IEnumerable<IChannelService>? channelServices = null
        )
        : base( serviceScope )
    {
        logger = serviceScope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<TcpChannel>();

        loggerScope = logger.BeginScope( $"tcp-{Id[..7]}" );

        Buffer = new WritableByteBuffer( options.BufferEndianness );
        Timeout = options.IdleTimeout;
        Socket = socket;
        Input = inputPipeline;
        Output = outputPipeline;

        if ( channelServices != null )
        {
            Services = channelServices;
        }

        initializeTask = InitializeAsync( cts.Token );

        logger.LogDebug( "Created" );
    }

    public Socket Socket { get; init; }

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
            Socket.Dispose();
        }
        catch { }

        initializeTask.TryDispose();

        loggerScope?.Dispose();

        base.Dispose();
    }

    public override Task WriteAsync( object data )
    {
        if ( Volatile.Read( ref isClosing ) == 1 )
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
        await CloseAsync()
            .ConfigureAwait( false );

        GC.SuppressFinalize( this );
    }

    protected override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await base.InitializeAsync( cancellationToken );

        BeginReceive();

        logger.LogInformation( "Ready." );
    }

    #region Socket

    internal bool IsConnected()
    {
        try
        {
            var readStatus = Socket.Poll( 1000, SelectMode.SelectRead );
            var dataUnavailable = Socket.Available == 0;

            if ( ( !Socket.Connected ) || ( readStatus && dataUnavailable ) )
            {
                return false;
            }

            return true;
        }
        catch ( Exception )
        {
            return false;
        }
    }

    private void BeginReceive()
    {
        try
        {
            Socket.BeginReceive( socketBuffer
                , 0
                , socketBuffer.Length
                , 0
                , new AsyncCallback( ReadCallback ), this );
        }
        catch ( ObjectDisposedException )
        {
            // socket is closed
            logger.LogTrace( "Disconnected." );

            _ = CloseAsync();
        }
    }

    public override Task WriteRawBytesAsync( byte[] data )
    {
        try
        {
            Socket.BeginSend( data
                , 0
                , data.Length
                , 0
                , new AsyncCallback( SendCallback )
                , this );
        }
        catch ( ObjectDisposedException )
        {
            // socket is closed
            logger.LogTrace( "Disconnected." );

            _ = CloseAsync();
        }

        return Task.CompletedTask;
    }

    private void OnDataReceived( byte[] data )
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

    private void OnDataSent( int bytesSent )
    {
        LastSent = DateTimeOffset.UtcNow;

        this.NotifyDataSent( bytesSent );
    }

    private void ReadCallback( IAsyncResult ar )
    {
        // retrieve the channel object and the handler socket from the asynchronous state object
        var channel = (TcpChannel)ar.AsyncState!;
        var handler = channel.Socket;

        if ( handler.SafeHandle.IsClosed )
        {
            // socket is closed
            logger.LogTrace( "Disconnected." );

            _ = channel.CloseAsync();

            return;
        }

        // read data from the client socket
        int bytesReceived;

        try
        {
            bytesReceived = handler.EndReceive( ar );
        }
        catch ( Exception ex )
        {
            logger.LogError(
                ex, 
                "Failed to receive data. {Message}",
                ex.Message
                );

            handler.TryDisconnect();
            handler.TryClose();

            return;
        }

        if ( bytesReceived <= 0 )
        {
            // nothing was read
            logger.LogTrace( "Disconnected." );

            handler.TryClose();

            _ = channel.CloseAsync();

            return;
        }

        // trigger data received
        logger.LogTrace( "received {bytesReceived} bytes", bytesReceived );

        channel.OnDataReceived( 
            channel.socketBuffer.Take( bytesReceived )
                .ToArray() );

        // read more data
        channel.BeginReceive();
    }

    private void SendCallback( IAsyncResult ar )
    {
        // retrieve the connection object and the handler socket from the asynchronous state object
        var connection = (TcpChannel)ar.AsyncState!;
        var handler = connection.Socket;

        if ( handler.SafeHandle.IsClosed )
        {
            // socket is closed
            return;
        }

        try
        {
            // complete sending the data to the remote device
            var bytesSent = handler.EndSend( ar );

            // trigger data sent
            logger.LogTrace( "sent {bytesSent} bytes", bytesSent );

            connection.OnDataSent( bytesSent );
        }
        catch ( Exception ex )
        {
            logger.LogError( ex, "Failed to send data. {Message}", ex.Message );
        }
    }

    #endregion
}
