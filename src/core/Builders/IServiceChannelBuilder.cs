namespace Faactory.Channels;

/// <summary>
/// An interface for configuring a service channel
/// </summary>
public interface IServiceChannelBuilder : IChannelBuilder
{
    /// <summary>
    /// Configure the builder with the service options
    /// </summary>
    /// <param name="configure">The action used to configure the channel service</param>
    IServiceChannelBuilder Configure( Action<ServiceChannelOptions> configure );
}
