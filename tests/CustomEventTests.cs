using System;
using System.Collections;
using System.Collections.Generic;
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

    private class MyEvents : IChannelEvents
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
            .AddSingleton<IChannelEvents, MyEvents>();

        var provider = services.BuildServiceProvider();

        var pipeline = new ChannelPipelineFactory( provider )
            .CreatePipeline( new IChannelAdapter[]
            {
                new MyAdapter()
            } );

        var options = new Microsoft.Extensions.Options.OptionsWrapper<ServiceChannelOptions>( new ServiceChannelOptions() );
        var channel = new ServiceChannelFactory( provider, options )
            .CreateChannel( new Socket( SocketType.Stream, ProtocolType.Tcp ) );

        var data = Guid.NewGuid().ToString( "N" );
        await pipeline.ExecuteAsync( channel, data );

        var events = provider.GetServices<IChannelEvents>()
            .Where( x => x.GetType() == typeof( MyEvents ) )
            .Cast<MyEvents>()
            .Single();

        var ev = Assert.Single<KeyValuePair<string, object?>>( events.CustomEvents );

        Assert.Equal( ev.Key, "custom-event" );
        Assert.Equal( ev.Value, data );
    }
}
