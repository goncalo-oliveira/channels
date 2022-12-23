namespace Faactory.Channels;

public class IdleChannelServiceOptions
{
    /// <summary>
    /// Idle detection mode; default is IdleDetectionMode.Auto
    /// </summary>
    public IdleDetectionMode DetectionMode { get; set; } = IdleDetectionMode.Auto;

    /// <summary>
    /// Idle timeout; default is 60 seconds
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds( 60 );
}
