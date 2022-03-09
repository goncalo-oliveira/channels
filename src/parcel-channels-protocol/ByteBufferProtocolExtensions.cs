using System.Text;
using Parcel.Protocol;

namespace Parcel.Buffers;

internal static class ByteBufferProtocolExtensions
{
    /// <summary>
    /// Reads a Message value
    /// </summary>
    /// <param name="buffer">The source buffer</param>
    /// <returns>A Message value</returns>
    public static Message ReadMessage( this IByteBuffer buffer )
    {
        var message = new Message();

        // read Id and ContentType
        message.Id = buffer.ReadString();
        message.ContentType = buffer.ReadString();

        // read Content length
        var contentLength = buffer.ReadUInt16();

        // read Content
        if ( contentLength > 0 )
        {
            message.Content = buffer.ReadBytes( contentLength );
        }

        // read Signature
        message.Signature = buffer.ReadString();

        return ( message );
    }

    /// <summary>
    /// Reads a string value
    /// </summary>
    /// <param name="buffer">The source buffer</param>
    /// <returns>A string value</returns>
    public static string? ReadString( this IByteBuffer buffer )
    {
        // read length
        var length = buffer.ReadByte();

        if ( length == 0 )
        {
            return ( null );
        }

        // read content
        var bytes = buffer.ReadBytes( length );

        return Encoding.UTF8.GetString( bytes );
    }

    /// <summary>
    /// Writes a Message value
    /// </summary>
    /// <param name="buffer">The source buffer</param>
    /// <param name="message">The message instance to write</param>
    public static void WriteMessage( this IByteBuffer buffer, Message message )
    {
        // write Id and ContentType
        buffer.WriteString( message.Id );
        buffer.WriteString( message.ContentType );

        // write Content
        var length = message.Content?.Length ?? 0;

        if ( length > UInt16.MaxValue )
        {
            throw new ArgumentOutOfRangeException( $"Can't write more than {UInt16.MaxValue} bytes into the 'Content' field." );
        }

        // write Content length
        buffer.WriteUInt16( (ushort)length );

        if ( message.Content?.Any() == true )
        {
            buffer.WriteBytes( message.Content );
        }

        // write Signature
        buffer.WriteString( message.Signature );
    }

    /// <summary>
    /// Writes a string value
    /// </summary>
    /// <param name="buffer">The source buffer</param>
    /// <param name="message">The string value to write</param>
    public static void WriteString( this IByteBuffer buffer, string? value )
    {
        if ( !( value?.Any() == true ) )
        {
            // empty
            buffer.WriteByte( 0x00 );

            return;
        }

        var bytes = Encoding.UTF8.GetBytes( value );

        if ( bytes.Length > byte.MaxValue )
        {
            throw new ArgumentOutOfRangeException( $"Can't write more than {byte.MaxValue} bytes into a string field." );
        }

        buffer.WriteByte( (byte)bytes.Length );
        buffer.WriteBytes( bytes );
    }
}
