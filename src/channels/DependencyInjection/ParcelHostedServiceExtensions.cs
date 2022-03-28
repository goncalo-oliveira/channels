using Faactory.Channels;
using Faactory.Channels.Adapters;
using Faactory.Channels.Hosting;
using Faactory.Sockets;

namespace Microsoft.Extensions.DependencyInjection;

public static class ParcelHostedServiceExtensions
{
    public static IServiceCollection AddParcelHostedService( this IServiceCollection services, Action<IServiceChannelBuilder> configure )
    {
        services.AddHostedService<ParcelHostedService>()
            .AddTransient<IServiceChannelFactory, ServiceChannelFactory>();

        var builder = new ServiceChannelBuilder( services );

        configure( builder );

        builder.AddOutputAdapter<OutputChannelAdapter>();

        return ( services );
    }
}
