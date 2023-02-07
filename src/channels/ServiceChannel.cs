using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Faactory.Channels.Buffers;
using Faactory.Channels;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
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
