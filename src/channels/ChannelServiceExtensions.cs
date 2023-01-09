using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal static class ChannelServiceExtensions
{
    public static void StartChannelServices( this Channel channel )
        => channel.GetChannelServices()
            .InvokeAll( x => x.Start( channel ) );

    public static void StopChannelServices( this Channel channel )
        => channel.GetChannelServices()
            .InvokeAll( x => x.Stop() );

    private static IEnumerable<IChannelService> GetChannelServices( this Channel channel )
        => channel.ServiceProvider.GetServices<IChannelService>();
}
