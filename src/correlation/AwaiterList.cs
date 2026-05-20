namespace Faactory.Channels.Correlation;

/// <summary>
/// Maintains a list of awaiters waiting for messages.
/// </summary>
internal sealed class AwaiterList
{
    private readonly Lock sync = new();
    private readonly List<IEntry> entries = [];

    public bool IsEmpty
    {
        get
        {
            lock ( sync )
            {
                return entries.Count == 0;
            }
        }
    }

    public void Add<T>( Func<T, bool> predicate, ChannelResponseAwaiter<T> awaiter )
    {
        Add( new Entry<T>( predicate, awaiter ) );
    }

    public void Add( IEntry entry )
    {
        lock ( sync )
        {
            entries.Add( entry );
        }
    }

    public void Push<T>( T message )
    {
        List<IEntry>? matches = null;

        lock ( sync )
        {
            // remove completed/canceled while scanning, and collect matches
            for ( int i = entries.Count - 1; i >= 0; i-- )
            {
                var e = entries[i];

                if ( e.IsCompleted )
                {
                    entries.RemoveAt( i );
                    continue;
                }

                if ( e is Entry<T> typed && typed.Matches( message ) )
                {
                    (matches ??= []).Add( e );
                    entries.RemoveAt( i );
                }
            }
        }

        // Complete outside lock (important!)
        if ( matches is not null )
        {
            foreach ( var m in matches )
            {
                m.TryComplete( message! );
            }
        }
    }

    /// <summary>
    /// Represents an entry in the awaiter list.
    /// </summary>
    public interface IEntry
    {
        /// <summary>
        /// Indicates whether the awaiter has already completed or been canceled.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Determines whether the given message matches the entry's predicate.
        /// </summary>
        /// <param name="message">The message to test.</param>
        void TryComplete( object message );
    }

    private sealed class Entry<T>( Func<T, bool> predicate, ChannelResponseAwaiter<T> awaiter ) : IEntry
    {
        public bool IsCompleted => awaiter.IsCompleted;

        public bool Matches(T message) => predicate(message);

        public void TryComplete( object message )
        {
            // Safe cast: only called from Push<T> after matching Entry<T>
            awaiter.TrySetResult( (T)message );
        }
    }
}
