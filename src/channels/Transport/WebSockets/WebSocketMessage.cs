using System.Net.WebSockets;
using Faactory.Channels.Buffers;

namespace Faactory.Channels.WebSockets;

/// <summary>
/// Represents a message sent or received by a WebSocket channel.
/// </summary>
public sealed class WebSocketMessage
{
    /// <summary>
    /// Gets the message type. The default value is <see cref="WebSocketMessageType.Binary"/>.
    /// </summary>
    public WebSocketMessageType Type { get; init; } = WebSocketMessageType.Binary;

    /// <summary>
    /// Gets the message data.
    /// </summary>
    public IByteBuffer Data { get; init; } = EmptyByteBuffer.Instance;

    /// <summary>
    /// Gets a value indicating whether the message is the end of a message. The default value is <c>true</c>.
    public bool EndOfMessage { get; init; } = true;
}
