using System.Text.Json.Serialization;

namespace Faactory.Channels.Buffers.Serialization;

/// <summary>
/// When placed on a property or type, it uses the ByteBufferJsonConverter as the converter type
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface, AllowMultiple = false)]
public sealed class ByteBufferJsonConverterAttribute : JsonConverterAttribute
{
    /// <summary>
    /// Gets or sets the format of the string value. Default is Base64.
    /// </summary>
    public ByteBufferSerializerFormat Format { get; set; } = ByteBufferSerializerFormat.Base64;

    public override JsonConverter? CreateConverter( Type typeToConvert )
        => new ByteBufferJsonConverter( Format );
}
