using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal static class ChannelServiceExtensions
{
    public static void StartChannelServices( this Channel channel )
        => GetChannelServices( channel )
            .InvokeAll( x => x.Start( channel ) );

    public static void StopChannelServices( this Channel channel )
        => GetChannelServices( channel )
            .InvokeAll( x => x.Stop() );

    private static void InvokeAll( this IChannelService[] services, Action<IChannelService> notify )
    {
        foreach ( var service in services )
        {
            notify( service );
        }
    }

    private static IChannelService[] GetChannelServices( Channel channel )
        => channel.ServiceProvider.GetServices<IChannelService>()
            .ToArray();
}
