namespace Faactory.Channels;

public class ChannelOptions
{
    public Buffers.Endianness BufferEndianness { get; set; } = Buffers.Endianness.BigEndian;
    public IdleDetectionMode IdleDetectionMode { get; set; } = IdleDetectionMode.Auto;
    public TimeSpan IdleDetectionTimeout { get; set; } = TimeSpan.FromSeconds( 60 );
}
