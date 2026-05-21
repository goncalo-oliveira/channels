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
    private readonly IServiceScope? channelScope;

    /// <summary>
    /// Initializes a new instance of the <see cref="NullChannel"/> class with optional services and a service scope.
    /// </summary>
    public NullChannel( IServiceScope? serviceScope = null, IEnumerable<IChannelService>? channelServices = null )
    {
        channelScope = serviceScope;
        ChannelServices = channelServices ?? [];

        initializeTask = ChannelServices.Any()
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
    /// Gets the name of the channel, which corresponds to the channel configuration name.
    /// Always returns "NullChannel" for the null channel.
    /// </summary>
    public string Name { get; } = "NullChannel";

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

    /// <summary>
    /// Gets the scoped service provider associated with the channel lifetime.
    /// For a detached channel, this property returns a null service provider since it is not associated with any scope or context.
    /// </summary>
    public IServiceProvider Services => channelScope?.ServiceProvider ?? NullServiceProvider.Instance;

    /// <summary>
    /// Gets the channel services initialized for the current channel instance.
    /// </summary>
    public IEnumerable<IChannelService> ChannelServices { get; }

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

        try
        {
            await StopServicesAsync()
                .ConfigureAwait( false );
        }
        catch ( OperationCanceledException )
        { }
    }

    /// <summary>
    /// Disposes the channel and releases all resources associated with it.
    /// </summary>
    public void Dispose()
    { }

    /// <summary>
    /// Asynchronously disposes the channel and releases all resources associated with it.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await CloseAsync()
            .ConfigureAwait( false );

        // if initialization is still running, wait for it to complete
        try
        {
            await initializeTask
                .ConfigureAwait( false );
        }
        catch { }
    }

    /// <summary>
    /// Writes data to the channel asynchronously. For the null channel, this method does nothing and completes immediately.
    /// </summary>
    public Task WriteAsync( object data ) => Task.CompletedTask;

    private Task StartServicesAsync( CancellationToken cancellationToken = default )
    {
        var tasks = ChannelServices.Select( async service =>
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
        var tasks = ChannelServices.Select( async service =>
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
