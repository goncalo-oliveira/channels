using System.Diagnostics.Metrics;

namespace Faactory.Channels;

internal static class Metrics
{
    private static readonly Meter Meter = new( new MeterOptions( "channels" )
    {
        Version = typeof( Metrics ).Assembly.GetName().Version?.ToString()
    } );

    public static readonly Counter<long> BytesReceived =
        Meter.CreateCounter<long>( "bytes.received", "bytes", "Total number of bytes received" );

    public static readonly Counter<long> BytesSent =
        Meter.CreateCounter<long>( "bytes.sent", "bytes", "Total number of bytes sent" );

    public static readonly UpDownCounter<long> ActiveChannels =
        Meter.CreateUpDownCounter<long>( "active", description: "Number of active channels" );

    public static readonly Histogram<double> ChannelDuration =
        Meter.CreateHistogram<double>( "duration", "ms", "Channel lifetime duration in milliseconds" );

    public static readonly Counter<long> MiddlewareExceptions =
        Meter.CreateCounter<long>( "middleware.exceptions", description: "Total number of middleware exceptions" );

    public static readonly Counter<long> IdleTimeouts =
        Meter.CreateCounter<long>( "idle.timeouts", description: "Total number of idle timeouts" );
}
