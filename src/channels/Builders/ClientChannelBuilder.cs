using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

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
