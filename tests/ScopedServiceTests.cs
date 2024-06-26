using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Faactory.Channels;
using Faactory.Channels.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Faactory.Channels.Tests;

public class ScopedServiceTests
{
    private class MyService : IChannelService
    {
        public string Id { get; } = Guid.NewGuid().ToString( "N" );
        public string Status { get; private set; } = "unknown";

        public void Dispose()
        { }

        public Task StartAsync( IChannel channel, CancellationToken cancellationToken )
        {
            Status = "started";

            return Task.CompletedTask;
        }

        public Task StopAsync( CancellationToken cancellationToken )
        {
            Status = "stopped";

            return Task.CompletedTask;
        }
    }

    private class MyAdapter : ChannelAdapter<string>, IInputChannelAdapter
    {
        private readonly string id;

        public MyAdapter( MyService service )
        {
            id = service.Id;
        }

        public string Id => id;

        public override Task ExecuteAsync( IAdapterContext context, string data )
        {
            context.Forward( id );

            return Task.CompletedTask;
        }
    }

    [Fact]
    public void TestScopedService()
    {
        var channelName = "__tests";
        IServiceCollection services = new ServiceCollection()
            .AddLogging()
            .AddKeyedTransient<IInputChannelAdapter, MyAdapter>( channelName )
            .AddScoped<MyService>();

        var provider = services.BuildServiceProvider();

        TcpChannel channelFactory()
        {
            return new TcpChannel(
                provider.CreateScope(),
                new Socket(SocketType.Stream, ProtocolType.Tcp),
                new ChannelOptions(),
                EmptyChannelPipeline.Instance,
                EmptyChannelPipeline.Instance,
                null
            );
        }

        var channel1 = channelFactory();
        var channel2 = channelFactory();

        var adapter1 = (MyAdapter)((TcpChannel)channel1).ServiceProvider.GetAdapters<IInputChannelAdapter>( channelName ).Single();
        var adapter2 = (MyAdapter)((TcpChannel)channel1).ServiceProvider.GetAdapters<IInputChannelAdapter>( channelName ).Single();

        var id1 = adapter1.Id;
        var id2 = adapter2.Id;

        // both ids have to match, since MyService is scoped
        Assert.Equal( id1, id2 );

        adapter1 = (MyAdapter)((TcpChannel)channel2).ServiceProvider.GetAdapters<IInputChannelAdapter>( channelName ).Single();
        adapter2 = (MyAdapter)((TcpChannel)channel2).ServiceProvider.GetAdapters<IInputChannelAdapter>( channelName ).Single();

        var id3 = adapter1.Id;
        var id4 = adapter2.Id;

        // both ids have to match, since MyService is scoped
        Assert.Equal( id3, id4 );

        // ids can't match, since they come from two different instances (different scopes)
        Assert.NotEqual( id1, id3 );
    }

    [Fact]
    public async Task TestChannelServices()
    {
        var channelName = "__tests";
        IServiceCollection services = new ServiceCollection()
            .AddLogging()
            .AddKeyedScoped<IChannelService, MyService>( channelName );

        var provider = services.BuildServiceProvider();

        TcpChannel channelFactory()
        {
            var scope = provider.CreateScope();

            return new TcpChannel(
                scope,
                new Socket( SocketType.Stream, ProtocolType.Tcp ),
                new ChannelOptions(),
                EmptyChannelPipeline.Instance,
                EmptyChannelPipeline.Instance,
                scope.ServiceProvider.GetKeyedServices<IChannelService>( channelName )
            );
        }

        var channel1 = channelFactory();
        var channel2 = channelFactory();

        var svc1 = channel1.GetService<MyService>();
        var svc2 = channel2.GetService<MyService>();

        Assert.NotNull( svc1 );
        Assert.NotNull( svc2 );

        // ids shouldn't match, since they come from two different channels (different scopes)
        Assert.NotEqual( svc1.Id, svc2.Id );

        var svc1Copy = channel1.GetService<MyService>();
        var svc2Copy = channel2.GetService<MyService>();

        Assert.NotNull( svc1Copy );
        Assert.NotNull( svc2Copy );

        // ids should match, since service is scoped to channel
        Assert.Equal( svc1.Id, svc1Copy.Id );
        Assert.Equal( svc2.Id, svc2Copy.Id );

        // both services should have "started"
        Assert.Equal( "started", svc1.Status );
        Assert.Equal( "started", svc2.Status );

        await channel1.CloseAsync();
        await channel2.CloseAsync();

        // both services should have "stopped"
        Assert.Equal( "stopped", svc1.Status );
        Assert.Equal( "stopped", svc2.Status );
    }
}
