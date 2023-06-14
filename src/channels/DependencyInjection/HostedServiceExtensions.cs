using Faactory.Channels;

namespace Microsoft.Extensions.DependencyInjection;

public static class ChannelsHostedServiceExtensions
{
    /// <summary>
    /// Adds the channel hosted service to the specified service IServiceCollection
    /// </summary>
    public static IServiceCollection AddChannels( this IServiceCollection services, Action<IServiceChannelBuilder> configure )
    {
        services.AddHostedService<ChannelsHostedService>()
            .AddTransient<IServiceChannelFactory, ServiceChannelFactory>();

        var builder = new ServiceChannelBuilder( services );

        configure( builder );

        return ( services );
    }
}
