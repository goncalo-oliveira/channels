#if DEBUG

using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo( "tests" )]
[assembly:InternalsVisibleTo( "faactory.channels.websockets" )]
[assembly:InternalsVisibleTo( "faactory.channels.client" )]

#endif
