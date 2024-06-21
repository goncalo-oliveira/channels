namespace Faactory.Channels;

public class ChannelOptions
{
    /// <summary>
    /// Gets or sets the channel's input buffer endianness; default is Buffers.Endianness.BigEndian.
    /// </summary>
    public Buffers.Endianness BufferEndianness { get; set; } = Buffers.Endianness.BigEndian;
}
