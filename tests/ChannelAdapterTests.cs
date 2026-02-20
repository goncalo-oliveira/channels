using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Faactory.Channels;
using Faactory.Channels.Adapters;
using Faactory.Channels.Buffers;
using Xunit;

namespace tests;

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

    // adapter<IReadableByteBuffer>.execute( byte[] ) should wrap data in an IReadableByteBuffer
    [Fact]
    public async Task Test_ReadableBuffer_Mutation()
    {
        var data = Encoding.ASCII.GetBytes( Guid.NewGuid().ToString( "N" ) );

        var context = new DetachedContext();
        IChannelAdapter adapter = new ReadableBufferAdapter();

        await adapter.ExecuteAsync( context, data );

        Assert.Single( context.Forwarded );

        var contextData = Assert.IsType<IReadableByteBuffer>( context.Forwarded.Single(), exactMatch: false );

        Assert.True( data.SequenceEqual( contextData.ToArray() ) );
    }

    // adapter<IByteBuffer>.execute( byte[] ) should wrap data in an IByteBuffer
    [Fact]
    public async Task Test_Buffer_Mutation()
    {
        var data = Encoding.ASCII.GetBytes( Guid.NewGuid().ToString( "N" ) );

        var context = new DetachedContext();
        IChannelAdapter adapter = new BufferAdapter();

        await adapter.ExecuteAsync( context, data );

        Assert.Single( context.Forwarded );

        var contextData = Assert.IsType<IByteBuffer>( context.Forwarded.Single(), exactMatch: false );

        Assert.True( data.SequenceEqual( contextData.ToArray() ) );
    }

    // adapter<CustomByteBuffer>.execute( byte[] ) should throw an exception since CustomByteBuffer is not a supported conversion type
    // even though it implements IByteBuffer
    // adapter<CustomByteBuffer>.execute( CustomByteBuffer ) should do a pass-through execution since the data is already of the expected type, so no conversion is attempted
    // adapter<IByteBuffer>.execute( CustomByteBuffer ) should do a pass-through execution since CustomByteBuffer implements IByteBuffer
    [Fact]
    public async Task Test_CustomBuffer_Mutation()
    {
        var data = Encoding.ASCII.GetBytes( Guid.NewGuid().ToString( "N" ) );

        var context = new DetachedContext();
        IChannelAdapter adapter = new CustomByteBufferAdapter();

        // adapter<CustomByteBuffer>.execute( byte[] ) throws an exception since CustomByteBuffer is not a supported conversion type
        // even though CustomByteBuffer implements IByteBuffer, the conversion is not supported since it is a custom type and not a known framework type
        await Assert.ThrowsAsync<InvalidCastException>( async () => await adapter.ExecuteAsync( context, data ) );

        // explicit payload works because it is already of the expected type, so no conversion is attempted
        await adapter.ExecuteAsync( context, new CustomByteBuffer( data ) );

        var customBuffer = Assert.IsType<CustomByteBuffer>( context.Forwarded.Single(), exactMatch: false );

        Assert.True( data.SequenceEqual( customBuffer.ToArray() ) );

        context.Clear();

        // adapter<IByteBuffer>.execute( CustomByteBuffer ) should do a pass-through execution since CustomByteBuffer implements IByteBuffer
        adapter = new BufferAdapter();

        await adapter.ExecuteAsync( context, new CustomByteBuffer( data ) );

        Assert.Single( context.Forwarded );

        var buffer = Assert.IsType<IByteBuffer>( context.Forwarded.Single(), exactMatch: false );

    }

    // adapter<byte[]>.execute( IReadableByteBuffer ) should unwrap IReadableByteBuffer data
    [Fact]
    public async Task TestByteArrayMutation()
    {
        var data = new ReadableByteBuffer( Encoding.ASCII.GetBytes( Guid.NewGuid().ToString( "N" ) ) );

        var context = new DetachedContext();
        IChannelAdapter adapter = new ByteArrayAdapter();

        await adapter.ExecuteAsync( context, data );

        Assert.Single( context.Forwarded );

        var contextData = Assert.IsType<byte[]>( context.Forwarded.Single() );

        Assert.True( data.ToArray().SequenceEqual( contextData ) );
    }

    private class CustomByteBuffer( byte[] data ) : IByteBuffer
    {
        public int ReadableBytes => data.Length;
        public Endianness Endianness => Endianness.BigEndian;
        public int Length => data.Length;

        public ReadOnlySpan<byte> AsSpan() => new( data );
        public byte[] ToArray() => data;
    }

    private class CustomByteBufferAdapter : ChannelAdapter<CustomByteBuffer>
    {
        public override Task ExecuteAsync( IAdapterContext context, CustomByteBuffer data, CancellationToken cancellationToken )
        {
            context.Forward( data );

            return Task.CompletedTask;
        }
    }

    private class ObjectAdapter : ChannelAdapter<string>
    {
        public override Task ExecuteAsync( IAdapterContext context, string data, CancellationToken cancellationToken )
        {
            context.Forward( data );

            return Task.CompletedTask;
        }
    }

    private class ObjectArrayAdapter : ChannelAdapter<string[]>
    {
        public override Task ExecuteAsync( IAdapterContext context, string[] data, CancellationToken cancellationToken )
        {
            context.Forward( data );

            return Task.CompletedTask;
        }
    }

    private class ReadableBufferAdapter : ChannelAdapter<IReadableByteBuffer>
    {
        public override Task ExecuteAsync( IAdapterContext context, IReadableByteBuffer data, CancellationToken cancellationToken )
        {
            context.Forward( data );

            return Task.CompletedTask;
        }
    }

    private class BufferAdapter : ChannelAdapter<IByteBuffer>
    {
        public override Task ExecuteAsync( IAdapterContext context, IByteBuffer data, CancellationToken cancellationToken )
        {
            context.Forward( data );

            return Task.CompletedTask;
        }
    }

    private class ByteArrayAdapter : ChannelAdapter<byte[]>
    {
        public override Task ExecuteAsync( IAdapterContext context, byte[] data, CancellationToken cancellationToken )
        {
            context.Forward( data );

            return Task.CompletedTask;
        }
    }
}
