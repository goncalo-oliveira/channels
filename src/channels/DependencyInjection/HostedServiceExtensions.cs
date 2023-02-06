using Faactory.Channels;

namespace Microsoft.Extensions.DependencyInjection;

public static class ChannelsHostedServiceExtensions
{
    /// <summary>
    /// Adds the channel hosted service to the specified service IServiceCollection
    /// </summary>
    public static IServiceCollection AddChannelsHostedService( this IServiceCollection services, Action<IServiceChannelBuilder> configure )
    {
        services.AddHostedService<ChannelHostedService>()
            .AddTransient<IServiceChannelFactory, ServiceChannelFactory>();

        var builder = new ServiceChannelBuilder( services );

        configure( builder );

        return ( services );
    }
}
