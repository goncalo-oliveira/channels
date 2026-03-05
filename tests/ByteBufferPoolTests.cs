using System;
using Faactory.Channels.Buffers;
using Xunit;

namespace tests;

public class ByteBufferPoolTests
{
    [Fact]
    public void Rent_ShouldUseDefaultInitialCapacity()
    {
        var pool = new TestPool();

        pool.Rent();

        Assert.Equal( WritableByteBuffer.InitialCapacity, pool.LastCapacity );
    }

    [Fact]
    public void Rent_ShouldThrowIfPoolIsNull()
    {
        IByteBufferPool pool = null!;

        Assert.Throws<ArgumentNullException>( () => pool.Rent() );
    }

    [Fact]
    public void Rent_ShouldReturnWritableBuffer()
    {
        var pool = new TestPool();

        using var buffer = pool.Rent();

        buffer.WriteByte(1);

        Assert.Equal(1, buffer.Length);
    }

    private sealed class TestPool : IByteBufferPool
    {
        public int? LastCapacity;

        public IWritableByteBuffer Rent( int capacity )
        {
            LastCapacity = capacity;

            return new WritableByteBuffer();
        }
    }
}
