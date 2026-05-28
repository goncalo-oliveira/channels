namespace Faactory.Channels.Buffers;

/// <summary>
/// Options for configuring the behavior of a <see cref="WritableByteBuffer"/> instance.
/// </summary>
public sealed class WritableByteBufferOptions
{
    /// <summary>
    /// Gets a default instance of <see cref="WritableByteBufferOptions"/> with default values for all properties.
    /// </summary>
    public static readonly WritableByteBufferOptions Default = new();

    /// <summary>
    /// Gets or sets the endianness of the buffer.
    /// </summary>
    public Endianness Endianness { get; init; } = Endianness.BigEndian;

    /// <summary>
    /// Gets or sets the initial capacity of the buffer.
    /// This determines the initial size of the underlying byte array used to store data in the buffer.
    /// The default value is 1024 bytes (1 KB).
    /// </summary>
    public int InitialCapacity { get; init; } = 1024;

    /// <summary>
    /// Gets or sets the maximum retained capacity of the buffer.
    /// This limits the maximum size of the underlying byte array that can be retained by the buffer after compaction.
    /// If the buffer's capacity exceeds this value during compaction, it will be reduced to this maximum retained capacity.
    /// The default value is 1024 bytes (1 KB).
    /// </summary>
    public int MaxRetainedCapacity { get; init; } = 1024;
}
