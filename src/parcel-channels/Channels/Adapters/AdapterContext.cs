using Microsoft.Extensions.Logging;

namespace Parcel.Channels.Adapters;

internal class AdapterContext : ChannelContext, IAdapterContext
{
    private readonly List<object> forwardedData = new List<object>();

    public AdapterContext( ILoggerFactory loggerFactory, IChannel channel )
        : base( loggerFactory, channel )
    {}

    public void Forward( object data )
    {
        forwardedData.Add( data );
    }

    internal object[] Flush()
    {
        var result = forwardedData.ToArray();

        forwardedData.Clear();

        return ( result );
    }
}
