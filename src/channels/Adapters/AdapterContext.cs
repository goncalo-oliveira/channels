using Microsoft.Extensions.Logging;

namespace Faactory.Channels.Adapters;

internal class AdapterContext : ChannelContext, IAdapterContext
{
    private readonly List<object> forwardedData = new List<object>();

    public AdapterContext( IChannel channel )
        : base( channel )
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
