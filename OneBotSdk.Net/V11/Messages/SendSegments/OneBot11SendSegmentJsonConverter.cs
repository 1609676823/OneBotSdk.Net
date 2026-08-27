using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>
/// Writes any concrete outgoing segment in the official type/data wire shape.
/// 以官方 type/data 线协议形态写入任意具体出站消息段。
/// </summary>
public sealed class OneBot11SendSegmentJsonConverter : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(OneBot11SendSegment).IsAssignableFrom(typeToConvert);
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(ConcreteConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class ConcreteConverter<TSegment> : JsonConverter<TSegment>
        where TSegment : OneBot11SendSegment
    {
        /// <inheritdoc />
        public override bool HandleNull => true;

        /// <inheritdoc />
        public override TSegment? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            throw new JsonException("Outgoing segments are constructed explicitly; use received segment models for parsing.");
        }

        /// <inheritdoc />
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
