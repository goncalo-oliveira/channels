
namespace Faactory.Channels;

internal sealed class NullServiceProvider : IServiceProvider
{
    public static NullServiceProvider Instance { get; } = new();

    public object? GetService( Type serviceType ) => null;
}
