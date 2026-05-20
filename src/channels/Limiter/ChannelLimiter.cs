using Microsoft.Extensions.Options;

namespace Faactory.Channels;

internal sealed class ChannelLimiter( IChannelRegistry registry, IOptionsSnapshot<ChannelLimiterOptions> optionsAccessor ) : IChannelMonitor
{
    public void ChannelClosed( IChannelInfo channelInfo )
    { }

    public void ChannelCreated( IChannelInfo channelInfo )
    {
        var options = optionsAccessor.Get( channelInfo.Name );

        if ( options.ConnectionLimit <= 0 )
        {
            // No limit, admit all channels.
            return;
        }

        if ( !registry.TryGet( channelInfo.Id, out var channelHandle ) )
        {
            // Channel not found, cannot apply limit.
            return;
        }

        // get a snapshot of the active channels with the same name as the new channel
        var activeChannels = registry.Channels
            .Where( c => c.Name == channelHandle.Name )
            .ToArray();

        if ( activeChannels.Length <= options.ConnectionLimit )
        {
            // Under the limit, admit the channel.
            return;
        }

        switch ( options.ConnectionLimitPolicy )
        {
            case ConnectionLimitPolicy.RejectNewest:
                _ = channelHandle.CloseAsync();
                break;

            case ConnectionLimitPolicy.EvictOldest:
                _ = activeChannels
                    .Where( c => c.Id != channelHandle.Id )
                    .OrderBy( c => c.Created )
                    .FirstOrDefault()
                    ?.CloseAsync();
                break;
        }
    }

    public void CustomEvent( IChannelInfo channelInfo, string name, object? data )
    { }

    public void DataReceived( IChannelInfo channelInfo, ReadOnlySpan<byte> data )
    { }

    public void DataSent( IChannelInfo channelInfo, ReadOnlySpan<byte> data )
    { }
}
