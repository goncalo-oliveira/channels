using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Faactory.Channels;
using Faactory.Channels.Adapters;
using Faactory.Channels.Buffers;
using Faactory.Channels.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace tests;

public class TcpServerTests
{
    [Fact]
    public async Task TcpServer_Receives_Bytes()
    {
        var receivedTcs = new TaskCompletionSource<byte[]>( TaskCreationOptions.RunContinuationsAsynchronously );

        await using var server = await TestServer.StartAsync(
            configureChannel: channel =>
            {
                channel.AddInputHandler( sp => new CaptureHandler<byte[]>( receivedTcs ) );
            }
        );

        using var client = new System.Net.Sockets.TcpClient();

        await client.ConnectAsync( "127.0.0.1", server.Port );

        var stream = client.GetStream();

        var payload = new byte[] { 1, 2, 3, 4 };

        await stream.WriteAsync( payload );
        await stream.FlushAsync();

        var received = await receivedTcs.Task.WaitAsync( TimeSpan.FromSeconds( 5 ) );

        Assert.True( received.AsSpan().SequenceEqual( payload ) );
    }

    [Fact]
    public async Task TcpServer_Receives_ReadableBuffer()
    {
        var receivedTcs = new TaskCompletionSource<IReadableByteBuffer>( TaskCreationOptions.RunContinuationsAsynchronously );

        await using var server = await TestServer.StartAsync(
            configureChannel: channel =>
            {
                channel.AddInputHandler( sp => new CaptureHandler<IReadableByteBuffer>( receivedTcs ) );
            }
        );

        using var client = new System.Net.Sockets.TcpClient();

        await client.ConnectAsync( "127.0.0.1", server.Port );

        var stream = client.GetStream();

        var payload = new byte[] { 1, 2, 3, 4 };

        await stream.WriteAsync( payload );
        await stream.FlushAsync();

        var received = await receivedTcs.Task.WaitAsync( TimeSpan.FromSeconds( 5 ) );

        Assert.True( received.AsSpan().SequenceEqual( payload ) );
    }

    [Fact]
    public async Task TcpServer_Echoes_Bytes_RoundTrip()
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

        var stream = client.GetStream();

        var payload = Encoding.ASCII.GetBytes( "TEST" );

