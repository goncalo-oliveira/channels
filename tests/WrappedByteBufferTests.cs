using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Faactory.Channels;
using Faactory.Channels.Buffers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class WrappedByteBufferTests
{
    [Fact]
    public void TestDiscardRead()
    {
        var buffer = new WrappedByteBuffer( new byte[]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        } );

        buffer.SkipBytes( 9 );
        buffer.DiscardReadBytes();

        Assert.Equal( 0, buffer.Offset );
        Assert.Equal( 1, buffer.ReadableBytes );
    }

    [Fact]
    public void TestDiscardAll()
    {
        var buffer = new WrappedByteBuffer( new byte[]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        } );

        buffer.SkipBytes( 5 );
        buffer.DiscardAll();

        Assert.Equal( 0, buffer.Offset );
        Assert.Equal( 0, buffer.ReadableBytes );
    }

    [Fact]
    public void TestMatches()
    {
        var buffer = new WrappedByteBuffer( new byte[]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        } );

        Assert.True( buffer.MatchBytes( new byte[]
        { 
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        } ) );

        Assert.True( buffer.MatchBytes( new byte[]
        { 
            0x00, 0x01
        } ) );

        Assert.False( buffer.MatchBytes( new byte[]
        { 
            0x00, 0x09
        } ) );

        Assert.True( buffer.MatchBytes( new byte[]
        { 
            0x01, 0x02
        }, 1 ) );

        Assert.False( buffer.MatchBytes( new byte[]
        { 
            0x00, 0x01
        }, 1 ) );

        Assert.Throws<InvalidOperationException>( () =>
        {
            var writable = buffer.MakeWritable();

            writable.MatchBytes( new byte[]
            { 
                0x00, 0x01
            } );
        } );
    }

    [Fact]
    public void TestFind()
    {
        var buffer = new WrappedByteBuffer( new byte[]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        } );

        Assert.Equal( 2, buffer.FindBytes( new byte[]
        {
            0x02, 0x03
        } ) );

        Assert.Equal( -1, buffer.FindBytes( new byte[]
        {
            0x02, 0x03
        }, 3 ) );

        Assert.Throws<InvalidOperationException>( () =>
        {
            var writable = buffer.MakeWritable();

            writable.FindBytes( new byte[]
            { 
                0x02, 0x03
            } );
        } );
    }

    [Fact]
    public void TestBufferSerializationBase64()
    {
        IByteBuffer buffer = new WrappedByteBuffer( new byte[]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        } );

        var json = JsonSerializer.Serialize( buffer );

        Array.Equals( "AAECAwQFBgcICQ==", json );
    }

    [Fact]
    public void TestBufferPropertySerializationBase64()
    {
        var obj = new
        {
            Id = Guid.NewGuid().ToString( "N" ),
            Number = Random.Shared.Next(),
            Buffer = (IByteBuffer)new WrappedByteBuffer( new byte[]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05
            } )
        };

        var json = JsonSerializer.Serialize( obj );

        Assert.Equal( $"{{\"Id\":\"{obj.Id}\",\"Number\":{obj.Number},\"Buffer\":\"AAECAwQF\"}}", json );
    }

    [Fact]
    public void TestBufferSerializationHex()
    {
        IByteBuffer buffer = new WrappedByteBuffer( new byte[]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        } );

        var options = new JsonSerializerOptions();
        options.Converters.Add( 
            new Faactory.Channels.Buffers.Serialization.ByteBufferJsonConverter( 
                Faactory.Channels.Buffers.Serialization.ByteBufferSerializerFormat.HexString ) );

        var json = JsonSerializer.Serialize( buffer, options );

        Array.Equals( "00010203040506070809", json );
    }

    [Fact]
    public void TestBufferPropertySerializationHex()
    {
        var obj = new ObjWithBuffer
        {
            Id = Guid.NewGuid().ToString( "N" ),
            Number = Random.Shared.Next(),
            Buffer = new WrappedByteBuffer( new byte[]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05
            } )
        };

        var json = JsonSerializer.Serialize( obj );

        Assert.Equal( $"{{\"Id\":\"{obj.Id}\",\"Number\":{obj.Number},\"Buffer\":\"000102030405\"}}", json );
    }

    private class ObjWithBuffer
    {
        public required string Id { get; set; }
        public int Number { get; set; }
        [Faactory.Channels.Buffers.Serialization.ByteBufferJsonConverter( 
            Format = Faactory.Channels.Buffers.Serialization.ByteBufferSerializerFormat.HexString 
        )]
        public required IByteBuffer Buffer { get; set; }
    }
}
