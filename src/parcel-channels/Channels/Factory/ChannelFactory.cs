using Microsoft.Extensions.DependencyInjection;
using Parcel.Channels.Adapters;
using Parcel.Channels.Handlers;

namespace Parcel.Channels;

internal abstract class ChannelFactory
{
    private readonly IServiceProvider serviceProvider;

    public ChannelFactory( IServiceProvider serviceProvider )
    {
        this.serviceProvider = serviceProvider;
    }

    protected IEnumerable<IChannelAdapter> GetAdapters<TPipeline>()
        => serviceProvider.GetServices<TPipeline>()
            .Cast<IChannelAdapter>()
            .ToArray();

    protected IEnumerable<IChannelHandler> GetHandlers()
        => serviceProvider.GetServices<IChannelHandler>()
            .ToArray();
}
