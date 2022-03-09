using Parcel.Channels;
using Parcel.Channels.Adapters;

namespace Microsoft.Extensions.DependencyInjection;

public static class ParcelClientServiceExtensions
{
    public static IServiceCollection AddParcelClient( this IServiceCollection services, Action<IClientChannelBuilder> configure )
    {
        services.AddTransient<IClientChannelFactory, ClientChannelFactory>();

        var builder = new ClientChannelBuilder( services );

        configure( builder );

        builder.AddOutputAdapter<OutputChannelAdapter>();

        return ( services );

    }
}
