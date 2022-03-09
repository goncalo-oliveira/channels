namespace Parcel.Protocol;

/// <summary>
/// A packet message
/// </summary>
public sealed class Message
{
    /// <summary>
    /// The message identifier
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The message content type
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// The message content
    /// </summary>
    public byte[]? Content { get; set; }

    /// <summary>
    /// The message signature
    /// </summary>
    public string? Signature { get; set; }

    public int Length
        => ( 1 + ( Id?.Length ?? 0 ) )
         + ( 1 + ( ContentType?.Length ?? 0 ) )
         + ( 2 + ( Content?.Length ?? 0 ) )
         + ( 1 + ( Signature?.Length ?? 0 ) );
}
