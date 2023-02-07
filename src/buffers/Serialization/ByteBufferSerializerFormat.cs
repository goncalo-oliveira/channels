namespace Faactory.Channels.Buffers.Serialization;

public enum ByteBufferSerializerFormat
{
    /// <summary>
    /// Buffer is serialized as a base64 string
    /// </summary>
    Base64,

    /// <summary>
    /// Buffer is serialized as an hex string
    /// </summary>
    HexString,
}
