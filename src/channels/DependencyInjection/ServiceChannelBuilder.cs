using Microsoft.Extensions.DependencyInjection;
using Faactory.Channels.Adapters;
using Faactory.Channels.Hosting;

namespace Faactory.Channels;

internal class ServiceChannelBuilder : ChannelBuilder, IServiceChannelBuilder
{
    public ServiceChannelBuilder( IServiceCollection services )
        : base( services )
    {}

    public IServiceChannelBuilder Configure( Action<ServiceChannelOptions> configure )
    {
        Services.Configure( configure );

        return ( this );
    }
}
