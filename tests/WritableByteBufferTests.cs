using System;
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

    [Fact]
    public void ToArray_ShouldCopy_WhenCreatedFromWritableView_AndFullyFilled()
    {
        var writable = new WritableByteBuffer( 4 );
        writable.WriteBytes( [0x01, 0x02, 0x03, 0x04] );

        var readableView = writable.AsReadableView();

        var result = readableView.ToArray();

        Assert.NotSame( writable.ToArray(), result );
        Assert.Equal( new byte[] { 0x01, 0x02, 0x03, 0x04 }, result );
    }

    [Fact]
    public void ToArray_FromWritableView_ShouldBeIndependentCopy()
    {
        var writable = new WritableByteBuffer( 4 );
        writable.WriteBytes( [0x01, 0x02, 0x03, 0x04] );

        var readableView = writable.AsReadableView();
        var snapshot = readableView.ToArray();

        writable.Truncate();
        writable.WriteBytes( [0x09, 0x09, 0x09, 0x09] );

        Assert.Equal( new byte[] { 0x01, 0x02, 0x03, 0x04 }, snapshot );
    }    

    [Fact]
    public void AsReadableView_ShouldReflectSubsequentWrites()
    {
        var writable = new WritableByteBuffer( 8 );
        writable.WriteBytes( [1, 2, 3] );

        var view = writable.AsReadableView();

        // mutate writable without reallocating
        writable.Truncate();
        writable.WriteBytes( [0x09, 0x08, 0x07] );

        Assert.Equal( [0x09, 0x08, 0x07], view.GetBytes( 0, 3 ) );
    }

    [Fact]
    public void Reserve_ShouldAdvanceOffset_AndIncreaseLength()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [0x01, 0x02] );

        buffer.Reserve( 4 );

        Assert.Equal( 6, buffer.Length );
    }

    [Fact]
    public void Seek_ShouldAllowOverwritingExistingBytes()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [0x01, 0x02, 0x03, 0x04] );

        buffer.Seek( 1 );
        buffer.WriteByte( 0xFF );

        Assert.Equal(
            new byte[] { 0x01, 0xFF, 0x03, 0x04 },
            buffer.AsSpan().ToArray()
        );
    }

    [Fact]
    public void Seek_Overwrite_ShouldNotReduceLength()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [1,2,3,4,5] );

        buffer.Seek( 2 );
        buffer.WriteByte( 9 );

        Assert.Equal( 5, buffer.Length );
    }

    [Fact]
    public void Reserve_ThenSeek_ShouldAllowBackpatching()
    {
        var buffer = new WritableByteBuffer();

        var lengthPos = buffer.Length;

        buffer.Reserve( 4 ); // placeholder

        var payloadStart = buffer.Length;

        buffer.WriteBytes( [0xAA, 0xBB, 0xCC] );

        var payloadLength = buffer.Length - payloadStart;

        buffer.Seek( lengthPos );
        buffer.WriteUInt32( (uint)payloadLength );

        buffer.Seek( buffer.Length );

        Assert.Equal(
            new byte[]
            {
                0x00,0x00,0x00,0x03, // payload length
                0xAA,0xBB,0xCC
            },
            buffer.AsSpan().ToArray()
        );
    }

    [Fact]
    public void Seek_ShouldThrow_WhenOffsetGreaterThanLength()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [1,2,3] );

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            buffer.Seek( 4 ));
    }

    [Fact]
    public void ResetOffset_WithOffset_ShouldTruncateBuffer()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [0x01, 0x02, 0x03, 0x04, 0x05] );

        buffer.Truncate(3);

        Assert.Equal(3, buffer.Length);
        Assert.Equal( new byte[] { 0x01, 0x02, 0x03 }, buffer.AsSpan().ToArray() );

        buffer.WriteByte(0xFF);

        Assert.Equal( new byte[] { 0x01, 0x02, 0x03, 0xFF }, buffer.AsSpan().ToArray() );
    }

    [Fact]
    public void ResetOffset_ShouldThrow_WhenOffsetGreaterThanLength()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [1, 2, 3] );

        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Truncate( 4 ));
    }

}
