using System.Collections.Generic;
using Faactory.Channels;
using Faactory.Channels.Adapters;
using Microsoft.Extensions.Logging;

public class TestAdapterContext : IAdapterContext
{
    public TestAdapterContext( ILoggerFactory loggerFactory )
    {
        LoggerFactory = loggerFactory;
    }

    public ILoggerFactory LoggerFactory { get; }

    public IChannel Channel => new FakeChannel();

    public IWritableBuffer Output => new FakeWritableBuffer();

    public void Forward( object data )
    {
        Data.Add( data );
    }

    public List<object> Data { get; } = new List<object>();
}
