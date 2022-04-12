using Faactory.Channels;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;

namespace Microsoft.Extensions.DependencyInjection;

public static class ChannelsClientServiceExtensions
{
    public static IServiceCollection AddChannelsClient( this IServiceCollection services, Action<IClientChannelBuilder> configure )
    {
        services.AddTransient<IClientChannelFactory, ClientChannelFactory>();

        var builder = new ClientChannelBuilder( services );

        configure( builder );

        //builder.Services.AddTransient<IChannelHandler, OutputChannelHandler>();

        return ( services );

    }
}
