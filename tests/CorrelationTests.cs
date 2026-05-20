using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Faactory.Channels;
using Faactory.Channels.Client;
using Faactory.Channels.Correlation;
using Faactory.Channels.Tcp;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace tests;

public class ChannelResponseRegistryTests
{
    [Fact]
    public async Task Awaiter_Completes_When_Message_Matches()
    {
        var registry = new ChannelResponseRegistry();

        var awaiter = registry.Create<string>( m => m == "ok" );

        registry.Push( "ok" );

        var result = await awaiter.WaitAsync();

        Assert.Equal( "ok", result );
    }

    [Fact]
    public async Task Awaiter_Does_Not_Complete_When_Message_Does_Not_Match()
    {
        var registry = new ChannelResponseRegistry();

        var awaiter = registry.Create<string>( m => m == "ok" );

        registry.Push( "not-ok" );

        var task = awaiter.WaitAsync();

        await Task.WhenAny( task, Task.Delay( 50 ) );

        Assert.False( task.IsCompleted );
    }

    [Fact]
    public async Task Multiple_Awaiters_All_Complete_When_Matching()
    {
        var registry = new ChannelResponseRegistry();

        var a1 = registry.Create<string>( m => m == "ok" );
        var a2 = registry.Create<string>( m => m == "ok" );

        registry.Push( "ok" );

        var r1 = await a1.WaitAsync();
        var r2 = await a2.WaitAsync();

        Assert.Equal( "ok", r1 );
        Assert.Equal( "ok", r2 );
    }

    [Fact]
    public async Task Awaiter_Throws_When_Canceled()
    {
        var registry = new ChannelResponseRegistry();

        var awaiter = registry.Create<string>( _ => true );

        using var cts = new CancellationTokenSource();

        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await awaiter.WaitAsync( cts.Token )
        );
    }

    [Fact]
    public async Task Canceled_Awaiter_Is_Not_Completed_By_Later_Push()
    {
        var registry = new ChannelResponseRegistry();
        var awaiter = registry.Create<string>( m => m == "ok" );

        using var cts = new CancellationTokenSource();

        // Start waiting
        var task = awaiter.WaitAsync( cts.Token );

        // Cancel
        cts.Cancel();

        // IMPORTANT: await the task inside the assertion
        await Assert.ThrowsAnyAsync<OperationCanceledException>( async () => await task );

        // Push after cancellation
        registry.Push( "ok" );

        Assert.True( task.IsCanceled );
    }

    [Fact]
    public async Task Different_Types_Are_Isolated()
    {
        var registry = new ChannelResponseRegistry();

        var stringAwaiter = registry.Create<string>( m => m == "ok" );
        var intAwaiter = registry.Create<int>( m => m == 42 );

        registry.Push( "ok" );
        registry.Push( 42 );

        var s = await stringAwaiter.WaitAsync();
        var i = await intAwaiter.WaitAsync();

        Assert.Equal( "ok", s );
        Assert.Equal( 42, i );
    }

    [Fact]
    public void Push_With_No_Awaiters_Does_Not_Throw()
    {
        var registry = new ChannelResponseRegistry();

        registry.Push( "nothing" );
    }

    [Fact]
    public async Task Concurrent_Awaiters_Complete_Correctly()
    {
        var registry = new ChannelResponseRegistry();

        var awaiters = Enumerable
            .Range( 0, 100 )
            .Select( _ => registry.Create<int>( m => m == 1 ) )
            .ToList();

        registry.Push( 1 );

        var results = await Task.WhenAll( awaiters.Select( a => a.WaitAsync() ) );

        Assert.All( results, r => Assert.Equal( 1, r ) );
    }

    [Fact]
    public async Task Completed_Awaiter_Is_Not_Completed_Twice()
    {
        var registry = new ChannelResponseRegistry();

        var awaiter = registry.Create<string>( m => m == "ok" );

        registry.Push( "ok" );

        var result = await awaiter.WaitAsync();

        Assert.Equal( "ok", result );

        // Push again
        registry.Push( "ok" );

        // Should not throw, not change state
        Assert.Equal( "ok", result );
    }

    [Fact]
    public async Task WaitAsync_After_Result_Returns_Immediately()
    {
        var registry = new ChannelResponseRegistry();

        var awaiter = registry.Create<string>( m => m == "ok" );

        registry.Push( "ok" );

        // WaitAsync called after completion
        var result = await awaiter.WaitAsync();

        Assert.Equal( "ok", result );
    }

    [Fact]
    public async Task Only_Matching_Awaiters_Complete()
    {
        var registry = new ChannelResponseRegistry();

        var a1 = registry.Create<int>( m => m == 1 );
        var a2 = registry.Create<int>( m => m == 2 );

        registry.Push( 1 );

        var r1 = await a1.WaitAsync();
        Assert.Equal( 1, r1 );

        Assert.False( a2.WaitAsync().IsCompleted );
    }

    [Fact]
    public async Task Canceled_Awaiter_Is_Removed_From_Registry()
    {
        var registry = new ChannelResponseRegistry();

        var awaiter = registry.Create<string>( _ => true );

        using var cts = new CancellationTokenSource();

        var task = awaiter.WaitAsync( cts.Token );

        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>( async () => await task );

        // Now push — should not throw and should not complete anything
        registry.Push( "anything" );
    }

    [Fact]
    public async Task TestServer_Handles_Correlation()
    {
        await using var server = await TestServer.StartNamedAsync( builder  =>
        {
            builder.Add( "server", channel =>
            {
                // Input: whatever arrives (byte[]) => write it back (goes through output pipeline)
                channel.AddInputHandler<byte[]>( async (ctx, data, ct) =>
                {
                    await ctx.Channel.WriteAsync( data );
                } );
            } );

            builder.Add( "client", channel =>
            {
                channel.AddCorrelation();

                channel.AddInputHandler<byte[]>( (ctx, data) =>
                {
                    ctx.Channel.GetChannelResponseRegistry().Push( data );
                } );
            } );
        },
        services =>
        {
            services.AddTcpChannelListener( "server", options =>
            {
                options.Port = 0; // dynamic
                options.Backlog = 1;
            } );

        } );

        var factory = server.Services.GetRequiredService<IChannelFactory>();

        using var client = new TcpClient(
            factory.ChannelServices.CreateScope(),
            new ChannelsClientOptions
            {
                Host = IPAddress.Loopback.ToString(),
                Port = server.Port
            },
            "client" );

        await Task.Delay( 500 ); // give the server a moment to start and bind to the dynamic port

        var registry = client.Channel.GetChannelResponseRegistry();

        var awaiter = registry.Create<byte[]>( m => m.SequenceEqual( Encoding.ASCII.GetBytes( "TEST" ) ) );

        await client.Channel.WriteAsync( Encoding.ASCII.GetBytes( "TEST" ) );

        var response = await awaiter.WaitAsync();

        Assert.Equal( Encoding.ASCII.GetBytes( "TEST" ), response );

        await client.CloseAsync();
    }
}
