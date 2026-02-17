using System.Text.Json;
using System.Text.Json.Serialization;

namespace Faactory.Channels.Buffers.Serialization;

/// <summary>
/// Converts a IByteBuffer value to or from JSON
/// </summary>
/// <remarks>
/// Initializes a new instance of the ByteBufferJsonConverter class with the specified format.
/// </remarks>
/// <param name="format">The format to use for serialization and deserialization. Default is Base64.</param>
public sealed class ByteBufferJsonConverter( ByteBufferSerializerFormat format = ByteBufferSerializerFormat.Base64 ) : JsonConverter<IByteBuffer>
{
    /// <summary>
    /// Gets the format used for serialization and deserialization.
    /// </summary>
    public ByteBufferSerializerFormat Format { get; } = format;

    /// <summary>
    /// Reads and converts the JSON to type IByteBuffer.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    public override IByteBuffer? Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        if ( reader.TokenType != JsonTokenType.String )
        {
            throw new NotSupportedException( "Expected a string value." );
        }

        var value = reader.GetString();

        if ( value == null )
        {
            return ( null );
        }

        if ( Format == ByteBufferSerializerFormat.HexString )
        {
            return new ReadableByteBuffer( Convert.FromHexString( value ) );
        }

        return new ReadableByteBuffer( Convert.FromBase64String( value ) );
    }

    /// <summary>
    /// Writes a IByteBuffer value as JSON.
    /// </summary>
    public override void Write( Utf8JsonWriter writer, IByteBuffer value, JsonSerializerOptions options )
    {
        if ( Format == ByteBufferSerializerFormat.HexString )
        {
            writer.WriteStringValue( value.ToHexString() );
        }
        else
        {
            writer.WriteStringValue( value.ToBase64String() );
        }
    }
}
