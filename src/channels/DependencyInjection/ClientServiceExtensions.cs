using Faactory.Channels;
using Faactory.Channels.Adapters;

namespace Microsoft.Extensions.DependencyInjection;

public static class ChannelsClientServiceExtensions
{
    public static IServiceCollection AddChannelsClient( this IServiceCollection services, Action<IClientChannelBuilder> configure )
    {
        services.AddTransient<IClientChannelFactory, ClientChannelFactory>();

        var builder = new ClientChannelBuilder( services );

        configure( builder );

        builder.AddOutputAdapter<OutputChannelAdapter>();

        return ( services );

    }
}
