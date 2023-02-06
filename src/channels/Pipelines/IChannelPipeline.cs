namespace Faactory.Channels;

/// <summary>
/// A channel pipeline
/// </summary>
public interface IChannelPipeline : IDisposable
{
    /// <summary>
    /// Executes the pipeline
    /// </summary>
    Task ExecuteAsync( IChannel channel, object data );
}
