using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal static class ChannelServiceExtensions
{
    public static Task StartChannelServicesAsync( this Channel channel, CancellationToken cancellationToken = default )
        => channel.GetChannelServices()
            .InvokeAllAsync( x => x.StartAsync( channel, cancellationToken ) );

    public static Task StopChannelServicesAsync( this Channel channel, CancellationToken cancellationToken = default )
        => channel.GetChannelServices()
            .InvokeAllAsync( x => x.StopAsync( cancellationToken ) );

    private static IEnumerable<IChannelService> GetChannelServices( this Channel channel )
        => channel.ServiceProvider.GetServices<IChannelService>();
}
