using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Parcel.Protocol;

namespace Parcel.Channels.Observables;

internal class MessageObserver : IMessageObserver
{
    private class Observable
    {
        public Observable()
        {
            CancellationTokenSource = new CancellationTokenSource();
        }

        public CancellationTokenSource CancellationTokenSource { get; }
        public Message? Value { get; set; }
    }

    private readonly ILogger logger;
    private readonly ConcurrentDictionary<string, Observable> observables = new ConcurrentDictionary<string, Observable>();

    public MessageObserver( ILoggerFactory loggerFactory )
    {
        logger = loggerFactory.CreateLogger<MessageObserver>();
    }

    public async Task<Message?> WaitForAsync( string messageId, TimeSpan timeout )
    {
        var observable = new Observable();

        if ( !observables.TryAdd( messageId, observable ) )
        {
            return ( null );
        }

        logger.LogInformation( $"Waiting for '{messageId}'..." );

        try
        {
            await Task.Delay( timeout, observable.CancellationTokenSource.Token );
        }
        catch ( TaskCanceledException )
        {}
        finally
        {
            observables.TryRemove( messageId, out _ );
        }

        return observable.Value;
    }

    public void Push( Message message )
    {
        if ( message.Id == null )
        {
            // ignore null identifiers
            return;
        }

        var observable = observables.GetValueOrDefault( message.Id );

        if ( observable == null )
        {
            // observable not found
            return;
        }

        logger.LogInformation( $"Pushed '{message.Id}'." );

        observable.Value = message;
        observable.CancellationTokenSource.Cancel();
    }
}
