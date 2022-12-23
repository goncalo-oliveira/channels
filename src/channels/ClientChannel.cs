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
        , Buffers.Endianness bufferEndianness )
        : base( serviceScope, loggerFactory, socket, bufferEndianness )
    {
        Input = new ChannelPipeline(  loggerFactory, inputAdapters, inputHandlers );
        Output = new ChannelPipeline( loggerFactory, outputAdapters, new IChannelHandler[]
        {
            new OutputChannelHandler( loggerFactory )
        } );
    }

    public ClientChannel( IServiceScope serviceScope
        , ILoggerFactory loggerFactory
        , Socket socket
        , Buffers.Endianness bufferEndianness )
        : base( 
              serviceScope
            , loggerFactory
            , socket
            , bufferEndianness )
    { }

    public override async Task CloseAsync()
    {
        Socket.Shutdown( SocketShutdown.Both );

        try
        {
            await Socket.DisconnectAsync( false )
                .ConfigureAwait( false );
        }
        catch ( Exception )
        {
            OnDisconnected();
        }

        try
        {
            Socket.Close();
        }
        catch ( Exception )
        {}
    }
}
