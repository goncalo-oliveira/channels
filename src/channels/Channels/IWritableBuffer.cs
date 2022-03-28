namespace Faactory.Channels;

/// <summary>
/// An interface for a writable buffer
/// </summary>
public interface IWritableBuffer
{
    /// <summary>
    /// Writes data to the buffer
    /// </summary>
    /// <param name="data">The data to be written</param>
    void Write( object data );
}
