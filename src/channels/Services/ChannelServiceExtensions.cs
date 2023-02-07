using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

public static class ChannelServiceControlExtensions
{
    internal static Task StartChannelServicesAsync( this Channel channel, CancellationToken cancellationToken = default )
        => channel.Services.InvokeAllAsync( x => x.StartAsync( channel, cancellationToken ) );

    internal static Task StopChannelServicesAsync( this Channel channel, CancellationToken cancellationToken = default )
        => channel.Services.InvokeAllAsync( x => x.StopAsync( cancellationToken ) );
}
