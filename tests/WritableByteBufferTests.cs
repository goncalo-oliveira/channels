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

public class WritableByteBufferTests
{
    [Fact]
    public void TestReplace()
    {
        var buffer = new WritableByteBuffer( new byte[]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        } );

        var instance = buffer.ReplaceBytes( new byte[]
        {
            0x01, 0x02
        }, new byte[]
        {
            0x01, 0x11, 0x02, 0x22
        } );

        Assert.Same( buffer, instance );

        Assert.True( buffer.MakeReadOnly().MatchBytes( new byte[]
        {
            0x00, 0x01, 0x11, 0x02, 0x22, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        } ) );

        Assert.Throws<NonWritableBufferException>( () =>
        {
            var readable = buffer.MakeReadOnly();

            readable.ReplaceBytes( new byte[]
            {
                0x01, 0x02
            }, new byte[]
            {
                0x11, 0x12
            } );
        } );
    }
}
