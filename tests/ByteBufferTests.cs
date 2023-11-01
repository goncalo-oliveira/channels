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
}
