using Parcel.Channels.Hosting;

namespace Parcel.Channels;

/// <summary>
/// An interface for configuring a client channel
/// </summary>
public interface IClientChannelBuilder : IChannelBuilder
{
    /// <summary>
    /// Configure the builder with the client options
    /// </summary>
    /// <param name="configure">The action used to configure the channel</param>
    IClientChannelBuilder Configure( Action<ClientChannelOptions> configure );

    /// <summary>
    /// Configure the builder with the client options
    /// </summary>
    /// <param name="name">The name of the options instance</param>
    /// <param name="configure">The action used to configure the channel</param>
    IClientChannelBuilder Configure( string name, Action<ClientChannelOptions> configure );
}
