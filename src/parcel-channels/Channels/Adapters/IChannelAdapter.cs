namespace Parcel.Channels.Adapters;

/// <summary>
/// A channel data adapter
/// </summary>
public interface IChannelAdapter
{
    /// <summary>
    /// Executes the data adapter
    /// </summary>
    /// <param name="context">The adapter context</param>
    /// <param name="data">The data to adapt</param>
    Task ExecuteAsync( IAdapterContext context, object data );
}

/// <summary>
/// A channel adapter that can be used on the input pipeline
/// </summary>
public interface IInputChannelAdapter {}

/// <summary>
/// A channel adapter that can be used on the output pipeline
/// </summary>
public interface IOutputChannelAdapter {}
