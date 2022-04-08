using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Faactory.Channels.Adapters;
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
        var inputAdapters = serviceScope.ServiceProvider.GetAdapters<IInputChannelAdapter>();
        var outputAdapters = serviceScope.ServiceProvider.GetAdapters<IOutputChannelAdapter>();
        var inputHandlers = serviceScope.ServiceProvider.GetHandlers();

        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        var idleChannelMonitor = ( options.IdleDetectionMode > IdleDetectionMode.None )
            ? new IdleChannelMonitor( loggerFactory, options.IdleDetectionMode, options.IdleDetectionTimeout )
            : null;

        // create a TCP/IP socket
        var client = new Socket( SocketType.Stream, ProtocolType.Tcp );

        await client.ConnectAsync( options.Host, options.Port, cancellationToken );

        var channel = new ClientChannel( serviceScope
            , loggerFactory
            , client
            , inputAdapters
            , outputAdapters
            , inputHandlers
            , idleChannelMonitor );

        channel.BeginReceive();

        return ( channel );
    }
}
