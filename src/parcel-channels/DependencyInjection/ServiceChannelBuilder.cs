using Microsoft.Extensions.DependencyInjection;
using Parcel.Channels.Adapters;
using Parcel.Channels.Hosting;

namespace Parcel.Channels;

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
