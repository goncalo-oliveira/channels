namespace Faactory.Channels;

public class IdleChannelServiceOptions
{
    /// <summary>
    /// Idle detection mode. Default is IdleDetectionMode.Auto
    /// </summary>
    public IdleDetectionMode DetectionMode { get; set; } = IdleDetectionMode.Auto;

    /// <summary>
    /// Idle timeout. Default is 60 seconds
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds( 60 );
}
