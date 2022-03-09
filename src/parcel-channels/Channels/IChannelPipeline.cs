namespace Parcel.Channels;

public interface IChannelPipeline : IDisposable
{
    Task ExecuteAsync( IChannel channel, object data );
}
