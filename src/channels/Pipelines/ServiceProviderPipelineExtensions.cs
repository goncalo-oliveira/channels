using Microsoft.Extensions.DependencyInjection;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;

namespace Faactory.Channels;

internal static class ServiceProviderPipelineExtensions
{
    public static IEnumerable<IChannelAdapter> GetAdapters<TPipeline>( this IServiceProvider serviceProvider, string channelName )
        => serviceProvider.GetKeyedServices<TPipeline>( channelName )
            .Cast<IChannelAdapter>()
            .ToArray();

    public static IEnumerable<IChannelHandler> GetHandlers( this IServiceProvider serviceProvider, string channelName )
        => serviceProvider.GetKeyedServices<IChannelHandler>( channelName )
            .ToArray();
}
