using Faactory.Channels.Buffers;
using Faactory.Channels.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

/// <summary>
/// A null channel is a channel that is always closed and cannot be written to.
/// </summary>
/// <remarks>
/// This can be used as a placeholder for channels that are not yet initialized or have been closed.
/// It can also be used for testing purposes.
/// </remarks>
/// <param name="serviceScope">The service scope to use for the channel. This is optional and can be null.</param>
public sealed class NullChannel : IChannel, IAsyncDisposable
{    
    private readonly Task initializeTask;
    private readonly CancellationTokenSource cts = new();

    public NullChannel( IServiceScope? serviceScope = null, IEnumerable<IChannelService>? channelServices = null )
    {
        ChannelScope = serviceScope;
        Services = channelServices ?? [];

        initializeTask = Services.Any()
            ? StartServicesAsync( cts.Token )
            : Task.CompletedTask;
    }

    /// <summary>
    /// A singleton instance of the null channel that can be used throughout the application.
    /// </summary>
    public static readonly NullChannel Instance = new( null );

    public string Id { get; } = Guid.Empty.ToString( "N" );

    public bool IsClosed => true;

    public IByteBuffer Buffer => throw new NotSupportedException();

    public ChannelData Data { get; } = [];

    public DateTimeOffset Created { get; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastReceived { get; }

    public DateTimeOffset? LastSent { get; }

    public IEnumerable<IChannelService> Services { get; }

    public IServiceScope? ChannelScope { get; }

    private int isClosing;

    public async Task CloseAsync()
    {
        if ( Interlocked.Exchange( ref isClosing, 1 ) == 1 )
        {
            return;
        }

        try
        {
            cts.Cancel();
        }
        catch { }

        try
        {
            await StopServicesAsync()
                .ConfigureAwait( false );
        }
        catch ( OperationCanceledException )
        { }

        initializeTask.TryDispose();
    }

    public void Dispose()
    {
        _ = CloseAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync()
            .ConfigureAwait( false );
    }

    public Task WriteAsync( object data ) => Task.CompletedTask;

    private Task StartServicesAsync( CancellationToken cancellationToken = default )
    {
        var tasks = Services.Select( async service =>
        {
            try
            {
                await service.StartAsync( this, cancellationToken );
            }
            catch ( OperationCanceledException )
            { }
        } );

        return Task.WhenAll( tasks );
    }

    private Task StopServicesAsync()
    {
        var tasks = Services.Select( async service =>
        {
            try
            {
                await service.StopAsync();
            }
            catch ( OperationCanceledException )
            { }
        } );

        return Task.WhenAll( tasks );
    }

}
