using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Faactory.Channels;
using Xunit;

namespace tests;

public class ChannelLimiterTests
{
    [Fact]
    public async Task TcpServer_Should_Reject_Connections_Beyond_Limit()
    {
        await using var server = await TestServer.StartAsync(
            configureChannel: channel =>
            {
                channel.AddInputHandler<byte[]>( ( ctx, data ) =>
                {
                    ctx.Output.Write( data );
                } );

                channel.AddConnectionLimiter( options =>
                {
                    options.ConnectionLimit = 1;
                } );
            }
        );

        using var client1 = new TcpClient();
        await client1.ConnectAsync( "127.0.0.1", server.Port );

        await Task.Delay( 100 );

        using var client2 = new TcpClient();
        await client2.ConnectAsync( "127.0.0.1", server.Port );

        // give lifecycle a tiny moment
        await Task.Delay( 100 );

        client1.GetStream().WriteByte( 0 );
        client2.GetStream().WriteByte( 0 );

        // second should have been closed
        var buffer = new byte[1];

        var read = await client1.GetStream()
            .ReadAsync( buffer );

        Assert.Equal( 1, read );

        await Assert.ThrowsAnyAsync<Exception>( async () =>
        {
            await client2.GetStream()
                .ReadExactlyAsync( buffer );
        } );
    }
}
