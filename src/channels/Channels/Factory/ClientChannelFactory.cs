using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Faactory.Channels.Adapters;

namespace Faactory.Channels;

internal class ClientChannelFactory : ChannelFactory, IClientChannelFactory
{
    private readonly ILoggerFactory loggerFactory;
    private readonly IOptionsMonitor<ClientChannelOptions> optionsMonitor;

    public ClientChannelFactory( ILoggerFactory loggerFactory
        , IServiceProvider serviceProvider
        , IOptionsMonitor<ClientChannelOptions> optionsMonitor )
        : base( serviceProvider )
    {
        this.loggerFactory = loggerFactory;
        this.optionsMonitor = optionsMonitor;
    }

    public Task<IChannel> CreateAsync( CancellationToken cancellationToken )
        => CreateAsync( "_default", cancellationToken );

    public async Task<IChannel> CreateAsync( string name, CancellationToken cancellationToken )
    {
        var options = optionsMonitor.Get( name );

        var inputAdapters = GetAdapters<IInputChannelAdapter>();
        var outputAdapters = GetAdapters<IOutputChannelAdapter>();
        var inputHandlers = GetHandlers();

        var idleChannelMonitor = ( options.IdleDetectionMode > IdleDetectionMode.None )
            ? new IdleChannelMonitor( loggerFactory, options.IdleDetectionMode, options.IdleDetectionTimeout )
            : null;

        // create a TCP/IP socket
        var client = new Socket( SocketType.Stream, ProtocolType.Tcp );

        await client.ConnectAsync( options.Host, options.Port, cancellationToken );

        var channel = new ClientChannel( loggerFactory
            , client
            , inputAdapters
            , outputAdapters
            , inputHandlers
            , idleChannelMonitor );

        channel.BeginReceive();

        return ( channel );
    }
}
