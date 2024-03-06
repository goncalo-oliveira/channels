using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal static class ChannelMonitorExtensions
{
    public static void NotifyChannelCreated( this Channel channel )
        => channel.GetMonitorServices()
            .InvokeAll( x => x.ChannelCreated( channel.Info ) );

    public static void NotifyChannelClosed( this Channel channel )
        => channel.GetMonitorServices()
            .InvokeAll( x => x.ChannelClosed( channel.Info ) );

    public static void NotifyDataReceived( this Channel channel, byte[] data )
        => channel.GetMonitorServices()
            .InvokeAll( x => x.DataReceived( channel.Info, data ) );

    public static void NotifyDataSent( this Channel channel, int sent )
        => channel.GetMonitorServices()
            .InvokeAll( x => x.DataSent( channel.Info, sent ) );

    public static void NotifyCustomEvent( this Channel channel, string name, object? data )
        => channel.GetMonitorServices()
            .InvokeAll( x => x.CustomEvent( channel.Info, name, data ) );

    private static IEnumerable<IChannelMonitor> GetMonitorServices( this Channel channel )
        => channel.ServiceProvider.GetServices<IChannelMonitor>();
}
