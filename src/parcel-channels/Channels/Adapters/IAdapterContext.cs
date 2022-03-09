namespace Parcel.Channels.Adapters;

public interface IAdapterContext : IChannelContext
{
    void Forward( object data );
}
