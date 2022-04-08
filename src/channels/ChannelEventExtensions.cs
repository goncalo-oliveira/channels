using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal static class ChannelEventExtensions
{
    public static void NotifyChannelCreated( this Channel channel )
        => GetEventServices( channel )
            .NotifyEvent( x => x.ChannelCreated( channel.Info ) );

    public static void NotifyChannelClosed( this Channel channel )
        => GetEventServices( channel )
            .NotifyEvent( x => x.ChannelClosed( channel.Info ) );

    public static void NotifyDataReceived( this Channel channel, byte[] data )
        => GetEventServices( channel )
            .NotifyEvent( x => x.DataReceived( channel.Info, data ) );

    public static void NotifyDataSent( this Channel channel, int sent )
        => GetEventServices( channel )
            .NotifyEvent( x => x.DataSent( channel.Info, sent ) );

    private static void NotifyEvent( this IChannelEvents[] services, Action<IChannelEvents> notify )
    {
        foreach ( var service in services )
        {
            notify( service );
        }
    }

    private static IChannelEvents[] GetEventServices( Channel channel )
        => channel.ServiceProvider.GetServices<IChannelEvents>()
            .ToArray();
}
