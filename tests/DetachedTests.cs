using System.Threading;
using System.Threading.Tasks;
using Faactory.Channels.Adapters;
using Xunit;

namespace Faactory.Channels.Tests;

public class DetachedChannelTests
{
    [Fact]
    public async Task TestChannelServiceAsync()
    {
        var pipeline = new DetachedPipeline()
            .AddAdapter( new MyAdapter() )
            .AddChannelService( new MyChannelService() );

        await pipeline.RunAsync( [] );

        var id = Assert.Single( pipeline.Context.Forwarded );
        Assert.Equal( "my-channel-service", id );
    }

    private class MyAdapter : IChannelAdapter, IInputChannelAdapter
    {
        public Task ExecuteAsync( IAdapterContext context, object data )
        {
            var svc = context.Channel.GetRequiredService<MyChannelService>();

            context.Forward( svc.Id );

            return Task.CompletedTask;
        }
    }

    private class MyChannelService : IChannelService
    {
        public string Id { get; } = "my-channel-service";

        public void Dispose()
        { }

        public Task StartAsync( IChannel channel, CancellationToken cancellationToken )
        {
            return Task.CompletedTask;
        }

        public Task StopAsync( CancellationToken cancellationToken )
        {
            return Task.CompletedTask;
        }
    }
}
