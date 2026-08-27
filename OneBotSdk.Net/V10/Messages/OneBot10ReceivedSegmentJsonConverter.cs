using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>
/// Reads and writes concrete received segments through their official type/data shape.
/// 通过官方 type/data 形态读写具体入站消息段。
/// </summary>
public sealed class OneBot10ReceivedSegmentJsonConverter : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(OneBot10ReceivedSegment).IsAssignableFrom(typeToConvert);
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(ConcreteConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class ConcreteConverter<TSegment> : JsonConverter<TSegment>
        where TSegment : OneBot10ReceivedSegment
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

            var parsed = OneBot10ReceivedSegment.Parse(JsonNode.Parse(ref reader));
            if (parsed is TSegment typed)
            {
                return typed;
            }

            throw new JsonException("The incoming segment discriminator does not match the requested received type.");
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
