using System.Net.Sockets;
using Faactory.Channels.Buffers;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal sealed class ServiceChannel : Channel
{
    public ServiceChannel( IServiceScope serviceScope
        , Socket socket
        , Endianness bufferEndianness )
        : base( 
              serviceScope
            , socket
            , bufferEndianness )
    {
        var pipelineFactory = new ChannelPipelineFactory( serviceScope.ServiceProvider );

        Input = pipelineFactory.CreateInputPipeline();
        Output = pipelineFactory.CreateOutputPipeline();
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
