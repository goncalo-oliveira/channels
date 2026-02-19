using System;
using System.Threading;
using System.Threading.Tasks;
using Faactory.Channels;
using Faactory.Channels.Handlers;
using Xunit;

namespace tests;

public class TcpServerTests
{
    [Fact]
    public async Task TcpServer_Receives_Bytes()
    {
        var receivedTcs = new TaskCompletionSource<byte[]>( TaskCreationOptions.RunContinuationsAsynchronously );

        await using var server = await TestServer.CreateAsync(
            configureChannel: channel =>
            {
                channel.AddInputHandler( sp => new CaptureHandler( receivedTcs ) );
            } );

        using var client = new System.Net.Sockets.TcpClient();

        await client.ConnectAsync( "127.0.0.1", server.Port );

        var stream = client.GetStream();

        var payload = new byte[] { 1, 2, 3, 4 };

        await stream.WriteAsync( payload );
        await stream.FlushAsync();

        var received = await receivedTcs.Task.WaitAsync( TimeSpan.FromSeconds( 5 ) );

        Assert.True( received.AsSpan().SequenceEqual( payload ) );
    }

    internal sealed class CaptureHandler( TaskCompletionSource<byte[]> tcs ) : ChannelHandler<byte[]>
    {
        public override Task ExecuteAsync( IChannelContext context, byte[] data, CancellationToken cancellationToken )
        {
            tcs.TrySetResult( data );

            return Task.CompletedTask;
        }
    }
}
