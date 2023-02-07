using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Faactory.Channels;
using Faactory.Channels.Buffers;
using Faactory.Collections;

public class FakeChannel : IChannel
{
    public string Id { get; } = Guid.NewGuid().ToString( "N" );

    public IMetadata Data => throw new NotImplementedException();

    public DateTimeOffset Created { get; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastReceived { get; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastSent { get; } = DateTimeOffset.UtcNow;

    public IByteBuffer Buffer { get; } = new WritableByteBuffer();

    public bool IsClosed { get; private set; }

    public IEnumerable<IChannelService> Services { get; } = Enumerable.Empty<IChannelService>();

    public Task CloseAsync()
    {
        IsClosed = true;

        return Task.CompletedTask;
    }

    public void Dispose()
    { }

    public Task WriteAsync( object data )
    {
        return Task.CompletedTask;
    }
}
