namespace Parcel.Channels;

public interface IClientChannelFactory
{
    Task<IChannel> CreateAsync( CancellationToken cancellationToken = default( CancellationToken ) );
    Task<IChannel> CreateAsync( string name, CancellationToken cancellationToken = default( CancellationToken ) );
}
