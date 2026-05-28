using System;
using System.Text.Json;
using Faactory.Channels.Buffers;
using Faactory.Channels.Buffers.Serialization;
using Xunit;

namespace tests;

public class WrappedByteBufferTests
{
    [Fact]
    public void GetByteBuffer_ShouldCreateWindowedView()
    {
        var writable = new WritableByteBuffer( 16 );
        writable.WriteBytes( [0x00, 0x01, 0x02, 0x03, 0x04, 0x05] );

        var view = writable.AsReadableView();
        var slice = view.GetByteBuffer( 2, 3 );

        Assert.Equal( 3, slice.Length );
        Assert.Equal( new byte[] { 0x02, 0x03, 0x04 }, slice.AsSpan().ToArray() );
    }

    [Fact]
    public void ToArray_ShouldReturnBackingArray_WhenBufferOwnsData()
    {
        var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        var readable = new ReadableByteBuffer( bytes );

        var result = readable.ToArray();

        Assert.Same( bytes, result );
    }

    [Fact]
    public void ToArray_ShouldCopy_WhenWindowedView()
    {
        var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };

        var readable = new ReadableByteBuffer( bytes, 1, 3 );

        var result = readable.ToArray();

        Assert.NotSame( bytes, result );
        Assert.Equal( new byte[] { 0x02, 0x03, 0x04 }, result );
    }

    [Fact]
    public void ToArray_ShouldReturnBackingArray_WhenOwnsBufferAndFullWindow()
    {
        var bytes = new byte[] { 10, 20, 30 };

        var readable = new ReadableByteBuffer( bytes, Endianness.BigEndian );

        Assert.Same( bytes, readable.ToArray() );
    }

    [Fact]
    public void Test_Seek()
    {
        var buffer = new ReadableByteBuffer( [0x00, 0x01, 0x02, 0x03] );

        Assert.Equal( 0x00, buffer.ReadByte() );
        Assert.Equal( 0x01, buffer.ReadByte() );

        buffer.Seek( 0 );

        Assert.Equal( 0x00, buffer.ReadByte() );
        Assert.Equal( 0x01, buffer.ReadByte() );
        Assert.Equal( 0x02, buffer.ReadByte() );
        Assert.Equal( 0x03, buffer.ReadByte() );

        Assert.Throws<ArgumentOutOfRangeException>( () => buffer.Seek( -1 ) );
        Assert.Throws<ArgumentOutOfRangeException>( () => buffer.Seek( 5 ) );
    }

    [Fact]
    public void Checkpoint_ShouldAllowSpeculativeReads()
    {
        var buffer = new ReadableByteBuffer( [0x00, 0x01, 0x02, 0x03] );

        var offset = buffer.Offset;

        using var checkpoint = buffer.Checkpoint();

        Assert.Equal( 0x00, checkpoint.Buffer.ReadByte() );
        Assert.Equal( 0x01, checkpoint.Buffer.ReadByte() );

        // Original buffer should not be affected by reads on the checkpoint
        Assert.Equal( offset, buffer.Offset );

        // Commit the checkpoint to update the original buffer's offset
        checkpoint.Commit();

        Assert.NotEqual( offset, buffer.Offset );

        Assert.Equal( 0x02, buffer.ReadByte() );
        Assert.Equal( 0x03, buffer.ReadByte() );
    }

    [Fact]
    public void TestMatches()
    {
        var buffer = new ReadableByteBuffer(
        [
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        ] );

        Assert.True( buffer.MatchBytes(
        [
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        ] ) );

        Assert.True( buffer.MatchBytes(
        [
            0x00, 0x01
        ] ) );

        Assert.False( buffer.MatchBytes(
        [
            0x00, 0x09
        ] ) );

        Assert.True( buffer.MatchBytes(
        [
            0x01, 0x02
        ], 1 ) );

        Assert.False( buffer.MatchBytes(
        [
            0x00, 0x01
        ], 1 ) );
    }

    [Fact]
    public void TestFind()
    {
        var buffer = new ReadableByteBuffer(
        [
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        ] );

        Assert.Equal( 2, buffer.IndexOf(
        [
            0x02, 0x03
        ] ) );

        Assert.Equal( -1, buffer.IndexOf(
        [
            0x02, 0x03
        ], 3 ) );
    }

    [Fact]
    public void TestBufferSerializationBase64()
    {
        IByteBuffer buffer = new ReadableByteBuffer(
        [
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        ] );

        var json = JsonSerializer.Serialize( buffer );

        Array.Equals( "AAECAwQFBgcICQ==", json );
    }

    [Fact]
    public void TestBufferPropertySerializationBase64()
    {
        var obj = new
        {
            Id = Guid.NewGuid().ToString( "N" ),
            Number = Random.Shared.Next(),
            Buffer = (IByteBuffer)new ReadableByteBuffer(
            [
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05
            ] )
        };

        var json = JsonSerializer.Serialize( obj );

        Assert.Equal( $"{{\"Id\":\"{obj.Id}\",\"Number\":{obj.Number},\"Buffer\":\"AAECAwQF\"}}", json );
    }

    [Fact]
    public void TestBufferSerializationHex()
    {
        IByteBuffer buffer = new ReadableByteBuffer(
        [
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        ] );

        var options = new JsonSerializerOptions();
        options.Converters.Add( 
            new ByteBufferJsonConverterFactory(
                ByteBufferSerializerFormat.HexString ) );

        var json = JsonSerializer.Serialize( buffer, options );

        Array.Equals( "00010203040506070809", json );
    }

    [Fact]
    public void TestBufferPropertySerializationHex()
    {
        var obj = new ObjWithBuffer
        {
            Id = Guid.NewGuid().ToString( "N" ),
            Number = Random.Shared.Next(),
            Buffer = new ReadableByteBuffer(
            [
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05
            ] )
        };

        var json = JsonSerializer.Serialize( obj );

        Assert.Equal( $"{{\"Id\":\"{obj.Id}\",\"Number\":{obj.Number},\"Buffer\":\"000102030405\"}}", json );
    }

    [Fact]
    public void Test_FindBytes()
    {
        var source = new ReadableByteBuffer(
            [0x00, 0x01, 0x02, 0x03]
        );

        Assert.Equal( 0, source.IndexOf( [0x00, 0x01] ) );
        Assert.Equal( 1, source.IndexOf( [0x01, 0x02] ) );
        Assert.Equal( 2, source.IndexOf( [0x02, 0x03] ) );
        Assert.Equal( 3, source.IndexOf( [0x03] ) );

        Assert.Equal( 0, source.IndexOf( 0x00 ) );
        Assert.Equal( 1, source.IndexOf( 0x01 ) );
        Assert.Equal( 2, source.IndexOf( 0x02 ) );
        Assert.Equal( 3, source.IndexOf( 0x03 ) );

        source.SkipBytes( 1 );

        Assert.Equal( 1, source.IndexOf( [0x01, 0x02] ) );
        Assert.Equal( 2, source.IndexOf( [0x02, 0x03] ) );
        Assert.Equal( 3, source.IndexOf( [0x03] ) );
        Assert.Equal( 3, source.IndexOf( [0x03], 3 ) );

        Assert.Equal( -1, source.IndexOf( 0x00 ) );
        Assert.Equal( 1, source.IndexOf( 0x01 ) );
        Assert.Equal( 2, source.IndexOf( 0x02 ) );
        Assert.Equal( 3, source.IndexOf( 0x03 ) );
    }

    [Fact]
    public void Test_IndexOf()
    {
        var buffer = new ReadableByteBuffer( [0x00, 0x01, 0x02, 0x03, 0x04, 0x05] );

        Assert.Equal( -1, buffer.IndexOf( 0xff ) );
        Assert.Equal( -1, buffer.IndexOf( 0xff, 0 ) );
        Assert.Equal( -1, buffer.IndexOf( 0xff, 5 ) );

        Assert.Equal( -1, buffer.IndexOf( [0xff] ) );
        Assert.Equal( -1, buffer.IndexOf( [0xff], 5 ) );
        Assert.Equal( -1, buffer.IndexOf( [0xff], 0 ) );

        Assert.Equal( -1, buffer.IndexOf( [0xff, 0xff] ) );
        Assert.Equal( -1, buffer.IndexOf( [0xff, 0xff], 4 ) );
        Assert.Equal( -1, buffer.IndexOf( [0xff, 0xff], 5 ) );
        Assert.Equal( -1, buffer.IndexOf( [0xff, 0xff], 0 ) );

        Assert.Equal( 1, buffer.IndexOf( [0x01, 0x02] ) );
        Assert.Equal( 1, buffer.IndexOf( [0x01, 0x02], 0 ) );
        Assert.Equal( 1, buffer.IndexOf( [0x01, 0x02], 1 ) );

        Assert.Equal( 0, buffer.IndexOf( [0x00] ) );
        Assert.Equal( 0, buffer.IndexOf( [0x00], 0 ) );
        
        Assert.Equal( 1, buffer.IndexOf( [0x01] ) );
        Assert.Equal( 1, buffer.IndexOf( [0x01], 0 ) );
        Assert.Equal( 1, buffer.IndexOf( [0x01], 1 ) );

        Assert.Equal( 2, buffer.IndexOf( [0x02] ) );
        Assert.Equal( 2, buffer.IndexOf( [0x02], 0 ) );
        Assert.Equal( 2, buffer.IndexOf( [0x02], 1 ) );
        Assert.Equal( 2, buffer.IndexOf( [0x02], 2 ) );

        Assert.Equal( 3, buffer.IndexOf( [0x03] ) );
        Assert.Equal( 3, buffer.IndexOf( [0x03], 0 ) );
        Assert.Equal( 3, buffer.IndexOf( [0x03], 1 ) );
        Assert.Equal( 3, buffer.IndexOf( [0x03], 2 ) );
        Assert.Equal( 3, buffer.IndexOf( [0x03], 3 ) );

        Assert.Equal( 4, buffer.IndexOf( [0x04] ) );
        Assert.Equal( 4, buffer.IndexOf( [0x04], 0 ) );
        Assert.Equal( 4, buffer.IndexOf( [0x04], 1 ) );
        Assert.Equal( 4, buffer.IndexOf( [0x04], 2 ) );
        Assert.Equal( 4, buffer.IndexOf( [0x04], 3 ) );
        Assert.Equal( 4, buffer.IndexOf( [0x04], 4 ) );

        Assert.Equal( 5, buffer.IndexOf( [0x05] ) );
        Assert.Equal( 5, buffer.IndexOf( [0x05], 0 ) );
        Assert.Equal( 5, buffer.IndexOf( [0x05], 1 ) );
        Assert.Equal( 5, buffer.IndexOf( [0x05], 2 ) );
        Assert.Equal( 5, buffer.IndexOf( [0x05], 3 ) );
        Assert.Equal( 5, buffer.IndexOf( [0x05], 4 ) );
        Assert.Equal( 5, buffer.IndexOf( [0x05], 5 ) );
    }

    [Fact]
    public void Test_Any()
    {
        var buffer = new ReadableByteBuffer( [0x00, 0x01, 0x02, 0x03, 0x04, 0x05] );

        Assert.True( buffer.Any( b => b == 0x03 ) );
        Assert.True( buffer.Any( b => b > 0x00 ) );
        Assert.False( buffer.Any( b => b == 0xff ) );
    }

    [Fact]
    public void Test_UndoRead()
    {
        var buffer = new ReadableByteBuffer( [0x00, 0x01, 0x02, 0x03] );

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

    private class ObjWithBuffer
    {
        public required string Id { get; set; }
        public int Number { get; set; }
        [ByteBufferJsonConverter( 
            Format = ByteBufferSerializerFormat.HexString 
        )]
        public required IByteBuffer Buffer { get; set; }
    }
}
