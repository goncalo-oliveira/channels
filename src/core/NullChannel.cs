using Faactory.Channels.Buffers;
using Microsoft.Extensions.DependencyInjection;

namespace Faactory.Channels;

/// <summary>
/// A null channel is a channel that is always closed and cannot be written to.
/// </summary>
/// <remarks>
/// This can be used as a placeholder for channels that are not yet initialized or have been closed.
/// It can also be used for testing purposes.
/// </remarks>
public sealed class NullChannel : IChannel, IAsyncDisposable
{    
    private readonly Task initializeTask;
    private readonly CancellationTokenSource cts = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="NullChannel"/> class with optional services and a service scope.
    /// </summary>
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
    public static readonly IChannel Instance = new NullChannel( null );

    /// <summary>
    /// Gets the unique identifier of the channel.
    /// </summary>
    public string Id { get; } = Guid.Empty.ToString( "N" );

    /// <summary>
    /// Gets a value indicating whether the channel is closed. Always returns true for the null channel.
    /// </summary>
    public bool IsClosed => true;

    /// <summary>
    /// Gets the channel's data.
    /// </summary>
    public ChannelData Data { get; } = [];

    /// <summary>
    /// Gets the date and time when the channel was created.
    /// </summary>
    public DateTimeOffset Created { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the date and time when the channel last received data. Always returns null for the null channel.
    /// </summary>
    public DateTimeOffset? LastReceived { get; }

    /// <summary>
    /// Gets the date and time when the channel last sent data. Always returns null for the null channel.
    /// </summary>
    public DateTimeOffset? LastSent { get; }

    private IEnumerable<IChannelService> Services { get; }

    /// <summary>
    /// Gets the service scope associated with the channel, if any. This can be used to resolve services that are specific to the channel's lifetime.
    /// </summary>
    public IServiceScope? ChannelScope { get; }

    private int isClosing;

    /// <summary>
    /// Closes the channel asynchronously.
    /// </summary>
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

        // if initialization is still running, wait for it to complete
        try
        {
            await initializeTask
                .ConfigureAwait( false );
        }
        catch { }

        try
        {
            await StopServicesAsync()
                .ConfigureAwait( false );
        }
        catch ( OperationCanceledException )
        { }
    }

    /// <summary>
    /// Gets a channel service of the specified type from the channel's services. Returns null if no service of the specified type is found.
    /// </summary>
    /// <param name="serviceType">The type of the channel service to retrieve.</param>
    /// <returns>The channel service of the specified type, or null if not found.</returns>
    public IChannelService? GetChannelService( Type serviceType )
        => Services.SingleOrDefault( s => s.GetType() == serviceType );

    /// <summary>
    /// Disposes the channel and releases all resources associated with it.
    /// </summary>
    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Asynchronously disposes the channel and releases all resources associated with it.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await CloseAsync()
            .ConfigureAwait( false );
    }

    /// <summary>
    /// Writes data to the channel asynchronously. For the null channel, this method does nothing and completes immediately.
    /// </summary>
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
