namespace Faactory.Channels.Adapters;

/// <summary>
/// Options for the buffer length adapter
/// </summary>
public sealed class BufferLengthAdapterOptions
{
    public BufferLengthAdapterOptions()
    {
        MaxLength = 1024 * 1024;
        CloseChannel = true;
    }

    /// <summary>
    /// The maximum length for the input IByteBuffer. Default is 1 MB.
    /// </summary>
    public int MaxLength { get; set; }

    /// <summary>
    /// If true, the channel is closed when the maximum length is exceeded. Default is true.
    /// </summary>
    public bool CloseChannel { get; set; }
}
