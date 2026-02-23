using System.Text.Json;
using System.Text.Json.Serialization;

namespace Faactory.Channels.Buffers.Serialization;

/// <summary>
/// A JSON converter factory for IByteBuffer types, allowing serialization and deserialization of byte buffers in either Base64 or Hex string formats.
/// </summary>
/// <param name="format">The format to use for serialization and deserialization. Default is Base64.</param>
public sealed class ByteBufferJsonConverterFactory( ByteBufferSerializerFormat format = ByteBufferSerializerFormat.Base64 ) : JsonConverterFactory
{
    /// <summary>
    /// Determines whether the converter can convert the specified type. It checks if the type is assignable from IByteBuffer.
    /// </summary>
    /// <param name="typeToConvert">The type to check for compatibility with IByteBuffer.</param>
    /// <returns>True if the type can be converted; otherwise, false.</returns>
    public override bool CanConvert( Type typeToConvert )
        => typeof(IByteBuffer).IsAssignableFrom( typeToConvert );

    /// <summary>
    /// Creates a JSON converter for the specified type. It generates a converter for types that implement IByteBuffer, using the specified format for serialization and deserialization.
    /// </summary>
    /// <param name="typeToConvert">The type to create a converter for.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>A JSON converter for the specified type.</returns>
    public override JsonConverter CreateConverter( Type typeToConvert, JsonSerializerOptions options )
    {
        var converterType = typeof( ByteBufferJsonConverter<> )
            .MakeGenericType( typeToConvert );

        return (JsonConverter)Activator.CreateInstance( converterType, format )!;
    }

    private sealed class ByteBufferJsonConverter<TBuffer>( ByteBufferSerializerFormat format ) : JsonConverter<TBuffer> where TBuffer : class, IByteBuffer
    {
        public override TBuffer? Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
        {
            if ( typeToConvert == typeof( IWritableByteBuffer ) )
            {
                throw new NotSupportedException(
                    "Deserialization to IWritableByteBuffer is not supported."
                );
            }

            if ( reader.TokenType != JsonTokenType.String )
            {
                throw new NotSupportedException( "Expected a string." );
            }

            var value = reader.GetString();

            if ( value is null )
            {
                return null;
            }

            var bytes = format == ByteBufferSerializerFormat.HexString
                ? Convert.FromHexString(value)
                : Convert.FromBase64String(value);

            return new ReadableByteBuffer(bytes) as TBuffer;
        }

        public override void Write( Utf8JsonWriter writer, TBuffer value, JsonSerializerOptions options )
        {
            writer.WriteStringValue(
                format == ByteBufferSerializerFormat.HexString
                    ? value.ToHexString()
                    : value.ToBase64String()
            );
        }
    }
}
