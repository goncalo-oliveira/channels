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
