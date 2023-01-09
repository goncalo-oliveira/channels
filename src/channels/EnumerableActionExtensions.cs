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
}
