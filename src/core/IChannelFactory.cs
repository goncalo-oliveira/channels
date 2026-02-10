namespace Faactory.Channels;

/// <summary>
/// A factory for creating channels
/// </summary>
public interface IChannelFactory
{
    /// <summary>
    /// The services registered used for creating channels
    /// </summary>
    IServiceProvider ChannelServices { get; }
}
