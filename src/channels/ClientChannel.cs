using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Faactory.Channels.Buffers;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
using Faactory.Sockets;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

public class ClientChannel : Channel
{
    public ClientChannel( IServiceScope serviceScope
        , ILoggerFactory loggerFactory
        , Socket socket
        , IEnumerable<IChannelAdapter> inputAdapters
        , IEnumerable<IChannelAdapter> outputAdapters
        , IEnumerable<IChannelHandler> inputHandlers
        , IIdleChannelMonitor? idleChannelMonitor )
        : base( serviceScope, loggerFactory, socket, idleChannelMonitor )
    {
        Input = new ChannelPipeline(  loggerFactory, inputAdapters, inputHandlers );
        Output = new ChannelPipeline( loggerFactory, outputAdapters, null );
    }

    public ClientChannel( IServiceScope serviceScope
        , ILoggerFactory loggerFactory
        , Socket socket
        , IdleDetectionMode idleDetectionMode
        , TimeSpan idleDetectionTimeout )
        : base( 
              serviceScope
            , loggerFactory
            , socket
            , new IdleChannelMonitor( loggerFactory, idleDetectionMode, idleDetectionTimeout ) )
    { }

    public override async Task CloseAsync()
    {
        Socket.Shutdown( SocketShutdown.Both );

        try
        {
            await Socket.DisconnectAsync( false );
        }
        catch ( Exception )
        {}

        try
        {
            Socket.Close();
        }
        catch ( Exception )
        {}
    }
}
