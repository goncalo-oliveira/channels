using System.Net.Sockets;
using Faactory.Channels.Buffers;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal sealed class ServiceChannel : TcpChannel
{
    public ServiceChannel( IServiceScope serviceScope
        , Socket socket
        , Endianness bufferEndianness
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

    public override Task CloseAsync()
    {
        try
        {
            Shutdown();
        }
        catch ( Exception )
        { }
        finally
        {
            OnDisconnected();
        }

        try
        {
            Socket.Close();
        }
        catch ( Exception )
        {}

        return Task.CompletedTask;
    }
}
