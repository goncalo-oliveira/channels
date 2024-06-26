using Faactory.Channels.Buffers;

namespace Faactory.Channels;

/// <summary>
/// A null channel is a channel that is always closed and cannot be written to.
/// </summary>
public sealed class NullChannel : IChannel
{
    private NullChannel()
    {}

    public static readonly NullChannel Instance = new();

    public string Id { get; } = Guid.Empty.ToString( "N" );

    public bool IsClosed => true;

    public IByteBuffer Buffer => throw new NotSupportedException();

    public ChannelData Data { get; } = [];

    public DateTimeOffset Created { get; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastReceived { get; }

    public DateTimeOffset? LastSent { get; }

    public IEnumerable<IChannelService> Services { get; } = [];

    public Task CloseAsync() => Task.CompletedTask;

    public void Dispose()
    { }

    public Task WriteAsync( object data ) => Task.CompletedTask;
}
