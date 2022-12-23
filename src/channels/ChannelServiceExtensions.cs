using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal static class ChannelServiceExtensions
{
    public static void StartChannelServices( this Channel channel )
        => channel.ServiceProvider.GetServices<IChannelService>()
            .InvokeAll( x => x.Start( channel ) );

    public static void StopChannelServices( this Channel channel )
        => channel.ServiceProvider.GetServices<IChannelService>()
            .InvokeAll( x => x.Stop() );

    private static void InvokeAll( this IEnumerable<IChannelService> services, Action<IChannelService> invoke )
    {
        foreach ( var service in services )
        {
            invoke( service );
        }
    }
}
