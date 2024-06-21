using System.Net.Sockets;
using Faactory.Channels.Buffers;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal sealed class ServiceChannel : TcpChannel
{
    private readonly Task initializeTask;
    private readonly CancellationTokenSource cts = new();

    public ServiceChannel( IServiceScope serviceScope
        , Socket socket
        , Endianness bufferEndianness
        , IChannelPipeline inputPipeline
        , IChannelPipeline outputPipeline
        , IEnumerable<IChannelService>? channelServices = null )
        : base( 
              serviceScope
            , socket
            , bufferEndianness )
    {
        Input = inputPipeline;
        Output = outputPipeline;

        if ( channelServices != null )
        {
            Services = channelServices;
        }

        initializeTask = InitializeAsync( cts.Token );
    }

    public override Task CloseAsync()
    {
        try
        {
            cts.Cancel();
        }
        catch { }

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

    public override void Dispose()
    {
        base.Dispose();

        initializeTask.Dispose();
    }
}
