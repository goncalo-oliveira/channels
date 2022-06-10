using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Faactory.Channels.Buffers;
using Faactory.Channels;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Sockets;

internal sealed class ServiceChannel : Channel
{
    public ServiceChannel( IServiceScope serviceScope
        , ILoggerFactory loggerFactory
        , Socket socket
        , IEnumerable<IChannelAdapter> inputAdapters
        , IEnumerable<IChannelAdapter> outputAdapters
        , IEnumerable<IChannelHandler> inputHandlers
        , IIdleChannelMonitor? idleChannelMonitor
        , Endianness bufferEndianness )
        : base( serviceScope, loggerFactory, socket, idleChannelMonitor, bufferEndianness )
    {
        Input = new ChannelPipeline(  loggerFactory, inputAdapters, inputHandlers );
        Output = new ChannelPipeline( loggerFactory, outputAdapters, new IChannelHandler[]
        {
            new OutputChannelHandler( loggerFactory )
        } );
    }

    public override Task CloseAsync()
    {
        try
        {
            Socket.Shutdown( SocketShutdown.Both );
        }
        catch ( Exception )
        {}

        try
        {
            Socket.Close();
        }
        catch ( Exception )
        {}

        return Task.CompletedTask;
    }
}
