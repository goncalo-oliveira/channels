using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Faactory.Channels.Buffers;
using Faactory.Channels.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal sealed class TcpChannel : Channel, IChannel
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

    private bool IsShutdown { get; set; }

    public override Task CloseAsync()
    {
        try
        {
            cts.Cancel();
        }
        catch { }

        try
        {
            Shutdown();
        }
        catch ( Exception )
        { }
        finally
        {
            OnDisconnected();
        }

        try
        {
            Socket.Close();
        }
        catch ( Exception )
        {}

        return Task.CompletedTask;
    }

    public override Task WriteAsync( object data )
    {
        if ( IsShutdown )
        {
            logger.LogWarning( "Can't write to a closed channel." );

            return Task.CompletedTask;
        }

        return base.WriteAsync( data );
    }

    public override void Dispose()
    {
        try
        {
            cts.Cancel();
        }
        catch { }

        base.Dispose();

        try
        {
            Socket.Dispose();
        }
        catch { }

        initializeTask.TryDispose();

        logger.LogDebug( "Disposed." );

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

            OnDisconnected();
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

            OnDisconnected();
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

        logger.LogDebug( "Stopping services." );

        await StopServicesAsync()
            .ConfigureAwait( false );

        logger.LogDebug( "Stopped services." );

        Dispose();
    }

    private void Shutdown()
    {
        try
        {
            Socket.Shutdown( SocketShutdown.Both );
        }
        catch ( Exception )
        { }

        IsShutdown = true;
    }

    private void ReadCallback( IAsyncResult ar )
    {
        // retrieve the connection object and the handler socket from the asynchronous state object
        var connection = (TcpChannel)ar.AsyncState!;
        var handler = connection.Socket;

        if ( handler.SafeHandle.IsClosed )
        {
            // socket is closed
            logger.LogTrace( "Disconnected." );

            connection.OnDisconnected();

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

            connection.OnDisconnected();

            return;
        }

        // trigger data received
        logger.LogTrace( "received {bytesReceived} bytes", bytesReceived );

        connection.OnDataReceived( 
            connection.socketBuffer.Take( bytesReceived )
                .ToArray() );

        // read more data
        connection.BeginReceive();
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
