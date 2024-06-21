using Faactory.Channels;
using Faactory.Channels.Client;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ChannelsClientServiceExtensions
{
    public static INamedClientBuilder AddChannelsClient( this IServiceCollection services )
    {
        services.TryAddSingleton<IChannelsClientFactory, ChannelsClientFactory>();

        return new NamedClientBuilder( services );
    }

    public static INamedClientBuilder AddChannelsClient( this IServiceCollection services, Action<IChannelsClientBuilder> configure )
    {
        services.TryAddSingleton<IChannelsClientFactory, ChannelsClientFactory>();

        var builder = new NamedClientBuilder( services )
            .AddChannel( ChannelsClientFactory.DefaultChannelName, configure );

        return builder;
    }
}
