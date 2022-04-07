using System;
using System.Threading.Tasks;
using Faactory.Channels;
using Faactory.Collections;

public class FakeChannel : IChannel
{
    public string Id { get; } = Guid.NewGuid().ToString( "N" );

    public IMetadata Data => throw new NotImplementedException();

    public DateTimeOffset Created => throw new NotImplementedException();

    public DateTimeOffset? LastReceived => throw new NotImplementedException();

    public DateTimeOffset? LastSent => throw new NotImplementedException();

    public Task CloseAsync()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    { }

    public Task WriteAsync( object data )
    {
        return Task.CompletedTask;
    }
}
