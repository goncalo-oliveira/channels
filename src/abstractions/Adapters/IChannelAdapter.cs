namespace Faactory.Channels.Adapters;

/// <summary>
/// A channel data adapter
/// </summary>
public interface IChannelAdapter
{
    /// <summary>
    /// Executes the data adapter
    /// </summary>
    /// <param name="context">The adapter context</param>
    /// <param name="data">The data to adapt</param>
    Task ExecuteAsync( IAdapterContext context, object data );
}
