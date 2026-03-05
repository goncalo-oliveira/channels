using System;
using Faactory.Channels.Buffers;
using Xunit;

namespace tests;

public class TrackedByteBufferPoolTests
{
    [Fact]
    public void Rent_ShouldReturnWritableBuffer()
    {
        using var pool = new TrackedByteBufferPool();

        var buffer = pool.Rent();

        buffer.WriteInt32( 123 );

        Assert.Equal( 4, buffer.Length );
    }

    [Fact]
    public void ManuallyDisposedBuffer_ShouldNotBeDisposedAgain()
    {
        using var pool = new TrackedByteBufferPool();

        var buffer = pool.Rent();

        buffer.Dispose();

        // should not throw
        pool.Dispose();
    }

    [Fact]
    public void PoolDispose_ShouldDisposeAllRemainingBuffers()
    {
        var pool = new TrackedByteBufferPool();

        var buffer = pool.Rent();

        pool.Dispose();

        Assert.Throws<ObjectDisposedException>( () => buffer.WriteByte( 1 ) );
    }

    [Fact]
    public void PoolDispose_ShouldDisposeAllTrackedBuffers()
    {
        var pool = new TrackedByteBufferPool();

        var b1 = pool.Rent();
        var b2 = pool.Rent();

        pool.Dispose();

        Assert.Throws<ObjectDisposedException>( () => b1.WriteByte( 1 ) );
        Assert.Throws<ObjectDisposedException>( () => b2.WriteByte( 1 ) );
    }

    [Fact]
    public void EarlyDispose_ShouldNotAffectOtherBuffers()
    {
        using var pool = new TrackedByteBufferPool();

        var b1 = pool.Rent();
        var b2 = pool.Rent();

        b1.Dispose();

        b2.WriteByte( 1 );

        pool.Dispose();
    }

}
