using Parcel.Protocol;

namespace Parcel.Channels.Observables;

public static class MessageObserverExtensions
{
    public static Task<Message?> WaitForAsync( this IMessageObserver observer, string messageId )
        => observer.WaitForAsync( messageId, TimeSpan.FromSeconds( 20 ) );
}
