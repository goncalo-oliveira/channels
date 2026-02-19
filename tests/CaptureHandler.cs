using System.Threading;
using System.Threading.Tasks;
using Faactory.Channels;
using Faactory.Channels.Handlers;

namespace tests;

internal sealed class CaptureHandler<T>( TaskCompletionSource<T> tcs ) : ChannelHandler<T>
{
    public override Task ExecuteAsync( IChannelContext context, T data, CancellationToken cancellationToken )
    {
        tcs.TrySetResult( data );

        return Task.CompletedTask;
    }
}
