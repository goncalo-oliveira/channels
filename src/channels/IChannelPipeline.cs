namespace Faactory.Channels;

public interface IChannelPipeline : IDisposable
{
    Task ExecuteAsync( IChannel channel, object data );
}
