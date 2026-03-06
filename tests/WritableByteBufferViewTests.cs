using System;
using Faactory.Channels.Buffers;
using Xunit;

namespace tests;

public class WritableByteBufferViewTests
{
    [Fact]
    public void WritableView_WriteByte_ShouldModifyParentBuffer()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [1,2,3,4] );

        var view = buffer.At( 1 );

        view.WriteByte( 9 );

        Assert.Equal( new byte[] { 1,9,3,4 }, buffer.AsSpan().ToArray() );
    }

    [Fact]
    public void WritableView_ShouldThrow_WhenWritingBeyondParentLength()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [1,2,3] );

        var view = buffer.At( 2 );

        Assert.Throws<InvalidOperationException>(
            () => view.WriteBytes( [9,9] )
        );
    }

    [Fact]
    public void WritableView_Reserve_ShouldAdvanceWriteOffset()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [1,2,3,4] );

        var view = buffer.At( 1 );

        view.Reserve( 2 );
        view.WriteByte( 9 );

        Assert.Equal( new byte[] { 1,2,3,9 }, buffer.AsSpan().ToArray() );
    }

    [Fact]
    public void WritableView_Truncate_ShouldMoveCursorBack()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [1,2,3,4] );

        var view = buffer.At( 1 );

        view.WriteByte( 9 );
        view.Truncate();
        view.WriteByte( 8 );

        Assert.Equal( new byte[] { 1,8,3,4 }, buffer.AsSpan().ToArray() );
    }

    [Fact]
    public void WritableView_AsSpan_ShouldReturnViewSlice()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [1,2,3,4] );

        var view = buffer.At( 2 );

        Assert.Equal( new byte[] { 3,4 }, view.AsSpan().ToArray() );
    }

    [Fact]
    public void WritableView_ToArray_ShouldReturnCopyOfView()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [1,2,3,4] );

        var view = buffer.At( 1 );

        var result = view.ToArray();

        Assert.Equal( new byte[] { 2,3,4 }, result );
    }

    [Fact]
    public void WritableView_ShouldNotAllowNestedViews()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [1,2,3] );

        var view = buffer.At( 1 );

        Assert.Throws<NotSupportedException>( () => view.At( 0 ) );
    }

    [Fact]
    public void WritableView_WriteInt32_ShouldRespectEndianness()
    {
        var buffer = new WritableByteBuffer( Endianness.BigEndian );

        buffer.WriteBytes( [0,0,0,0,0,0] );

        var view = buffer.At( 1 );

        view.WriteInt32( 0x01020304 );

        Assert.Equal(
            new byte[] { 0,1,2,3,4,0 },
            buffer.AsSpan().ToArray()
        );
    }

    [Fact]
    public void WritableView_ShouldNotGrow_WhenParentBufferGrows()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [1,2,3,4,5] );

        var view = buffer.At( 2 );

        // grow parent after view creation
        buffer.WriteBytes( [6,7,8] );

        Assert.Equal( 3, view.Length );
        Assert.Equal( new byte[] { 3,4,5 }, view.AsSpan().ToArray() );
    }

    [Fact]
    public void WritableView_ShouldNotWriteIntoParentGrowthRegion()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [1,2,3,4,5] );

        var view = buffer.At( 3 );

        // parent grows
        buffer.WriteBytes( [6,7] );

        // view should still be limited to original parent length
        Assert.Throws<InvalidOperationException>(() =>
            view.WriteBytes( [9,9,9] )
        );
    }

    [Fact]
    public void WritableView_AsSpan_ShouldRemainFrozen_WhenParentGrows()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [1,2,3,4,5] );

        var view = buffer.At( 1 );

        buffer.WriteBytes( [6,7,8] );

        Assert.Equal( new byte[] { 2,3,4,5 }, view.AsSpan().ToArray() );
    }

    [Fact]
    public void WritableView_ToArray_ShouldRemainFrozen_WhenParentGrows()
    {
        var buffer = new WritableByteBuffer();

        buffer.WriteBytes( [1,2,3,4] );

        var view = buffer.At( 1 );

        buffer.WriteBytes( [5,6] );

        var snapshot = view.ToArray();

        Assert.Equal( new byte[] { 2,3,4 }, snapshot );
    }
}
