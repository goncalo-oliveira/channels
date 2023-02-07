using System.Net.Sockets;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal class ClientChannelFactory : IClientChannelFactory
{
    private readonly IServiceProvider serviceProvider;
    private readonly IOptionsMonitor<ClientChannelOptions> optionsMonitor;

    public ClientChannelFactory( IServiceProvider serviceProvider
        , IOptionsMonitor<ClientChannelOptions> optionsMonitor )
    {
        this.serviceProvider = serviceProvider;
        this.optionsMonitor = optionsMonitor;
    }

    public Task<IChannel> CreateAsync( CancellationToken cancellationToken )
        => CreateAsync( "_default", cancellationToken );

    public async Task<IChannel> CreateAsync( string name, CancellationToken cancellationToken )
    {
        var options = optionsMonitor.Get( name );

        var serviceScope = serviceProvider.CreateScope();

        // create a TCP/IP socket
        var client = new Socket( SocketType.Stream, ProtocolType.Tcp );

        await client.ConnectAsync( options.Host, options.Port, cancellationToken )
            .ConfigureAwait( false );

        var channel = new ClientChannel(
              serviceScope
            , client
            , options.BufferEndianness );

        await channel.InitializeAsync();

        channel.BeginReceive();

        return ( channel );
    }
}
