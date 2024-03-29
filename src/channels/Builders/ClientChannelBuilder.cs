using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal class ClientChannelBuilder( IServiceCollection services ) : ChannelBuilder( services ), IClientChannelBuilder
{
    public IClientChannelBuilder Configure( Action<ClientChannelOptions> configure )
        => Configure( "_default", configure );

    public IClientChannelBuilder Configure( string name, Action<ClientChannelOptions> configure )
    {
        Services.Configure( name, configure );

        return this;
    }
}
