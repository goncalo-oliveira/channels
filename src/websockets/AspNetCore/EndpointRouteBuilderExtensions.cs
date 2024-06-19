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
    [RequiresDynamicCode("This API may perform reflection on the supplied delegate and its parameters. These types may require generated code and aren't compatible with native AOT applications.")]
    [RequiresUnreferencedCode("This API may perform reflection on the supplied delegate and its parameters. These types may be trimmed if not directly referenced.")]
    public static RouteHandlerBuilder MapWebSocketChannel( this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern )
        => endpoints.MapGet( pattern, WebSocketChannelMiddleware.InvokeAsync );

    public static RouteHandlerBuilder MapWebSocketChannel( this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern, Action<IChannelPipelineBuilder> configure )
        => endpoints.MapGet( pattern, ( HttpContext httpContext, IWebSocketChannelFactory channelFactory ) =>
        {
            var builder = new ChannelPipelineBuilder( httpContext.RequestServices );

            // add the WebSocket-specific middleware (output adapters and handlers)
            builder.AddWebSocketsMiddleware();

            configure.Invoke( builder );

            return WebSocketChannelMiddleware.InvokeAsync( httpContext, channelFactory, builder );
        } );
}
