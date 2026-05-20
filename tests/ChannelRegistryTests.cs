using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Faactory.Channels;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace tests;

 public class ChannelRegistryTests
 {
    [Fact]
    public async Task ChannelRegistry_EchoTest()
    {
        await using var server = await TestServer.StartAsync( channel  =>
        {
            // Input: whatever arrives (byte[]) => write it back (goes through output pipeline)
            channel.AddInputHandler<byte[]>( async (ctx, data, ct) =>
            {
                await ctx.Channel.WriteAsync( data );
            } );
        } );

        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync( IPAddress.Loopback, server.Port );

        await Task.Delay( 200 ); // give some time for the server to accept the connection

        var registry = server.Services.GetRequiredService<IChannelRegistry>();

        var handle = Assert.Single( registry.Channels );

        var stream = client.GetStream();

        var payload = Encoding.ASCII.GetBytes( "TEST" );

        await stream.WriteAsync( payload );
        await stream.FlushAsync();

        await Task.Delay( 100 ); // give some time for the server to process the write

        Assert.Equal( payload.Length, handle.BytesSent );

        // read exactly payload length (simple echo)
        var received = new byte[payload.Length];
        int read = 0;

        while ( read < received.Length )
        {
            int n = await stream.ReadAsync( received.AsMemory( read, received.Length - read ) );

            Assert.True( n > 0, "socket closed before receiving echo" );

            read += n;
        }

        Assert.Equal( payload, received );
        Assert.Equal( received.Length, handle.BytesReceived );

        client.Close();

        await Task.Delay( 100 ); // give some time for the server to process the close

        Assert.Empty( registry.Channels );
    }    
 }
