using Faactory.Channels;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
using Faactory.Channels.Hosting;
using Faactory.Sockets;

namespace Microsoft.Extensions.DependencyInjection;

public static class ChannelsHostedServiceExtensions
{
    public static IServiceCollection AddChannelsHostedService( this IServiceCollection services, Action<IServiceChannelBuilder> configure )
    {
        services.AddHostedService<ChannelsHostedService>()
            .AddTransient<IServiceChannelFactory, ServiceChannelFactory>();

        var builder = new ServiceChannelBuilder( services );

        configure( builder );

        //builder.Services.AddTransient<IChannelHandler, OutputChannelHandler>();

        return ( services );
    }
}
