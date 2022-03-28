namespace Faactory.Channels.Handlers;

/// <summary>
/// A channel data handler
/// </summary>
public interface IChannelHandler
{
    /// <summary>
    /// Executes the data handler
    /// </summary>
    /// <param name="context">The channel context</param>
    /// <param name="data">The data to process</param>
    Task ExecuteAsync( IChannelContext context, object data );
}
