using Faactory.Channels.Buffers;
using Xunit;

namespace tests;

public class WritableByteBufferTests
{
    [Fact]
    public void TestReplace()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes(
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

        Assert.True( buffer.AsReadable().MatchBytes(
        [
            0x00, 0x01, 0x11, 0x02, 0x22, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        ] ) );
    }

    [Fact]
    public void AsReadableView_ShouldExposeWrittenPortion_WithoutCopy()
    {
        var writable = new WritableByteBuffer( 16 );
        writable.WriteBytes( [0x01, 0x02, 0x03, 0x04] );

        var view = writable.AsReadableView();

        Assert.Equal( 4, view.Length );
        Assert.Equal( 0, view.Offset );
        Assert.Equal( new byte[] { 0x01, 0x02, 0x03, 0x04 }, view.AsSpan().ToArray() );
    }

    [Fact]
    public void Compact_ShouldShiftUnreadBytesToStart()
    {
        var writable = new WritableByteBuffer( 16 );
        writable.WriteBytes( [0x01, 0x02, 0x03, 0x04, 0x05, 0x06] );

        var view = writable.AsReadableView();

        view.ReadBytes( 2 ); // consume 0x01, 0x02

        writable.Compact( view.Offset );

        var newView = writable.AsReadableView();

        Assert.Equal( 4, newView.Length );
        Assert.Equal( new byte[] { 0x03, 0x04, 0x05, 0x06 }, newView.AsSpan().ToArray() );
    }

    [Fact]
    public void AsReadable_ShouldCreateIndependentCopy()
    {
        var writable = new WritableByteBuffer( 16 );
        writable.WriteBytes( [0x01, 0x02, 0x03] );

        var copy = writable.AsReadable();

        writable.WriteBytes( [0x04] );

        Assert.Equal( new byte[] { 0x01, 0x02, 0x03 }, copy.AsSpan().ToArray() );
    }
}
