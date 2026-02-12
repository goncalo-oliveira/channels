using System;
using System.Linq;
using Faactory.Channels.Buffers;
using Xunit;

namespace Faactory.Channels.Tests;

public class ByteBufferTests
{
    [Fact]
    public void TestMakeReadOnly()
    {
        /*
        When the buffer is already read-only and the endianness doesn't change
        the instance returned should be the same as the original.
        */
        IByteBuffer source = new WrappedByteBuffer( new byte[] { 0x00, 0x01 }, Endianness.BigEndian );
        IByteBuffer readOnly = source.MakeReadOnly();

        Assert.Equal( source, readOnly );
        Assert.True( readOnly.IsReadable );
        Assert.False( readOnly.IsWritable );

        /*
        However, if the endianness changes, the returned instance should be
        a new instance with the new endianness, even if the buffer is already
        read-only.
        */

        source = new WrappedByteBuffer( new byte[] { 0x00, 0x01 }, Endianness.BigEndian );
        readOnly = source.MakeReadOnly( Endianness.LittleEndian );

        Assert.NotEqual( source, readOnly );
        Assert.True( readOnly.IsReadable );
        Assert.False( readOnly.IsWritable );

        /*
        If the buffer is not read-only, the returned instance should be a new
        instance with the same endianness.
        */

        source = new WritableByteBuffer()
            .WriteBytes( new byte[] { 0x00, 0x01 } );
        
        readOnly = source.MakeReadOnly();

        Assert.NotEqual( source, readOnly );
        Assert.True( readOnly.IsReadable );
        Assert.False( readOnly.IsWritable );
    }

    [Fact]
    public void TestMakeWritable()
    {
        /*
        When the buffer is already writable and the endianness doesn't change
        the instance returned should be the same as the original.
        */
        IByteBuffer source = new WritableByteBuffer()
            .WriteBytes( new byte[] { 0x00, 0x01 } );

        IByteBuffer writable = source.MakeWritable();

        Assert.Equal( source, writable );
        Assert.True( writable.IsWritable );
        Assert.False( writable.IsReadable );

        /*
        However, if the endianness changes, the returned instance should be
        a new instance with the new endianness, even if the buffer is already
        writable.
        */

        source = new WritableByteBuffer()
            .WriteBytes( new byte[] { 0x00, 0x01 } );

        writable = source.MakeWritable( Endianness.LittleEndian );

        Assert.NotEqual( source, writable );
        Assert.True( writable.IsWritable );
        Assert.False( writable.IsReadable );

        /*
        If the buffer is not writable, the returned instance should be a new
        instance with the same endianness.
        */

        source = new WrappedByteBuffer( new byte[] { 0x00, 0x01 }, Endianness.BigEndian );
        writable = source.MakeWritable();

        Assert.NotEqual( source, writable );
        Assert.True( writable.IsWritable );
        Assert.False( writable.IsReadable );
    }

    [Fact]
    public void TestFindBytes()
    {
        IByteBuffer source = new WrappedByteBuffer(
            new byte[] { 0x00, 0x01, 0x02, 0x03 }
        );

        Assert.Equal( 0, source.IndexOf( new byte[] { 0x00, 0x01 } ) );
        Assert.Equal( 1, source.IndexOf( new byte[] { 0x01, 0x02 } ) );
        Assert.Equal( 2, source.IndexOf( new byte[] { 0x02, 0x03 } ) );
        Assert.Equal( 3, source.IndexOf( new byte[] { 0x03 } ) );

        Assert.Equal( 0, source.IndexOf( 0x00 ) );
        Assert.Equal( 1, source.IndexOf( 0x01 ) );
        Assert.Equal( 2, source.IndexOf( 0x02 ) );
        Assert.Equal( 3, source.IndexOf( 0x03 ) );

        source.SkipBytes( 1 );

        Assert.Equal( 1, source.IndexOf( new byte[] { 0x01, 0x02 } ) );
        Assert.Equal( 2, source.IndexOf( new byte[] { 0x02, 0x03 } ) );
        Assert.Equal( 3, source.IndexOf( new byte[] { 0x03 } ) );
        Assert.Equal( 3, source.IndexOf( new byte[] { 0x03 }, 3 ) );

        Assert.Equal( -1, source.IndexOf( 0x00 ) );
        Assert.Equal( 1, source.IndexOf( 0x01 ) );
        Assert.Equal( 2, source.IndexOf( 0x02 ) );
        Assert.Equal( 3, source.IndexOf( 0x03 ) );
    }

    [Fact]
    public void TestIndexOf()
    {
        var buffer = new WrappedByteBuffer( new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 } );

        Assert.Equal( -1, buffer.IndexOf( 0xff ) );
        Assert.Equal( -1, buffer.IndexOf( 0xff, 0 ) );
        Assert.Equal( -1, buffer.IndexOf( 0xff, 5 ) );

