namespace Faactory.Channels;

public class ChannelOptions
{
    public IdleDetectionMode IdleDetectionMode { get; set; } = IdleDetectionMode.Auto;
    public TimeSpan IdleDetectionTimeout { get; set; } = TimeSpan.FromSeconds( 60 );
}
