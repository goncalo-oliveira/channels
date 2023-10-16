using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels.Sockets;

/// <summary>
/// A connected channel TCP socket
/// </summary>
public abstract class ConnectedSocket
{
    public const int DefaultBufferLength = 13312;

    private readonly byte[] socketBuffer = new byte[DefaultBufferLength];

    internal ConnectedSocket( ILoggerFactory loggerFactory, Socket socket )
    {
        Id = Guid.NewGuid().ToString( "N" );
        Socket = socket;

        Logger = loggerFactory.CreateLogger( $"ConnectedSocket_{Id[..7]}" );
    }

    private ILogger Logger { get; init; }

    public string Id { get; }

    public Socket Socket { get; init; }

    internal bool IsShutdown { get; private set; }

    internal bool IsConnected()
    {
        try
        {
            var readStatus = Socket.Poll( 1000, SelectMode.SelectRead );
            var dataUnavailable = ( Socket.Available == 0 );

            if ( ( !Socket.Connected ) || ( readStatus && dataUnavailable ) )
            {
                return ( false );
            }

            return ( true );
        }
        catch ( Exception )
        {
            return ( false );
        }
    }

    internal void BeginReceive()
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
            Logger.LogTrace( "Disconnected." );

            OnDisconnected();
        }
    }

    public void Send( byte[] data )
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
            Logger.LogTrace( "Disconnected." );

            OnDisconnected();
        }
    }

    protected virtual void OnDataReceived( byte[] data )
    {}
    
    protected virtual void OnDataSent( int bytesSent )
    {}

    protected virtual void OnDisconnected()
    {}

    protected void Shutdown()
    {
        try
        {
            Socket.Shutdown( SocketShutdown.Both );
        }
        catch ( Exception )
        { }

        IsShutdown = true;
    }

    private static void ReadCallback( IAsyncResult ar )
    {
        // retrieve the connection object and the handler socket from the asynchronous state object
        var connection = (ConnectedSocket)ar.AsyncState!;
        var handler = connection.Socket;

        if ( handler.SafeHandle.IsClosed )
        {
            // socket is closed
            connection.Logger.LogTrace( "Disconnected." );

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
            connection.Logger.LogError(
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
            connection.Logger.LogTrace( "Disconnected." );

            handler.TryClose();

            connection.OnDisconnected();

            return;
        }

        // trigger data received
        connection.Logger.LogTrace( "received {bytesReceived} bytes", bytesReceived );

        connection.OnDataReceived( 
            connection.socketBuffer.Take( bytesReceived )
                .ToArray() );

        // read more data
        connection.BeginReceive();
    }

    private static void SendCallback( IAsyncResult ar )
    {
        // retrieve the connection object and the handler socket from the asynchronous state object
        var connection = (ConnectedSocket)ar.AsyncState!;
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
            connection.Logger.LogTrace( "sent {bytesSent} bytes", bytesSent );

            connection.OnDataSent( bytesSent );
        }
        catch ( Exception ex )
        {
            connection.Logger.LogError( ex, "Failed to send data. {Message}", ex.Message );
        }
    }
}
