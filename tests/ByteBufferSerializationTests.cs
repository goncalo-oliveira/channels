using System;
using System.Linq;
using System.Text.Json;
using Faactory.Channels.Buffers;
using Faactory.Channels.Buffers.Serialization;
using Xunit;

namespace Faactory.Channels.Tests;

public class ByteBufferSerializationTests
{
    [Fact]
    public void Test_RoundTrip_Serialization()
    {
        var obj = new Object
        {
            Buffer = new ReadableByteBuffer(
                [0x54, 0x45, 0x53, 0x54] // "TEST" in ASCII
            )
        };

        var json = JsonSerializer.Serialize( obj );

        // json should contain the base64 string representation of the buffer
        Assert.Equal( $"{{\"Buffer\":\"{Convert.ToBase64String( obj.Buffer.AsSpan() )}\"}}", json );

        var deserialized = JsonSerializer.Deserialize<Object>( json );

        Assert.NotNull( deserialized );
        Assert.NotNull( deserialized.Buffer );

        // deserialized buffer should be a readable buffer
        Assert.IsType<IReadableByteBuffer>( deserialized.Buffer, exactMatch: false );

        // deserialized buffer should have the same content as the original buffer
        Assert.True( deserialized.Buffer.AsSpan().SequenceEqual( obj.Buffer.AsSpan() ) );
    }

    [Fact]
    public void Test_RoundTrip_Serialization_HexString()
    {
        var options = new JsonSerializerOptions
        {
            Converters =
            {
                new ByteBufferJsonConverterFactory( ByteBufferSerializerFormat.HexString )
            }
        };

        var obj = new Object
        {
            Buffer = new ReadableByteBuffer(
                [0x54, 0x45, 0x53, 0x54] // "TEST" in ASCII
            )
        };

        var json = JsonSerializer.Serialize( obj, options );

        // json should contain the hex string representation of the buffer
        Assert.Equal( $"{{\"Buffer\":\"54455354\"}}", json );

        var deserialized = JsonSerializer.Deserialize<Object>( json, options );

        Assert.NotNull( deserialized );
        Assert.NotNull( deserialized.Buffer );

        // deserialized buffer should be a readable buffer
        Assert.IsType<IReadableByteBuffer>( deserialized.Buffer, exactMatch: false );

        // deserialized buffer should have the same content as the original buffer
        Assert.True( deserialized.Buffer.AsSpan().SequenceEqual( obj.Buffer.AsSpan() ) );
    }

    [Fact]
    public void Test_RoundTrip_Serialization_IReadableByteBuffer()
    {
        var obj = new ReadableObject
        {
            Buffer = new ReadableByteBuffer(
                [0x54, 0x45, 0x53, 0x54] // "TEST" in ASCII
            )
        };

        var json = JsonSerializer.Serialize( obj );

        // json should contain the base64 string representation of the buffer
        Assert.Equal( $"{{\"Buffer\":\"{Convert.ToBase64String( obj.Buffer.AsSpan() )}\"}}", json );

        var deserialized = JsonSerializer.Deserialize<ReadableObject>( json );

        Assert.NotNull( deserialized );
        Assert.NotNull( deserialized.Buffer );

        // deserialized buffer should be a readable buffer
        Assert.IsType<IReadableByteBuffer>( deserialized.Buffer, exactMatch: false );

        // deserialized buffer should have the same content as the original buffer
        Assert.True( deserialized.Buffer.AsSpan().SequenceEqual( obj.Buffer.AsSpan() ) );
    }

    [Fact]
    public void Test_RoundTrip_Serialization_IWritableByteBuffer()
    {
        var obj = new WritableObject
        {
            Buffer = new WritableByteBuffer()
        };

        obj.Buffer.WriteBytes( [0x54, 0x45, 0x53, 0x54] ); // "TEST" in ASCII

        var json = JsonSerializer.Serialize( obj );

        // json should contain the base64 string representation of the buffer
        Assert.Equal( $"{{\"Buffer\":\"{Convert.ToBase64String( obj.Buffer.AsSpan() )}\"}}", json );

        // deserialization of IWritableByteBuffer should throw NotSupportedException
        Assert.Throws<NotSupportedException>( () =>
        {
            JsonSerializer.Deserialize<WritableObject>( json );
        } );
    }

    private class Object
    {
        public IByteBuffer? Buffer { get; set; }
    }

    private class ReadableObject
    {
        public IReadableByteBuffer? Buffer { get; set; }
    }

    private class WritableObject
    {
        public IWritableByteBuffer? Buffer { get; set; }
    }
}
