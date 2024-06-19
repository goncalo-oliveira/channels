# Channels over WebSockets

An extension library to use the Channels middleware with WebSockets.

## Design

Unlike TCP or UDP Channels, the library does not provide a low-level server to handle WebSocket connections. Instead, it provides a pipeline builder to create a Channel and its associated middleware over an existing WebSocket connection. While this allows the library to be used with any HTTP server that can produce a WebSocket connection, the library does provide extension methods for the `Microsoft.AspNetCore.Http` namespace to make it super easy to use with the ASP.NET Core middleware.
