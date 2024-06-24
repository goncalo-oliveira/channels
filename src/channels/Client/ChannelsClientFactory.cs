using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Faactory.Channels.Client;

internal sealed class ChannelName
{
    public required string Name { get; set; }
}

internal sealed class ChannelsClientFactory( IServiceProvider serviceProvider ) : IChannelsClientFactory
{
    public const string DefaultClientName = "__default_client";

    public IChannelsClient Create( string name )
    {
        var scope = ServiceProvider.CreateScope();

        /*
        this gets the channel name (pipeline configuration) bound to the client name
        */
        var channelName = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ChannelName>>()
            .Get( name ).Name;

        /*
        get the client options
        */
        var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ChannelsClientOptions>>()
            .Get( name );

        if ( options.TransportMode == ChannelTransportMode.Tcp )
        {
            return new Tcp.TcpClient( scope, options, channelName );
        }

        if ( options.TransportMode == ChannelTransportMode.Udp )
        {
            return new Udp.ChannelsUdpClient( scope, options, channelName );
        }

        throw new NotSupportedException( $"Transport mode {options.TransportMode} is not supported." );
    }

    private IServiceProvider ServiceProvider { get; } = serviceProvider;
}
