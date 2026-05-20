namespace Faactory.Channels.Correlation;

using System.Collections.Concurrent;

/// <summary>
/// Registry for channel response awaiters. It allows to create awaiters for specific message types and push messages to them.
/// </summary>
internal sealed class ChannelResponseRegistry : IChannelResponseRegistry
{
    private readonly ConcurrentDictionary<Type, AwaiterList> lists = new();

    public IChannelResponseAwaiter<T> Create<T>( Func<T, bool> predicate )
    {
        ArgumentNullException.ThrowIfNull( predicate );

        var list = lists.GetOrAdd( typeof( T ), static _ => new AwaiterList() );
        var awaiter = new ChannelResponseAwaiter<T>();

        list.Add( predicate, awaiter );

        return awaiter;
    }

    public void Push<T>( T message )
    {
        if ( !lists.TryGetValue( typeof( T ), out var list ) )
        {
            // No awaiters for this type, so nothing to do.
            return;
        }

        list.Push( message );

        // if list becomes empty, remove it.
        if ( list.IsEmpty )
        {
            lists.TryRemove( new KeyValuePair<Type, AwaiterList>( typeof(T), list ) );
        }
    }
}
