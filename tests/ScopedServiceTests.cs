using System;
using System.Collections;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Faactory.Channels;
using Faactory.Channels.Adapters;
using Faactory.Channels.Buffers;
using Faactory.Channels.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class ScopedServiceTests
{
    private class MyService : ChannelService
    {
        public string Id { get; } = Guid.NewGuid().ToString( "N" );
        public string Status { get; private set; } = "unknown";

        public override async Task StartAsync( IChannel channel, CancellationToken cancellationToken )
        {
            await base.StartAsync( channel, cancellationToken );

            if ( !cancellationToken.IsCancellationRequested )
            {
                Status = "started";
            }
        }

        public override async Task StopAsync( CancellationToken cancellationToken )
        {
            await base.StopAsync( cancellationToken );

            Status = "stopped";
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while ( !cancellationToken.IsCancellationRequested )
            {
                await Task.Delay( 1000 );
            }
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
        IServiceCollection services = new ServiceCollection()
            .AddLogging()
            .AddTransient<IServiceChannelFactory, ServiceChannelFactory>()
            .AddTransient<IInputChannelAdapter, MyAdapter>()
            .AddScoped<MyService>();

        var provider = services.BuildServiceProvider();

        var channelFactory = provider.GetRequiredService<IServiceChannelFactory>();

        var channel1 = channelFactory.CreateChannel( new Socket( SocketType.Stream, ProtocolType.Tcp ) );
        var channel2 = channelFactory.CreateChannel( new Socket( SocketType.Stream, ProtocolType.Tcp ) );

        var adapter1 = (MyAdapter)((Channel)channel1).ServiceProvider.GetServices<IInputChannelAdapter>().Single();
        var adapter2 = (MyAdapter)((Channel)channel1).ServiceProvider.GetServices<IInputChannelAdapter>().Single();

        var id1 = adapter1.Id;
        var id2 = adapter2.Id;

        // both ids have to match, since MyService is scoped
        Assert.Equal( id1, id2 );

        adapter1 = (MyAdapter)((Channel)channel2).ServiceProvider.GetServices<IInputChannelAdapter>().Single();
        adapter2 = (MyAdapter)((Channel)channel2).ServiceProvider.GetServices<IInputChannelAdapter>().Single();

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
        IServiceCollection services = new ServiceCollection()
            .AddLogging()
            .AddTransient<IServiceChannelFactory, ServiceChannelFactory>()
            .AddScoped<IChannelService, MyService>();

        var provider = services.BuildServiceProvider();

        var channelFactory = provider.GetRequiredService<IServiceChannelFactory>();

        var channel1 = channelFactory.CreateChannel( new Socket( SocketType.Stream, ProtocolType.Tcp ) );
        var channel2 = channelFactory.CreateChannel( new Socket( SocketType.Stream, ProtocolType.Tcp ) );

        var svc1 = (MyService)((Channel)channel1).ServiceProvider.GetServices<IChannelService>().Single();
        var svc2 = (MyService)((Channel)channel2).ServiceProvider.GetServices<IChannelService>().Single();

        // ids shouldn't match, since they come from two different channels (different scopes)
        Assert.NotEqual( svc1.Id, svc2.Id );

        var svc1Copy = (MyService)((Channel)channel1).ServiceProvider.GetServices<IChannelService>().Single();
        var svc2Copy = (MyService)((Channel)channel2).ServiceProvider.GetServices<IChannelService>().Single();

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
