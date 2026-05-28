using System;
using System.Buffers.Binary;
using System.Collections.Generic;
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

        Assert.True( buffer.AsReadableView().MatchBytes(
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
    public void WriteByte_ShouldIncreaseLength()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteByte(1);
        buffer.WriteByte(2);

        Assert.Equal(2, buffer.Length);
        Assert.Equal(new byte[] { 1, 2 }, buffer.AsSpan().ToArray());
    }

    [Fact]
    public void ResetOffset_ShouldClearLengthWithoutReallocating()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteByte(1);
        buffer.Truncate();

        Assert.Equal(0, buffer.Length);

        buffer.WriteByte(5);
        Assert.Equal(new byte[] { 5 }, buffer.AsSpan().ToArray());
    }

    [Fact]
    public void Clear_ShouldResetLength()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteByte(1);
        buffer.WriteByte(2);

        buffer.Clear();

        Assert.Equal(0, buffer.Length);
    }

    [Fact]
    public void EnsureCapacity_ShouldGrowBuffer()
    {
        var buffer = new WritableByteBuffer(4);

        buffer.WriteBytes(new byte[10], 0, 10);

        Assert.Equal(10, buffer.Length);
    }

    [Fact]
    public void Compact_ShouldShiftRemainingBytes()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes([1, 2, 3, 4], 0, 4);

        buffer.Compact(2);

        Assert.Equal(new byte[] { 3, 4 }, buffer.AsSpan().ToArray());
    }

    [Fact]
    public void Dispose_ShouldReleaseBuffer()
    {
        var bufferAllocator = new TestBufferAllocator();

        var buffer = new WritableByteBuffer(
            bufferAllocator
        );

        Assert.Equal( 1, bufferAllocator.References );

        buffer.Dispose();

        Assert.Equal( 0, bufferAllocator.References );
    }

    [Fact]
    public void Methods_ShouldThrowAfterDispose()
    {
        var buffer = new WritableByteBuffer();

        buffer.Dispose();

        Assert.Throws<ObjectDisposedException>(() => buffer.AsSpan());
        Assert.Throws<ObjectDisposedException>(() => buffer.AsReadableView());
        Assert.Throws<ObjectDisposedException>(() => buffer.Clear());
        Assert.Throws<ObjectDisposedException>(() => buffer.ToArray());
    }

    [Fact]
    public void Clear_ShouldReleaseOversizedBuffer()
    {
        var bufferAllocator = new TestBufferAllocator();

        var buffer = new WritableByteBuffer(
            bufferAllocator
        );

        buffer.WriteBytes( new byte[5000], 0, 5000 );

        var originalBuffer = buffer.Buffer;

        buffer.Clear();

        Assert.Equal( 1, bufferAllocator.References ); // still 1, but the oversized buffer should have been released
        Assert.NotSame( originalBuffer, buffer.Buffer ); // should have allocated a new buffer
        Assert.Equal( 0, buffer.Length );
    }

    [Fact]
    public void Reserve_ShouldAdvanceOffset_AndIncreaseLength()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes([0x01, 0x02]);

        buffer.Reserve(4);

        Assert.Equal(6, buffer.Length);
    }

    [Fact]
    public void Truncate_WithOffset_ShouldTruncateBuffer()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes([0x01, 0x02, 0x03, 0x04, 0x05]);

        buffer.Truncate(3);

        Assert.Equal(3, buffer.Length);
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03 }, buffer.AsSpan().ToArray());

        buffer.WriteByte(0xFF);

        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0xFF }, buffer.AsSpan().ToArray());
    }

    [Fact]
    public void Truncate_ShouldThrow_WhenOffsetGreaterThanLength()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes([1, 2, 3]);

        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Truncate(4));
    }

    [Fact]
    public void Writable_GrowsCorrectly()
    {
        var buffer = new WritableByteBuffer();

        for ( int i = 0; i < 10000; i++ )
        {
            buffer.WriteInt32( i );
        }

        Assert.Equal( 10000 * 4, buffer.Length );

        // get private buffer field using reflection
        var field = typeof( WritableByteBuffer ).GetField( "buffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
        var internalBuffer = (byte[])field!.GetValue( buffer )!;

        /*
        initial capacity is 1024
        every time the buffer needs to grow, it doubles its capacity
        writing 4 bytes at a time, the buffer will grow as follows:
        1024 -> 2048 -> 4096 -> 8192 -> 16384 -> 32768 -> 65536 -> 131072 -> etc...
        the first 1024 need 256 writes, the next 1024 need 256 writes, the next 2048 need 512 writes, etc...
        how many times the buffer needs to grow to accommodate 10000 * 4 bytes?
        10000 * 4 = 40000 bytes
        1024 -> 2048 -> 4096 -> 8192 -> 16384 -> 32768 -> 65536
        the buffer needs to grow 6 times to accommodate 40000 bytes, which means the final capacity will be 65536 bytes
        */

        Assert.Equal( 65536, internalBuffer.Length );

        var span = buffer.AsSpan();

        for ( int i = 0; i < 10000; i++ )
        {
            var value = BinaryPrimitives.ReadInt32BigEndian(
                span.Slice( i * 4, 4 )
            );

            Assert.Equal( i, value );
        }
    }

    [Fact]
    public void Writable_Compact_ShouldShrinkBuffer_WhenCapacityExceedsMaxRetainedCapacity()
    {
        var bufferAllocator = new TestBufferAllocator();

        var buffer = new WritableByteBuffer(
            new WritableByteBufferOptions
            {
                InitialCapacity = 16,
                MaxRetainedCapacity = 16
            },
            bufferAllocator
        );

        // force growth
        buffer.WriteBytes( new byte[128] );

        var grownCapacity = buffer.Buffer.Length;

        Assert.True( grownCapacity > 16 );

        var originalBuffer = buffer.Buffer;

        // compact most data away
        buffer.Compact( 120 );

        // remaining data should be small
        Assert.Equal( 8, buffer.Length );

        // old oversized buffer should have been released
        Assert.Equal( 1, bufferAllocator.References );
        Assert.NotSame( originalBuffer, buffer.Buffer );

        // buffer should shrink back to retained capacity
        Assert.Equal( 16, buffer.Buffer.Length );
    }

    [Fact]
    public void Writable_Compact_WithZeroOffset_ShouldShrinkBuffer_WhenCapacityExceedsMaxRetainedCapacity()
    {
        var buffer = new WritableByteBuffer(
            new WritableByteBufferOptions
            {
                InitialCapacity = 16,
                MaxRetainedCapacity = 32
            }
        );

        // force growth
        buffer.WriteBytes( new byte[128] );

        Assert.True( buffer.Buffer.Length > 32 );

        // simulate logical reset
        buffer.Truncate();

        // should still apply retention policy
        buffer.Compact( 0 );

        Assert.Equal( 0, buffer.Length );
        Assert.Equal( 32, buffer.Buffer.Length );
    }

    private class TestBufferAllocator : IByteBufferAllocator
    {
        public int References { get; private set; }

        public byte[] Allocate( int size )
        {
            References++;

            return new byte[size];
        }

        public void Release( byte[] buffer )
        {
            References--;
        }
    }
}
