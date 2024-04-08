using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Faactory.Channels.Adapters;
using Faactory.Channels.Handlers;
using Xunit;

namespace Faactory.Channels.Tests;

public class ChannelMiddlewareTests
{
    [Fact]
    public async Task TestTypeCheckAsync()
    {
        var context = new DetachedContext();
        var handler = new IdentityHandler();

        await handler.ExecuteAsync( context, new object() );

        Assert.True( handler.notSuitable );
    }

    [Fact]
    public async Task TestSequenceOrderAsync()
    {
        var data = new string[ 100 ];

        for ( int i = 0; i < data.Length; i++ )
        {
            data[ i ] = ( i + 1 ).ToString();
        }

        var context = new DetachedContext();
        var adapter = new NullAdapter();

        /*
        Since recent changes, sparsed data is no longer spawned in parallel tasks.
        Instead, the data is aggregated and forwarded in the same order as received.
        */

        await adapter.ExecuteAsync( context, data );

        // context.Forwarded should have 10 elements
        // the order should be the same as the input data
        Assert.Equal( data.Length, context.Forwarded.Length );

        for ( int i = 0; i < data.Length; i++ )
        {
            Assert.Equal( data[ i ], context.Forwarded[ i ] );
        }
    }

    private class IdentityHandler : ChannelHandler<DeviceIdentity>
    {
        internal bool notSuitable;

        public override Task ExecuteAsync( IChannelContext context, DeviceIdentity data )
        {
            var value = data.ToString();

            if ( value == null )
            {
                throw new NullReferenceException( "The identity data is null!!!! Type check failed!" );
            }

            return Task.CompletedTask;
        }

        protected override void OnDataNotSuitable( IChannelContext context, object data )
        {
            base.OnDataNotSuitable( context, data );

            notSuitable = true;
        }
    }

    private readonly struct DeviceIdentity
    {
        private readonly string value;

        public DeviceIdentity( string uuid )
        {
            value = uuid;
        }

        public override string ToString() => value;

        public static implicit operator string( DeviceIdentity uuid ) => uuid.ToString();
    }

    private class NullAdapter : ChannelAdapter<string>, IInputChannelAdapter
    {
        public override Task ExecuteAsync( IAdapterContext context, string data )
        {
            context.Forward( data );

            return Task.CompletedTask;
        }
    }
}
