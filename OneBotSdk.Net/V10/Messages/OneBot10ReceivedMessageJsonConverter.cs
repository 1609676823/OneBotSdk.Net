using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>
/// Reads received message chains while preserving their original wire shape.
/// 读取入站消息链，同时保留其原始线协议形态。
/// </summary>
public sealed class OneBot10ReceivedMessageJsonConverter : JsonConverter<OneBot10ReceivedMessage>
{
    /// <inheritdoc />
    public override bool HandleNull => true;

    /// <inheritdoc />
    public override OneBot10ReceivedMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        try
        {
            return OneBot10ReceivedMessage.Parse(JsonNode.Parse(ref reader));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, OneBot10ReceivedMessage value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        value.ToJsonNode().WriteTo(writer, options);
    }
}
