using System.Diagnostics.CodeAnalysis;
using Faactory.Channels;
using Faactory.Channels.WebSockets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder;

public static class EndpointRouteBuilderWebSocketChannelExtensions
{
    /// <summary>
    /// Adds a RouteEndpoint to the IEndpointRouteBuilder that matches WebSocket requests for the specified pattern and creates a WebSocket channel using the accepted connection.
    /// </summary>
    /// <param name="pattern">The pattern to match.</param>
    [RequiresDynamicCode("This API may perform reflection on the supplied delegate and its parameters. These types may require generated code and aren't compatible with native AOT applications.")]
    [RequiresUnreferencedCode("This API may perform reflection on the supplied delegate and its parameters. These types may be trimmed if not directly referenced.")]
    public static RouteHandlerBuilder MapWebSocketChannel( this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern )
        => endpoints.MapGet( pattern, WebSocketChannelMiddleware.InvokeAsync );

    /// <summary>
    /// Adds a RouteEndpoint to the IEndpointRouteBuilder that matches WebSocket requests for the specified pattern and creates a WebSocket channel using the accepted connection.
    /// </summary>
    /// <param name="pattern">The pattern to match.</param>
    /// <param name="channelName">The name of the channel to create.</param>
    [RequiresDynamicCode("This API may perform reflection on the supplied delegate and its parameters. These types may require generated code and aren't compatible with native AOT applications.")]
    [RequiresUnreferencedCode("This API may perform reflection on the supplied delegate and its parameters. These types may be trimmed if not directly referenced.")]
    public static RouteHandlerBuilder MapWebSocketChannel( this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern, string channelName )
        => endpoints.MapGet( pattern, ( HttpContext httpContext, IWebSocketChannelFactory channelFactory ) =>
        {
            return WebSocketChannelMiddleware.InvokeNamedAsync( httpContext, channelFactory, channelName );
        } );
}
