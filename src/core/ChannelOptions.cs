namespace Faactory.Channels;

public class ChannelOptions
{
    /// <summary>
    /// Gets or sets the channel's input buffer endianness; default is Buffers.Endianness.BigEndian.
    /// </summary>
    public Buffers.Endianness BufferEndianness { get; set; } = Buffers.Endianness.BigEndian;

    /// <summary>
    /// Gets or sets the channel's timeout value when no data is sent or received; default is 60 seconds. Set to TimeSpan.Zero to disable.
    /// </summary>
    public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromSeconds( 60 );
}