        await stream.WriteAsync( payload );

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
    }

    [Fact]
    public async Task TcpServer_Echoes_Multiple_Messages_On_Same_Connection()
    {
        await using var server = await TestServer.StartAsync( channel  =>
        {
            channel.AddInputHandler<byte[]>( async (ctx, data, ct) =>
            {
                await ctx.Channel.WriteAsync( data );
            } );
        } );

        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync( IPAddress.Loopback, server.Port );

        var stream = client.GetStream();

        for ( int i = 0; i < 10; i++ )
        {
            var payload = Encoding.ASCII.GetBytes( $"MSG{i}" );
            await stream.WriteAsync( payload );

            var received = new byte[payload.Length];
            int read = 0;

            while ( read < received.Length )
            {
                int n = await stream.ReadAsync( received.AsMemory( read, received.Length - read ) );

                Assert.True( n > 0, "socket closed before receiving echo" );

                read += n;
            }

            Assert.Equal( payload, received );
        }
    }

    [Fact]
    public async Task TcpServer_Echoes_Large_Payload()
    {
        await using var server = await TestServer.StartAsync( channel =>
        {
            channel.AddInputHandler<byte[]>(async (ctx, data, ct) =>
            {
                await ctx.Channel.WriteAsync( data );
            } );
        } );

        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync( IPAddress.Loopback, server.Port );

        var stream = client.GetStream();

        var payload = new byte[200_000]; // > default buffer sizes
        Random.Shared.NextBytes( payload );

        await stream.WriteAsync( payload );

        var received = new byte[payload.Length];
        int read = 0;

        while ( read < received.Length )
        {
            int n = await stream.ReadAsync( received.AsMemory( read, received.Length - read ) );

            Assert.True( n > 0 , "socket closed before receiving echo" );

            read += n;
        }

        Assert.Equal( payload, received );
    }

    [Fact]
    public async Task TcpServer_Handles_Multiple_Clients()
    {
        await using var server = await TestServer.StartAsync( channel =>
        {
            channel.AddInputHandler<byte[]>(async (ctx, data, ct) =>
            {
                await ctx.Channel.WriteAsync( data );
            } );
        } );

        const int clientCount = 10;

        var tasks = Enumerable.Range( 0, clientCount ).Select( async i =>
        {
            using var client = new System.Net.Sockets.TcpClient();
            await client.ConnectAsync( IPAddress.Loopback, server.Port );

            var stream = client.GetStream();

            var payload = Encoding.ASCII.GetBytes($"CLIENT-{i}");
            await stream.WriteAsync( payload );

            var received = new byte[payload.Length];
            int read = 0;

            while ( read < received.Length )
            {
                int n = await stream.ReadAsync( received.AsMemory( read, received.Length - read ) );

                Assert.True( n > 0 , $"socket closed before receiving echo for client {i}" );

                read += n;
            }

            Assert.Equal( payload, received );
        });

        await Task.WhenAll( tasks );
    }

    [Fact]
    public async Task TcpServer_Handles_Client_Disconnect()
    {
        var monitor = new TestChannelMonitor();

        await using var server = await TestServer.StartAsync(
            configureServices: services =>
            {
                services.AddSingleton<IChannelMonitor, TestChannelMonitor>( _ => monitor );
            }
        );

        using ( var client = new System.Net.Sockets.TcpClient() )
        {
            await client.ConnectAsync( IPAddress.Loopback, server.Port );
        }

        // Give server a moment to process disconnect
        await Task.Delay( 200 );

        // If we reached here without crashing, test passes.
        // Also verify that the monitor recorded the connection and disconnection events.
        Assert.Equal( 1, monitor.Connected );
        Assert.Equal( 1, monitor.Disconnected );
    }

    [Fact]
    public async Task Pipeline_Adapter_Transforms_And_Handler_Receives_Typed_Message()
    {
        var tcs = new TaskCompletionSource<Word>( TaskCreationOptions.RunContinuationsAsynchronously );

        await using var server = await TestServer.StartAsync(
            configureChannel: channel =>
            {
                channel
                    .AddInputAdapter<IReadableByteBuffer>( (ctx,data) =>
                    {
                        var text = Encoding.ASCII.GetString( data.AsSpan() );

                        ctx.Forward( new Word( text ) );
                    })
                    .AddInputHandler( sp => new CaptureHandler<Word>( tcs ) );
            }
        );

        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync( IPAddress.Loopback, server.Port );

        var payload = Encoding.ASCII.GetBytes( "HELLO" );
        await client.GetStream().WriteAsync( payload );

        var received = await tcs.Task.WaitAsync( TimeSpan.FromSeconds( 2 ) );

        Assert.Equal( "HELLO", received.Value );
    }

    [Fact]
    public async Task Pipeline_Stops_When_Adapter_Does_Not_Forward()
    {
        var handlerCalled = false;

        await using var server = await TestServer.StartAsync(
            configureChannel: channel =>
            {
                channel
                    .AddInputAdapter<IReadableByteBuffer>( (ctx, data ) =>
                    {
                        // Adapter receives data but does not forward, so handler should not be called
                    } )
                    .AddInputHandler<IReadableByteBuffer>( (ctx, data) =>
                    {
                        handlerCalled = true;
                    } );
            }
        );

        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync( IPAddress.Loopback, server.Port );

        await client.GetStream().WriteAsync( Encoding.ASCII.GetBytes( "TEST" ) );

        await Task.Delay( 200 );

        Assert.False( handlerCalled );
    }

    [Fact]
    public async Task Handler_Throws_Channel_Does_Not_Crash_Process()
    {
        var monitor = new TestChannelMonitor();

        await using var server = await TestServer.StartAsync(
            configureChannel: channel =>
            {
                channel.AddInputHandler<byte[]>( (ctx, data) =>
                {
                    throw new InvalidOperationException( "boom" );
                } );
            },
            configureServices: services =>
            {
                services.AddSingleton<IChannelMonitor>( monitor );
            }
        );

        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync( IPAddress.Loopback, server.Port );

        await client.GetStream().WriteAsync( new byte[] { 1, 2, 3 } );

        await Task.Delay( 300 );

        Assert.Equal( 1, monitor.Connected );
        Assert.Equal( 1, monitor.Disconnected );
    }

    [Fact]
    public async Task OutputPipeline_Transforms_Typed_Message_To_Bytes()
    {
        await using var server = await TestServer.StartAsync(
            configureChannel: channel =>
            {
                channel
                    .AddOutputAdapter<Word>( (ctx, data ) =>
                    {
                        var bytes = Encoding.ASCII.GetBytes( data.Value );

                        ctx.Forward( bytes );
                    })
                    .AddInputHandler<byte[]>( async (ctx, data) =>
                    {
                        await ctx.Channel.WriteAsync( new Word( "HELLO" ) );
                    } );
            }
        );

        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync( IPAddress.Loopback, server.Port );

        await client.GetStream().WriteAsync(new byte[] { 0x01 });

        var buffer = new byte[5];
        int read = 0;

        while ( read < buffer.Length )
        {
            read += await client.GetStream()
                .ReadAsync( buffer.AsMemory( read, buffer.Length - read ) );
        }

        Assert.Equal( "HELLO", Encoding.ASCII.GetString( buffer ) );
    }

    [Fact]
    public async Task OutputPipeline_Transforms_Typed_Message_To_Buffer()
    {
        await using var server = await TestServer.StartAsync(
            configureChannel: channel =>
            {
                channel
                    .AddOutputAdapter<Word>( (ctx, data ) =>
                    {
                        var buffer = new WritableByteBuffer();

                        buffer.WriteBytes( Encoding.ASCII.GetBytes( data.Value ) );

                        // forwarding a writable buffer should work as well, and the pipeline should write it to the channel
                        ctx.Forward( buffer );
                    })
                    .AddInputHandler<byte[]>( async (ctx, data) =>
                    {
                        await ctx.Channel.WriteAsync( new Word( "HELLO" ) );
                    } );
            }
        );

        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync( IPAddress.Loopback, server.Port );

        await client.GetStream().WriteAsync(new byte[] { 0x01 });

        var buffer = new byte[5];
        int read = 0;

        while ( read < buffer.Length )
        {
            read += await client.GetStream()
                .ReadAsync( buffer.AsMemory( read, buffer.Length - read ) );
        }

        Assert.Equal( "HELLO", Encoding.ASCII.GetString( buffer ) );
    }

    [Fact]
    public async Task TcpServer_Frames_Buffer_Until_Complete_Packet()
    {
        var tcs = new TaskCompletionSource<IReadableByteBuffer>();
        var expected = Encoding.ASCII.GetBytes( "HELLO" );

        await using var server = await TestServer.StartAsync(
            configureChannel: channel =>
            {
                channel
                    .AddInputAdapter<IReadableByteBuffer>( (ctx, buffer) =>
                    {
                        // Not enough bytes yet... partial packet received
                        if ( buffer.ReadableBytes < expected.Length )
                        {
                            return;
                        }

                        ctx.Forward( buffer );
                    } )
                    .AddInputHandler<IReadableByteBuffer>( (ctx, buffer) =>
                    {
                        tcs.TrySetResult( buffer );
                    } );
            }
        );

        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync( IPAddress.Loopback, server.Port );

        var stream = client.GetStream();

        // Send partial
        await stream.WriteAsync( Encoding.ASCII.GetBytes( "HE" ) );
        await Task.Delay( 50 );

        Assert.False( tcs.Task.IsCompleted );

        // Send rest
        await stream.WriteAsync( Encoding.ASCII.GetBytes( "LLO" ) );

        var result = await tcs.Task.WaitAsync( TimeSpan.FromSeconds( 2 ) );

        Assert.Equal( "HELLO", Encoding.ASCII.GetString( result.AsSpan() ) );
    }

    private sealed record Word( string Value );

    private sealed class TestChannelMonitor : IChannelMonitor
    {
        public int Connected;
        public int Disconnected;

        public void ChannelClosed( IChannelInfo channelInfo )
        {
            Interlocked.Increment( ref Disconnected );
        }

        public void ChannelCreated( IChannelInfo channelInfo )
        {
            Interlocked.Increment( ref Connected );
        }

        public void CustomEvent( IChannelInfo channelInfo, string name, object? data )
        { }

        public void DataReceived( IChannelInfo channelInfo, byte[] data )
        { }

        public void DataSent( IChannelInfo channelInfo, byte[] data )
        { }
    }
}