        Assert.Equal( -1, buffer.IndexOf( new byte[] { 0xff } ) );
        Assert.Equal( -1, buffer.IndexOf( new byte[] { 0xff }, 5 ) );
        Assert.Equal( -1, buffer.IndexOf( new byte[] { 0xff }, 0 ) );

        Assert.Equal( -1, buffer.IndexOf( new byte[] { 0xff, 0xff } ) );
        Assert.Equal( -1, buffer.IndexOf( new byte[] { 0xff, 0xff }, 4 ) );
        Assert.Equal( -1, buffer.IndexOf( new byte[] { 0xff, 0xff }, 5 ) );
        Assert.Equal( -1, buffer.IndexOf( new byte[] { 0xff, 0xff }, 0 ) );

        Assert.Equal( 1, buffer.IndexOf( new byte[] { 0x01, 0x02 } ) );
        Assert.Equal( 1, buffer.IndexOf( new byte[] { 0x01, 0x02 }, 0 ) );
        Assert.Equal( 1, buffer.IndexOf( new byte[] { 0x01, 0x02 }, 1 ) );

        Assert.Equal( 0, buffer.IndexOf( new byte[] { 0x00 } ) );
        Assert.Equal( 0, buffer.IndexOf( new byte[] { 0x00 }, 0 ) );
        
        Assert.Equal( 1, buffer.IndexOf( new byte[] { 0x01 } ) );
        Assert.Equal( 1, buffer.IndexOf( new byte[] { 0x01 }, 0 ) );
        Assert.Equal( 1, buffer.IndexOf( new byte[] { 0x01 }, 1 ) );

        Assert.Equal( 2, buffer.IndexOf( new byte[] { 0x02 } ) );
        Assert.Equal( 2, buffer.IndexOf( new byte[] { 0x02 }, 0 ) );
        Assert.Equal( 2, buffer.IndexOf( new byte[] { 0x02 }, 1 ) );
        Assert.Equal( 2, buffer.IndexOf( new byte[] { 0x02 }, 2 ) );

        Assert.Equal( 3, buffer.IndexOf( new byte[] { 0x03 } ) );
        Assert.Equal( 3, buffer.IndexOf( new byte[] { 0x03 }, 0 ) );
        Assert.Equal( 3, buffer.IndexOf( new byte[] { 0x03 }, 1 ) );
        Assert.Equal( 3, buffer.IndexOf( new byte[] { 0x03 }, 2 ) );
        Assert.Equal( 3, buffer.IndexOf( new byte[] { 0x03 }, 3 ) );

        Assert.Equal( 4, buffer.IndexOf( new byte[] { 0x04 } ) );
        Assert.Equal( 4, buffer.IndexOf( new byte[] { 0x04 }, 0 ) );
        Assert.Equal( 4, buffer.IndexOf( new byte[] { 0x04 }, 1 ) );
        Assert.Equal( 4, buffer.IndexOf( new byte[] { 0x04 }, 2 ) );
        Assert.Equal( 4, buffer.IndexOf( new byte[] { 0x04 }, 3 ) );
        Assert.Equal( 4, buffer.IndexOf( new byte[] { 0x04 }, 4 ) );

        Assert.Equal( 5, buffer.IndexOf( new byte[] { 0x05 } ) );
        Assert.Equal( 5, buffer.IndexOf( new byte[] { 0x05 }, 0 ) );
        Assert.Equal( 5, buffer.IndexOf( new byte[] { 0x05 }, 1 ) );
        Assert.Equal( 5, buffer.IndexOf( new byte[] { 0x05 }, 2 ) );
        Assert.Equal( 5, buffer.IndexOf( new byte[] { 0x05 }, 3 ) );
        Assert.Equal( 5, buffer.IndexOf( new byte[] { 0x05 }, 4 ) );
        Assert.Equal( 5, buffer.IndexOf( new byte[] { 0x05 }, 5 ) );
    }

    [Fact]
    public void TestAny()
    {
        var buffer = new WrappedByteBuffer( new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 } );

        Assert.True( buffer.Any( b => b == 0x03 ) );
        Assert.True( buffer.Any( b => b > 0x00 ) );
        Assert.False( buffer.Any( b => b == 0xff ) );
    }

    [Fact]
    public void TestUndoRead()
    {
        var buffer = new WrappedByteBuffer( [0x00, 0x01, 0x02, 0x03] );

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

    [Fact]
    public void TestMatchBytes()
    {
        var buffer = new WrappedByteBuffer( [0x00, 0x01, 0x02, 0x03] );

        Assert.True( buffer.MatchBytes( [0x00, 0x01] ) );
        Assert.False( buffer.MatchBytes( [0x02, 0x03] ) );

        Assert.Equal( 0x00, buffer.ReadByte() );
        Assert.Equal( 0x01, buffer.ReadByte() );

        Assert.True( buffer.MatchBytes( [0x02, 0x03] ) );
        Assert.False( buffer.MatchBytes( [0x00, 0x01] ) );
    }
}
