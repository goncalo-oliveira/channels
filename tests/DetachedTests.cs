using System.Threading;
using System.Threading.Tasks;
using Faactory.Channels;
using Faactory.Channels.Adapters;
using Xunit;

namespace tests;

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

    [Fact]
    public void ChannelInfo_Data_Reflects_Live_Changes()
    {
        // Arrange
        var channel = new DetachedChannel();
        channel.Data["device"] = "A";

        var info = new ChannelInfo( channel );

        // Act + Assert (initial state)
        Assert.Equal( "A", info.Data["device"] );

        // Mutate original dictionary
        channel.Data["device"] = "B";

        // Assert (should reflect updated value)
        Assert.Equal( "B", info.Data["device"] );

        // Add new key after ChannelInfo creation
        channel.Data["tenant"] = "T1";

        Assert.True( info.Data.ContainsKey( "tenant" ) );
        Assert.Equal( "T1", info.Data["tenant"] );
    }

    private class MyAdapter : IChannelAdapter, IInputChannelAdapter
    {
        public Task ExecuteAsync( IAdapterContext context, object data, CancellationToken cancellationToken )
        {
            var svc = context.Channel.GetRequiredChannelService<MyChannelService>();

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
