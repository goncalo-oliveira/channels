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

        writable.ResetOffset();
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
        writable.ResetOffset();
        writable.WriteBytes( [0x09, 0x08, 0x07] );

        Assert.Equal( [0x09, 0x08, 0x07], view.GetBytes( 0, 3 ) );
    }

    [Fact]
    public void WriteByte_ShouldIncreaseLength()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteByte( 1 );
        buffer.WriteByte( 2 );

        Assert.Equal( 2, buffer.Length );
        Assert.Equal( new byte[] { 1, 2 }, buffer.AsSpan().ToArray() );
    }

    [Fact]
    public void ResetOffset_ShouldClearLengthWithoutReallocating()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteByte( 1 );
        buffer.ResetOffset();

        Assert.Equal( 0, buffer.Length );

        buffer.WriteByte( 5 );
        Assert.Equal( new byte[] { 5 }, buffer.AsSpan().ToArray() );
    }

    [Fact]
    public void Clear_ShouldResetLength()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteByte( 1 );
        buffer.WriteByte( 2 );

        buffer.Clear();

        Assert.Equal( 0, buffer.Length );
    }

    [Fact]
    public void EnsureCapacity_ShouldGrowBuffer()
    {
        var buffer = new WritableByteBuffer( 4 );

        buffer.WriteBytes( new byte[ 10 ], 0, 10 );

        Assert.Equal( 10, buffer.Length );
    }

    [Fact]
    public void Compact_ShouldShiftRemainingBytes()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [1, 2, 3, 4], 0, 4 );

        buffer.Compact( 2 );

        Assert.Equal( new byte[] { 3, 4 }, buffer.AsSpan().ToArray() );
    }

    [Fact]
    public void Dispose_ShouldReleaseBuffer()
    {
        bool released = false;

        var buffer = new WritableByteBuffer(
            allocator: size => new byte[size],
            releaser: _ => released = true
        );

        buffer.Dispose();

        Assert.True( released );
    }

    [Fact]
    public void Methods_ShouldThrowAfterDispose()
    {
        var buffer = new WritableByteBuffer();

        buffer.Dispose();

        Assert.Throws<ObjectDisposedException>( () => buffer.AsSpan() );
        Assert.Throws<ObjectDisposedException>( () => buffer.AsReadableView() );
        Assert.Throws<ObjectDisposedException>( () => buffer.Clear() );
        Assert.Throws<ObjectDisposedException>( () => buffer.ToArray() );
    }

    [Fact]
    public void Clear_ShouldReleaseOversizedBuffer()
    {
        bool released = false;

        var buffer = new WritableByteBuffer(
            allocator: size => new byte[size],
            releaser: _ => released = true
        );

        buffer.WriteBytes( new byte[ 5000 ], 0, 5000 );

        buffer.Clear();

        Assert.True( released );
        Assert.Equal( 0, buffer.Length );
    }

}
