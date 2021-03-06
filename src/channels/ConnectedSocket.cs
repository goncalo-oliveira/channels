using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Faactory.Channels.Sockets;

public class ConnectedSocket
{
    public const int DefaultBufferLength = 13312;

    private readonly byte[] socketBuffer = new byte[DefaultBufferLength];

    public ConnectedSocket( ILoggerFactory loggerFactory, Socket socket )
    {
        Id = Guid.NewGuid().ToString( "N" );
        Socket = socket;

        Logger = loggerFactory.CreateLogger( $"socket-{Id.Substring( 0, 6 )}" );
    }

    private ILogger Logger { get; init; }

    public string Id { get; }

    public Socket Socket { get; init; }

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
        Socket.BeginReceive( socketBuffer
            , 0
            , socketBuffer.Length
            , 0
            , new AsyncCallback( ReadCallback ), this );
    }

    public void Send( byte[] data )
    {
        Socket.BeginSend( data
            , 0
            , data.Length
            , 0
            , new AsyncCallback( SendCallback )
            , this );
    }

    protected virtual void OnDataReceived( byte[] data )
    {}
    
    protected virtual void OnDataSent( int bytesSent )
    {}

    protected virtual void OnDisconnected()
    {}

    private static void ReadCallback( IAsyncResult ar )
    {
        // retrieve the connection object and the handler socket from the asynchronous state object
        var connection = (ConnectedSocket)ar.AsyncState!;
        var handler = connection.Socket;

        // var json = JsonSerializer.Serialize( handler );
        // connection.Logger.LogDebug( json );

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
            connection.Logger.LogError( ex, $"Failed to receive data. {ex.Message}" );

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
        connection.Logger.LogTrace( $"received {bytesReceived} bytes" );

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
            connection.Logger.LogTrace( $"sent {bytesSent} bytes" );

            connection.OnDataSent( bytesSent );
        }
        catch ( Exception ex )
        {
            connection.Logger.LogError( ex, $"Failed to send data. {ex.Message}" );
        }
    }
}
