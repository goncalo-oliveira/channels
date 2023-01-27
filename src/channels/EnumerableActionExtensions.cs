internal static class EnumerableActionExtensions
{
    /// <summary>
    /// Encapsulates a method with a single parameter for each item in the collection
    /// </summary>
    public static void InvokeAll<T>( this IEnumerable<T> services, Action<T> action )
    {
        foreach ( var service in services )
        {
            action.Invoke( service );
        }
    }

    /// <summary>
    /// Encapsulates a method with a single parameter for each item in the collection
    /// </summary>
    public static async Task InvokeAllAsync<T>( this IEnumerable<T> services, Func<T, Task> action )
    {
        var tasks = services.Select( service => action.Invoke( service ) );

        await Task.WhenAll( tasks )
            .ConfigureAwait( false );
    }
}
