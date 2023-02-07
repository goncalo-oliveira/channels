namespace Faactory.Channels;

internal sealed class EmptyChannelPipeline : IChannelPipeline
{
    public static IChannelPipeline Instance = new EmptyChannelPipeline();

    private EmptyChannelPipeline()
    { }

    public void Dispose()
    { }

    public Task ExecuteAsync( IChannel channel, object data )
    {
        return Task.CompletedTask;
    }
}
