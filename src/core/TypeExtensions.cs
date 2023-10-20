using System.Collections;

namespace Faactory.Channels;

internal static class TypeExtensions
{
    public static bool IsEnumerable( this Type type )
        => !type.Equals( typeof( string ) ) && typeof( IEnumerable ).IsAssignableFrom( type );

    public static bool IsEnumerable<T>( this Type type )
        => IsEnumerable( type ) && ( type.GetElementType()?.Equals( typeof( T ) ) == true );

    public static Type? GetEnumerableElementType( this Type type )
    {
        if ( type.HasElementType )
        {
            return type.GetElementType();
        }

        if ( type.IsGenericType )
        {
            return type.GenericTypeArguments.First();
        }

        return default;
    }
}
