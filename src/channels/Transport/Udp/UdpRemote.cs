using System.Net;
using System.Net.Sockets;

namespace Faactory.Channels.Udp;

internal sealed class UdpRemote( UdpClient udpClient, IPEndPoint? remoteEndPoint ) : IEquatable<UdpRemote>
{
    private readonly UdpClient socket = udpClient;
    private readonly IPEndPoint? remoteEndPoint = remoteEndPoint;

    public string Id { get; } = remoteEndPoint?.ToString() ?? string.Empty;

    public string ChannelId { get; set; } = string.Empty;

    public ValueTask<int> SendAsync( ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default )
    {
        if ( remoteEndPoint != null )
        {
            return socket.SendAsync( data, remoteEndPoint, cancellationToken );
        }

        return socket.SendAsync( data, cancellationToken );
    }

    public override int GetHashCode()
        => Id.GetHashCode();

    public bool Equals( UdpRemote? other )
    {
        if ( other is null )
        {
            return false;
        }

        return Id == other.Id;
    }

    public override bool Equals( object? obj )
        => Equals(obj as UdpRemote);
}
