using Parcel.Channels;
using Parcel.Channels.Adapters;
using Parcel.Channels.Hosting;
using Parcel.Sockets;

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
