using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal class ClientChannel : TcpChannel
{
    public ClientChannel( IServiceScope serviceScope
        , Socket socket
        , Buffers.Endianness bufferEndianness
        , IEnumerable<IChannelService>? channelServices = null )
        : base( 
              serviceScope
            , socket
            , bufferEndianness )
    {
        var pipelineFactory = new ChannelPipelineFactory( serviceScope.ServiceProvider );

        Input = pipelineFactory.CreateInputPipeline();
        Output = pipelineFactory.CreateOutputPipeline();

        if ( channelServices is not null )
        {
            Services = channelServices;
        }
    }

    public override async Task CloseAsync()
    {
        Shutdown();

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
