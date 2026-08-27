using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Reads and writes concrete received segments through their wire shape. / 通过线协议形态读写具体接收消息段。</summary>
public sealed class OneBot12ReceivedSegmentJsonConverter : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert) => typeof(OneBot12ReceivedSegment).IsAssignableFrom(typeToConvert);

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter)Activator.CreateInstance(typeof(ConcreteConverter<>).MakeGenericType(typeToConvert))!;
    }

    private sealed class ConcreteConverter<TSegment> : JsonConverter<TSegment>
        where TSegment : OneBot12ReceivedSegment
    {
        public override bool HandleNull => true;

        public override TSegment? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            var parsed = OneBot12ReceivedSegment.Parse(JsonNode.Parse(ref reader));
            if (parsed is TSegment typed)
            {
                return typed;
            }

            throw new JsonException("The received segment discriminator does not match the requested type.");
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
