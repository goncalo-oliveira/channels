using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
}