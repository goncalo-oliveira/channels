using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Faactory.Channels;
using Xunit;

namespace tests;

public class ChannelLimiterTests
{
    [Fact]
    public void ChannelLimiter_Should_Limit_Connections()
    {
        var limiter = new ChannelLimiter( 2 );

        var channel1 = new DetachedChannel();
        var channel2 = new DetachedChannel();
        var channel3 = new DetachedChannel();

        limiter.ChannelCreated( new ChannelInfo( channel1 ) );
        limiter.ChannelCreated( new ChannelInfo( channel2 ) );
        limiter.ChannelCreated( new ChannelInfo( channel3 ) );

        Assert.True( limiter.IsAdmitted( channel1 ) );
        Assert.True( limiter.IsAdmitted( channel2 ) );
        Assert.False( limiter.IsAdmitted( channel3 ) );
    }

    [Fact]
    public void ChannelLimiter_Should_Release_Connections_On_Close()
    {
        var limiter = new ChannelLimiter( 2 );

        var channel1 = new DetachedChannel();
        var channel2 = new DetachedChannel();
        var channel3 = new DetachedChannel();

        limiter.ChannelCreated( new ChannelInfo( channel1 ) );
        limiter.ChannelCreated( new ChannelInfo( channel2 ) );

        limiter.ChannelClosed( new ChannelInfo( channel1 ) );

        limiter.ChannelCreated( new ChannelInfo( channel3 ) );

        Assert.True( limiter.IsAdmitted( channel3 ) );
    }

    [Fact]
    public void ChannelLimiter_Should_Reject_Unknown_Channel()
    {
        var limiter = new ChannelLimiter( 1 );

        var channel = new DetachedChannel();

        Assert.False( limiter.IsAdmitted( channel ) );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    public void ChannelLimiter_Should_Disable_Limit_When_Less_Than_Or_Equal_To_Zero( int limit )
    {
        var limiter = new ChannelLimiter( limit );

        var channels = Enumerable.Range( 0, 100 )
            .Select( _ => new DetachedChannel() )
            .ToArray();

        foreach ( var channel in channels )
        {
            limiter.ChannelCreated( new ChannelInfo( channel ) );

            Assert.True( limiter.IsAdmitted( channel ) );
        }
    }

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
