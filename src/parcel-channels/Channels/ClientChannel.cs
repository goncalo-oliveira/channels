using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parcel.Buffers;
using Parcel.Channels.Adapters;
using Parcel.Channels.Handlers;
using Parcel.Sockets;

namespace Parcel.Channels;

public class ClientChannel : Channel
{
    private readonly ILoggerFactory loggerFactory;
    public ClientChannel( ILoggerFactory loggerFactory
        , Socket socket
        , IEnumerable<IChannelAdapter> inputAdapters
        , IEnumerable<IChannelAdapter> outputAdapters
        , IEnumerable<IChannelHandler> inputHandlers
        , IIdleChannelMonitor? idleChannelMonitor )
        : base( loggerFactory, socket, idleChannelMonitor )
    {
        this.loggerFactory = loggerFactory;

        Input = new ChannelPipeline(  loggerFactory, inputAdapters, inputHandlers );
        Output = new ChannelPipeline( loggerFactory, outputAdapters, null );
    }

    public ClientChannel( 
          ILoggerFactory loggerFactory
        , Socket socket
        , IdleDetectionMode idleDetectionMode
        , TimeSpan idleDetectionTimeout )
        : base( 
              loggerFactory
            , socket
            , new IdleChannelMonitor( loggerFactory, idleDetectionMode, idleDetectionTimeout ) )
    {
        this.loggerFactory = loggerFactory;
    }

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
