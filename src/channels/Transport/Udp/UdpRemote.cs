using System.Net;
using System.Net.Sockets;

namespace Faactory.Channels.Udp;

internal sealed class UdpRemote( UdpClient udpClient, IPEndPoint remoteEndPoint )
{
    private readonly UdpClient socket = udpClient;
    private readonly IPEndPoint remoteEndPoint = remoteEndPoint;

    public ValueTask<int> SendAsync( ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default )
        => socket.SendAsync( data, remoteEndPoint, cancellationToken );
}
