using System;
using System.Collections;
using System.Linq;
using System.Net.Sockets;
using System.Text;
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
    private class MyService
    {
        public string Id { get; } = Guid.NewGuid().ToString( "N" );
    }

    private class MyAdapter : ChannelAdapter<string>
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
}
