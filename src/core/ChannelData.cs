using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Faactory.Channels;

/// <summary>
/// A thread-safe data holder available throughout the entire channel session
/// </summary>
public sealed class ChannelData : ConcurrentDictionary<string, object>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelData"/> class.
    /// </summary>
    public ChannelData()
        : base( StringComparer.OrdinalIgnoreCase )
    { }

    /// <summary>
    /// Adds a key/value pair to the ChannelData by using the specified function if the key does not already exist. Returns the new value, or the existing value if the key exists.
    /// </summary>
    /// <typeparam name="T">The type of the value to add or retrieve.</typeparam>
    /// <param name="key">The key of the value to add or retrieve.</param>
    /// <param name="factory">The function to generate a value for the key if it does not exist.</param>
    /// <returns>The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value if the key was not in the dictionary.</returns>
    public T GetOrAdd<T>( string key, Func<string, T> factory ) where T : notnull
    {
        if ( base.GetOrAdd( key, k => factory( k ) ) is not T value )
        {
            throw new InvalidOperationException(
                $"The value associated with the key '{key}' is not of type '{typeof( T ).FullName}'."
            );
        }

        return value;
    }

    /// <summary>
    /// Attempts to get the value associated with the specified key from the ChannelData with the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found and the value is of the specified type; otherwise, the default value for the type.</param>
    /// <returns>true if the key was found and the value is of the specified type, false otherwise.</returns>
    public bool TryGetValue<T>( string key, [MaybeNullWhen( false )] out T value )
    {
        if ( TryGetValue( key, out var obj ) && obj is T typed )
        {
            value = typed;

            return true;
        }

        value = default;

        return false;
    }
}
