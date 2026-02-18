using System;
using System.Linq;
using Faactory.Channels.Buffers;
using Xunit;

namespace Faactory.Channels.Tests;

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

    [Fact]
    public void Test_FindBytes()
    {
        var source = new ReadableByteBuffer(
            [0x00, 0x01, 0x02, 0x03]
        );

        Assert.Equal( 0, source.IndexOf( [0x00, 0x01] ) );
        Assert.Equal( 1, source.IndexOf( [0x01, 0x02] ) );
        Assert.Equal( 2, source.IndexOf( [0x02, 0x03] ) );
        Assert.Equal( 3, source.IndexOf( [0x03] ) );

        Assert.Equal( 0, source.IndexOf( 0x00 ) );
        Assert.Equal( 1, source.IndexOf( 0x01 ) );
        Assert.Equal( 2, source.IndexOf( 0x02 ) );
        Assert.Equal( 3, source.IndexOf( 0x03 ) );

        source.SkipBytes( 1 );

        Assert.Equal( 1, source.IndexOf( [0x01, 0x02] ) );
        Assert.Equal( 2, source.IndexOf( [0x02, 0x03] ) );
        Assert.Equal( 3, source.IndexOf( [0x03] ) );
        Assert.Equal( 3, source.IndexOf( [0x03], 3 ) );

        Assert.Equal( -1, source.IndexOf( 0x00 ) );
        Assert.Equal( 1, source.IndexOf( 0x01 ) );
        Assert.Equal( 2, source.IndexOf( 0x02 ) );
        Assert.Equal( 3, source.IndexOf( 0x03 ) );
    }

    [Fact]
    public void TestIndexOf()
    {
        var buffer = new ReadableByteBuffer( [0x00, 0x01, 0x02, 0x03, 0x04, 0x05] );

        Assert.Equal( -1, buffer.IndexOf( 0xff ) );
        Assert.Equal( -1, buffer.IndexOf( 0xff, 0 ) );
        Assert.Equal( -1, buffer.IndexOf( 0xff, 5 ) );

        Assert.Equal( -1, buffer.IndexOf( [0xff] ) );
        Assert.Equal( -1, buffer.IndexOf( [0xff], 5 ) );
        Assert.Equal( -1, buffer.IndexOf( [0xff], 0 ) );

        Assert.Equal( -1, buffer.IndexOf( [0xff, 0xff] ) );
        Assert.Equal( -1, buffer.IndexOf( [0xff, 0xff], 4 ) );
        Assert.Equal( -1, buffer.IndexOf( [0xff, 0xff], 5 ) );
        Assert.Equal( -1, buffer.IndexOf( [0xff, 0xff], 0 ) );

        Assert.Equal( 1, buffer.IndexOf( [0x01, 0x02] ) );
        Assert.Equal( 1, buffer.IndexOf( [0x01, 0x02], 0 ) );
        Assert.Equal( 1, buffer.IndexOf( [0x01, 0x02], 1 ) );

        Assert.Equal( 0, buffer.IndexOf( [0x00] ) );
        Assert.Equal( 0, buffer.IndexOf( [0x00], 0 ) );
        
        Assert.Equal( 1, buffer.IndexOf( [0x01] ) );
        Assert.Equal( 1, buffer.IndexOf( [0x01], 0 ) );
        Assert.Equal( 1, buffer.IndexOf( [0x01], 1 ) );

        Assert.Equal( 2, buffer.IndexOf( [0x02] ) );
        Assert.Equal( 2, buffer.IndexOf( [0x02], 0 ) );
        Assert.Equal( 2, buffer.IndexOf( [0x02], 1 ) );
        Assert.Equal( 2, buffer.IndexOf( [0x02], 2 ) );

        Assert.Equal( 3, buffer.IndexOf( [0x03] ) );
        Assert.Equal( 3, buffer.IndexOf( [0x03], 0 ) );
        Assert.Equal( 3, buffer.IndexOf( [0x03], 1 ) );
        Assert.Equal( 3, buffer.IndexOf( [0x03], 2 ) );
        Assert.Equal( 3, buffer.IndexOf( [0x03], 3 ) );

        Assert.Equal( 4, buffer.IndexOf( [0x04] ) );
        Assert.Equal( 4, buffer.IndexOf( [0x04], 0 ) );
        Assert.Equal( 4, buffer.IndexOf( [0x04], 1 ) );
        Assert.Equal( 4, buffer.IndexOf( [0x04], 2 ) );
        Assert.Equal( 4, buffer.IndexOf( [0x04], 3 ) );
        Assert.Equal( 4, buffer.IndexOf( [0x04], 4 ) );

        Assert.Equal( 5, buffer.IndexOf( [0x05] ) );
        Assert.Equal( 5, buffer.IndexOf( [0x05], 0 ) );
        Assert.Equal( 5, buffer.IndexOf( [0x05], 1 ) );
        Assert.Equal( 5, buffer.IndexOf( [0x05], 2 ) );
        Assert.Equal( 5, buffer.IndexOf( [0x05], 3 ) );
        Assert.Equal( 5, buffer.IndexOf( [0x05], 4 ) );
        Assert.Equal( 5, buffer.IndexOf( [0x05], 5 ) );
    }

    [Fact]
    public void TestAny()
    {
        var buffer = new ReadableByteBuffer( [0x00, 0x01, 0x02, 0x03, 0x04, 0x05] );

        Assert.True( buffer.Any( b => b == 0x03 ) );
        Assert.True( buffer.Any( b => b > 0x00 ) );
        Assert.False( buffer.Any( b => b == 0xff ) );
    }

    [Fact]
    public void TestUndoRead()
    {
        var buffer = new ReadableByteBuffer( [0x00, 0x01, 0x02, 0x03] );

        Assert.Equal( 0x00, buffer.ReadByte() );
        Assert.Equal( 0x01, buffer.ReadByte() );

        buffer.UndoRead( 1 );

        Assert.Equal( 0x01, buffer.ReadByte() );

        buffer.UndoRead( 2 );

        Assert.Equal( 0x00, buffer.ReadByte() );
        Assert.Equal( 0x01, buffer.ReadByte() );
        Assert.Equal( 0x02, buffer.ReadByte() );
        Assert.Equal( 0x03, buffer.ReadByte() );

        buffer.ResetOffset();

        Assert.Throws<ArgumentOutOfRangeException>( () => buffer.UndoRead( 1 ) );
    }
}
