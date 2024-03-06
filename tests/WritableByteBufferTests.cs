using Faactory.Channels.Buffers;
using Xunit;

namespace Faactory.Channels.Tests;

public class WritableByteBufferTests
{
    [Fact]
    public void TestReplace()
    {
        var buffer = new WritableByteBuffer(
        [
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        ] );

        var instance = buffer.ReplaceBytes(
        [
            0x01, 0x02
        ],
        [
            0x01, 0x11, 0x02, 0x22
        ] );

        Assert.Same( buffer, instance );

        Assert.True( buffer.MakeReadOnly().MatchBytes(
        [
            0x00, 0x01, 0x11, 0x02, 0x22, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        ] ) );

        Assert.Throws<NonWritableBufferException>( () =>
        {
            var readable = buffer.MakeReadOnly();

            readable.ReplaceBytes(
            [
                0x01, 0x02
            ],
            [
                0x11, 0x12
            ] );
        } );
    }
}
