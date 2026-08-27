using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Writes concrete outgoing segments in the standard type/data shape. / 以标准 type/data 形态写出具体出站消息段。</summary>
public sealed class OneBot12SendSegmentJsonConverter : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert) => typeof(OneBot12SendSegment).IsAssignableFrom(typeToConvert);

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter)Activator.CreateInstance(typeof(ConcreteConverter<>).MakeGenericType(typeToConvert))!;
    }

    private sealed class ConcreteConverter<TSegment> : JsonConverter<TSegment>
        where TSegment : OneBot12SendSegment
    {
        public override bool HandleNull => true;

        public override TSegment? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            throw new JsonException("Outgoing segments are constructed explicitly; use received models for parsing.");
        }

        public override void Write(Utf8JsonWriter writer, TSegment value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            value.ToJsonObject().WriteTo(writer, options);
        }
    }
}
