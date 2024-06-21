using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Faactory.Channels.Client;

internal sealed class ChannelsClientFactory( IServiceProvider serviceProvider ) : IChannelsClientFactory
{
    internal const string DefaultChannelName = "__client-default";
    
    public IChannelsClient Create( string name )
    {
        var scope = ServiceProvider.CreateScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ChannelsClientOptions>>()
            .Get( name );

        return new ChannelsClient( scope, options, name );
    }

    private IServiceProvider ServiceProvider { get; } = serviceProvider;
}
