using System;
using System.Threading.Tasks;
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
}
