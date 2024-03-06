using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Faactory.Channels.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Faactory.Channels.Tests;

public class CustomEventTests
{
    private class MyAdapter : ChannelAdapter<string>, IInputChannelAdapter
    {
        public override Task ExecuteAsync( IAdapterContext context, string data )
        {
            context.Forward( data );

            // can't notify here because IChannel is fake...... need an interface for notifications??? how???
            context.NotifyCustomEvent( "custom-event", data );

            return Task.CompletedTask;
        }
    }

    private class MyEvents : IChannelMonitor
    {
        public void ChannelClosed(IChannelInfo channelInfo)
        { }

        public void ChannelCreated(IChannelInfo channelInfo)
        { }

        public void DataReceived(IChannelInfo channelInfo, byte[] data)
        { }

        public void DataSent(IChannelInfo channelInfo, int sent)
        { }

        public void CustomEvent( IChannelInfo channelInfo, string name, object? data )
        {
            CustomEvents.Add( new KeyValuePair<string, object?>( name, data ) );
        }

        public List<KeyValuePair<string, object?>> CustomEvents = new List<KeyValuePair<string, object?>>();
    }

    [Fact]
    public async Task TestCustomEventAsync()
    {
        IServiceCollection services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IChannelMonitor, MyEvents>();

        var provider = services.BuildServiceProvider();

        var pipeline = new ChannelPipelineFactory( provider )
            .CreatePipeline(
            [
                new MyAdapter()
            ] );

        var options = new Microsoft.Extensions.Options.OptionsWrapper<ServiceChannelOptions>( new ServiceChannelOptions() );
        var channel = await new ServiceChannelFactory( provider, options )
            .CreateChannelAsync( new Socket( SocketType.Stream, ProtocolType.Tcp ) );

        var data = Guid.NewGuid().ToString( "N" );
        await pipeline.ExecuteAsync( channel, data );

        var events = provider.GetServices<IChannelMonitor>()
            .Where( x => x.GetType() == typeof( MyEvents ) )
            .Cast<MyEvents>()
            .Single();

        var ev = Assert.Single<KeyValuePair<string, object?>>( events.CustomEvents );

        Assert.Equal( "custom-event", ev.Key );
        Assert.Equal( data, ev.Value );
    }
}
