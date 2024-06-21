using Faactory.Channels;

namespace Microsoft.Extensions.DependencyInjection;

public static class ChannelsHostedServiceExtensions
{
    /// <summary>
    /// Adds the channels middleware to the service collection and returns a builder for configuring named channels.
    /// </summary>
    public static INamedServiceChannelBuilder AddChannels( this IServiceCollection services )
    {
        services.AddHostedService<ChannelsHostedService>()
            .AddTransient<IServiceChannelFactory, ServiceChannelFactory>();

        return new NamedServiceChannelBuilder( services );
    }

    /// <summary>
    /// Adds the channels middleware to the service collection and returns a builder for configuring named channels.
    /// </summary>
    /// <param name="configure">A delegate that configures the default channel.</param>
    public static INamedServiceChannelBuilder AddChannels( this IServiceCollection services, Action<IServiceChannelBuilder> configure )
    {
        services.AddHostedService<ChannelsHostedService>()
            .AddTransient<IServiceChannelFactory, ServiceChannelFactory>();

        var builder = new NamedServiceChannelBuilder( services )
            .AddChannel( ServiceChannelFactory.DefaultChannelName, configure );

        return builder;
    }
}
