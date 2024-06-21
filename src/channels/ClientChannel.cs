using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

internal sealed class ClientChannel : TcpChannel
{
    private readonly Task initializeTask;
    private readonly CancellationTokenSource cts = new();

    public ClientChannel( IServiceScope serviceScope
        , Socket socket
        , Buffers.Endianness bufferEndianness
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

    public override async Task CloseAsync()
    {
        try
        {
            cts.Cancel();
        }
        catch { }

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

    public override void Dispose()
    {
        base.Dispose();

        initializeTask.Dispose();
    }
}
