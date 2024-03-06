// using System.Collections.Generic;
// using Faactory.Channels;
// using Faactory.Channels.Adapters;
// using Microsoft.Extensions.Logging;

// public class TestAdapterContext : IAdapterContext
// {
//     public IChannel Channel => new FakeChannel();

//     public IWritableBuffer Output => new FakeWritableBuffer();

//     public void NotifyCustomEvent( string name, object? data )
//     { }

//     public void Forward( object data )
//         => Data.Add( data );

//     public List<object> Data { get; } = new List<object>();
// }
