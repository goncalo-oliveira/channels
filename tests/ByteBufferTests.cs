using System;
using System.Buffers.Binary;
using System.Linq;
using Faactory.Channels.Buffers;
using Xunit;

namespace tests;

public class ByteBufferTests
{
    [Fact]
    public void Test_As_Readable()
    {
        /*
        If the buffer is not read-only, the returned instance should be a new
        instance with the same endianness.
        */

        var writable = new WritableByteBuffer()
            .WriteBytes( [0x00, 0x01] );
        
        var readble = writable.AsReadable();

        Assert.NotEqual( (IByteBuffer)writable, readble);
        Assert.IsType<IReadableByteBuffer>( readble, exactMatch: false );
        Assert.IsType<IWritableByteBuffer>( writable, exactMatch: false );
    }

    [Fact]
    public void Test_As_Writable()
    {
        /*
        If the buffer is not writable, the returned instance should be a new
        instance with the same endianness.
        */

        var readable = new ReadableByteBuffer( [0x00, 0x01], Endianness.BigEndian );
        var writable = readable.AsWritable();

        Assert.NotEqual( (IByteBuffer)readable, writable );
        Assert.IsType<IWritableByteBuffer>( writable, exactMatch: false );
        Assert.IsType<IReadableByteBuffer>( readable, exactMatch: false );
    }
}
