namespace Faactory.Channels.Adapters;

/// <summary>
/// A channel context used in adapter middleware
/// </summary>
public interface IAdapterContext : IChannelContext
{
    /// <summary>
    /// Forwards data to the next middleware
    /// </summary>
    void Forward( object data );
}
