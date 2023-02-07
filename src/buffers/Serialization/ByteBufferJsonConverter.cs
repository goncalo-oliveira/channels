using System.Text.Json;
using System.Text.Json.Serialization;

namespace Faactory.Channels.Buffers.Serialization;

/// <summary>
/// Converts a IByteBuffer or value to or from JSON
/// </summary>
public sealed class ByteBufferJsonConverter : JsonConverter<IByteBuffer>
{
    public ByteBufferSerializerFormat Format { get; }

    public ByteBufferJsonConverter( ByteBufferSerializerFormat format = ByteBufferSerializerFormat.Base64 )
    {
        Format = format;
    }

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
            return new WrappedByteBuffer( Convert.FromHexString( value ) );
        }

        return new WrappedByteBuffer( Convert.FromBase64String( value ) );
    }

    public override void Write( Utf8JsonWriter writer, IByteBuffer value, JsonSerializerOptions options )
    {
        if ( Format == ByteBufferSerializerFormat.HexString )
        {
            writer.WriteStringValue( string.Concat( value.ToArray().Select( b => string.Format( "{0:X2}", b ) ) ) );
        }
        else
        {
            writer.WriteStringValue( Convert.ToBase64String( value.ToArray() ) );
        }
    }
}
