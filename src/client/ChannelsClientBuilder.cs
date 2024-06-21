using Faactory.Channels.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

/// <summary>
/// An interface for configuring a client channel
/// </summary>
public interface IChannelsClientBuilder : IChannelBuilder<IChannelsClientBuilder>
{
    /// <summary>
    /// Configure the builder with the client options
    /// </summary>
    /// <param name="configure">The action used to configure the channel</param>
    IChannelsClientBuilder Configure( Action<ChannelsClientOptions> configure );
}

internal class ChannelsClientBuilder( IServiceCollection services, string channelName ) : ChannelBuilder<IChannelsClientBuilder>( services, channelName ), IChannelsClientBuilder
{
    public IChannelsClientBuilder Configure( Action<ChannelsClientOptions> configure )
    {
        Services.Configure( Name, configure );

        return Self();
    }

    protected override IChannelsClientBuilder Self() => this;
}
