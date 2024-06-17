namespace System.Net.Sockets;

internal static class SocketExtensions
{
    public static void TryClose( this Socket socket )
    {
        try
        {
            socket.Close();
        }
        catch ( Exception )
        {}
    }

    public static void TryDisconnect( this Socket socket )
    {
        try
        {
            socket.Disconnect( false );
        }
        catch ( Exception )
        {}
    }
}
