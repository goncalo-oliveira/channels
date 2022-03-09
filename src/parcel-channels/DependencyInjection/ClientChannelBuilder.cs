using Microsoft.Extensions.DependencyInjection;
using Parcel.Channels.Adapters;
using Parcel.Channels.Hosting;

namespace Parcel.Channels;

internal class ClientChannelBuilder : ChannelBuilder, IClientChannelBuilder
{
    public ClientChannelBuilder( IServiceCollection services )
        : base( services )
    {}

    public IClientChannelBuilder Configure( Action<ClientChannelOptions> configure )
        => Configure( "_default", configure );

    public IClientChannelBuilder Configure( string name, Action<ClientChannelOptions> configure )
    {
        Services.Configure( name, configure );

        return ( this );
    }
}
