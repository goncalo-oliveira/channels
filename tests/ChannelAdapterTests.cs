using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Faactory.Channels.Adapters;
using Faactory.Channels.Buffers;
using Xunit;

namespace Faactory.Channels.Tests;

public class ChannelAdapterTests
{
    // adapter<T>.execute( T[] ) should do a spread execution: execute( T[].element )
    [Fact]
    public async Task TestEnumerableDataMutation()
    {
        var data = new string[]
        {
            Guid.NewGuid().ToString( "N" ),
            Guid.NewGuid().ToString( "N" ),
            Guid.NewGuid().ToString( "N" )
        };

        var context = new DetachedContext();
        IChannelAdapter adapter = new ObjectAdapter();

        await adapter.ExecuteAsync( context, data );

        Assert.Equal( data.Length, context.Forwarded.Count() );

        for ( int idx = 0; idx < data.Length; idx++ )
        {
            Assert.Equal( data[idx], (string)context.Forwarded.ElementAt( idx ) );
        }
    }

    // adapter<T[]>.execute( T ) should do a wrap execution; execute( T[] { T } )
    [Fact]
    public async Task TestEnumerableAdapterMutation()
    {
        var data = Guid.NewGuid().ToString( "N" );

        var context = new DetachedContext();
        IChannelAdapter adapter = new ObjectArrayAdapter();

        await adapter.ExecuteAsync( context, data );

        Assert.Single( context.Forwarded );

        var contextData = context.Forwarded.Single();

        Assert.True( contextData.GetType().IsEnumerable() );

        var contextDataItems = ( (IEnumerable)contextData ).OfType<string>();

        Assert.Single( contextDataItems );

        Assert.Equal( data, contextDataItems.Single() );
    }

    // adapter<IByteBuffer>.execute( byte[] ) should wrap data in an IByteBuffer
    [Fact]
    public async Task TestBufferMutation()
    {
        var data = Encoding.ASCII.GetBytes( Guid.NewGuid().ToString( "N" ) );

        var context = new DetachedContext();
        IChannelAdapter adapter = new BufferAdapter();

        await adapter.ExecuteAsync( context, data );

        Assert.Single( context.Forwarded );

        var contextData = Assert.IsType<WrappedByteBuffer>( context.Forwarded.Single() );

        Assert.True( data.SequenceEqual( contextData.ToArray() ) );
    }

    // adapter<byte[]>.execute( IByteBuffer ) should unwrap IByteBuffer data
    [Fact]
    public async Task TestByteArrayMutation()
    {
        var data = new WrappedByteBuffer( Encoding.ASCII.GetBytes( Guid.NewGuid().ToString( "N" ) ) );

        var context = new DetachedContext();
        IChannelAdapter adapter = new ByteArrayAdapter();

        await adapter.ExecuteAsync( context, data );

        Assert.Single( context.Forwarded );

        var contextData = Assert.IsType<byte[]>( context.Forwarded.Single() );

        Assert.True( data.ToArray().SequenceEqual( contextData ) );
    }

    private class ObjectAdapter : ChannelAdapter<string>
    {
        public override Task ExecuteAsync( IAdapterContext context, string data )
        {
            context.Forward( data );

            return Task.CompletedTask;
        }
    }

    private class ObjectArrayAdapter : ChannelAdapter<string[]>
    {
        public override Task ExecuteAsync( IAdapterContext context, string[] data )
        {
            context.Forward( data );

            return Task.CompletedTask;
        }
    }

    private class BufferAdapter : ChannelAdapter<IByteBuffer>
    {
        public override Task ExecuteAsync( IAdapterContext context, IByteBuffer data )
        {
            context.Forward( data );

            return Task.CompletedTask;
        }
    }

    private class ByteArrayAdapter : ChannelAdapter<byte[]>
    {
        public override Task ExecuteAsync( IAdapterContext context, byte[] data )
        {
            context.Forward( data );

            return Task.CompletedTask;
        }
    }
}
