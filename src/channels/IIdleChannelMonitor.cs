namespace Faactory.Channels;

public interface IIdleChannelMonitor : IDisposable
{
    void Start( IChannel channel );
    void Stop();
}
