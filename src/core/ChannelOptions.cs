using System.Diagnostics;

namespace Faactory.Channels;

/// <summary>
/// Represents the options for configuring a channel
/// </summary>
public class ChannelOptions
{
    internal Func<IChannelInfo, TagList> MetricsTagsFactory { get; private set; } = _ => default;

    /// <summary>
    /// Gets or sets the channel's input buffer endianness; default is Buffers.Endianness.BigEndian.
    /// </summary>
    public Buffers.Endianness BufferEndianness { get; set; } = Buffers.Endianness.BigEndian;

    /// <summary>
    /// Gets or sets the channel's timeout value when no data is sent or received; default is 60 seconds. Set to TimeSpan.Zero to disable.
    /// </summary>
    public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromSeconds( 60 );

    /// <summary>
    /// Configures a factory for creating metrics tags for the channel.
    /// </summary>
    /// <param name="factory">A function that takes an IChannelInfo instance and returns a TagList representing the metrics tags.</param>
    public void ConfigureMetricsTags( Func<IChannelInfo, TagList> factory )
        => MetricsTagsFactory = factory;
}
