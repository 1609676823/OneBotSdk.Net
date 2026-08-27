using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using OneBotSdk.Net.V12.Json;
using OneBotSdk.Net.V12.Messages;
using Xunit;

namespace OneBotSdk.Net.Tests;

[Collection(JsonConfigurationCollection.Name)]
public sealed class OneBot12MessageTests
{
    [Fact]
    public void ReceivedMessage_ParsesEveryStandardSegmentAndUnknownExtension()
    {
        var source = JsonNode.Parse(
            "[" +
            "{\"type\":\"text\",\"data\":{\"text\":\"hello\"}}," +
            "{\"type\":\"mention\",\"data\":{\"user_id\":\"u1\"}}," +
            "{\"type\":\"mention_all\",\"data\":{}}," +
            "{\"type\":\"image\",\"data\":{\"file_id\":\"i1\"}}," +
            "{\"type\":\"voice\",\"data\":{\"file_id\":\"v1\"}}," +
            "{\"type\":\"audio\",\"data\":{\"file_id\":\"a1\"}}," +
            "{\"type\":\"video\",\"data\":{\"file_id\":\"m1\"}}," +
            "{\"type\":\"file\",\"data\":{\"file_id\":\"f1\"}}," +
            "{\"type\":\"location\",\"data\":{\"latitude\":31.2,\"longitude\":121.5,\"title\":\"Shanghai\",\"content\":\"Bund\"}}," +
            "{\"type\":\"reply\",\"data\":{\"message_id\":\"r1\",\"user_id\":\"u2\"}}," +
            "{\"type\":\"vendor.segment\",\"data\":{\"value\":42},\"extension\":true}" +
            "]");

        var message = OneBot12ReceivedMessage.Parse(source);

        Assert.NotNull(message);
        Assert.Equal(11, message!.Count);
        Assert.IsType<OneBot12TextReceivedSegment>(message[0]);
        Assert.IsType<OneBot12MentionReceivedSegment>(message[1]);
        Assert.IsType<OneBot12MentionAllReceivedSegment>(message[2]);
        Assert.IsType<OneBot12ImageReceivedSegment>(message[3]);
        Assert.IsType<OneBot12VoiceReceivedSegment>(message[4]);
        Assert.IsType<OneBot12AudioReceivedSegment>(message[5]);
        Assert.IsType<OneBot12VideoReceivedSegment>(message[6]);
        Assert.IsType<OneBot12FileReceivedSegment>(message[7]);
        Assert.IsType<OneBot12LocationReceivedSegment>(message[8]);
        Assert.IsType<OneBot12ReplyReceivedSegment>(message[9]);
        var unknown = Assert.IsType<OneBot12UnknownReceivedSegment>(message[10]);
        Assert.Equal(42, unknown.Data["value"]!.GetValue<int>());
        Assert.True(unknown.RawJson["extension"]!.GetValue<bool>());
        Assert.Equal("hello", message.PlainText);
    }

    [Fact]
    public void ReceivedMessage_IsolatesMalformedFieldsAndArrayItems()
    {
        var source = JsonNode.Parse(
            "[" +
            "null," +
            "{\"type\":\"location\",\"data\":{\"latitude\":\"bad\",\"longitude\":120,\"title\":42,\"content\":\"ok\"}}," +
            "{\"type\":\"text\",\"data\":{\"text\":\"still parsed\"}}" +
            "]");

        var message = OneBot12ReceivedMessage.Parse(source);

        Assert.NotNull(message);
        Assert.Equal(2, message!.Count);
        var location = Assert.IsType<OneBot12LocationReceivedSegment>(message[0]);
        Assert.Null(location.Latitude);
        Assert.Equal(120d, location.Longitude);
        Assert.Equal("42", location.Title);
        Assert.Equal("ok", location.Content);
        Assert.Equal("still parsed", message.PlainText);
        Assert.Equal(3, message.RawJson.Count);
    }

    [Fact]
    public void SendMessage_WritesStringSingleAndArrayShapes()
    {
        var stringMessage = OneBot12SendMessage.FromString("[CQ:at,qq=all]");
        var singleMessage = OneBot12SendMessage.FromSegment(new OneBot12MentionSendSegment("u1"));
        var arrayMessage = new OneBot12SendMessage()
            .Text("hello")
            .MentionAll()
            .Location(31.2, 121.5, "Shanghai", "Bund")
            .Reply("m1", "u2");

        var stringNode = stringMessage.ToJsonNode();
        var singleNode = Assert.IsType<JsonObject>(singleMessage.ToJsonNode());
        var arrayNode = Assert.IsType<JsonArray>(arrayMessage.ToJsonNode());

        // OneBot 12 string shorthand is plain text and never decodes legacy CQ codes.
        // OneBot 12 的字符串简写只表示纯文本，绝不会解析旧版 CQ 码。
        Assert.Equal("[CQ:at,qq=all]", stringNode!.GetValue<string>());
        Assert.Equal("mention", singleNode["type"]!.GetValue<string>());
        Assert.Equal("u1", singleNode["data"]!["user_id"]!.GetValue<string>());
        Assert.Equal(4, arrayNode.Count);
        Assert.Equal("text", arrayNode[0]!["type"]!.GetValue<string>());
        Assert.Equal("mention_all", arrayNode[1]!["type"]!.GetValue<string>());
        Assert.Equal(31.2, arrayNode[2]!["data"]!["latitude"]!.GetValue<double>());
        Assert.Equal("m1", arrayNode[3]!["data"]!["message_id"]!.GetValue<string>());
    }

    [Fact]
    public void ReceivedMessage_RejectsNonArrayWireShape()
    {
        Assert.Null(OneBot12ReceivedMessage.Parse(JsonValue.Create("text")));
        Assert.Null(OneBot12ReceivedMessage.Parse(new JsonObject
        {
            ["type"] = "text",
            ["data"] = new JsonObject { ["text"] = "not an event message" }
        }));
    }

    [Fact]
    public void JsonConfiguration_UsesSafeDefaultAndExplicitUnsafeOptIn()
    {
        var previous = OneBot12Json.UseUnsafeRelaxedJsonEscaping;
        try
        {
            var value = new JsonObject { ["text"] = "<中文&>" };

            OneBot12Json.UseUnsafeRelaxedJsonEscaping = false;
            var safeOptions = OneBot12Json.CreateSerializerOptions();
            var safeJson = OneBot12Json.Serialize(value);

            OneBot12Json.UseUnsafeRelaxedJsonEscaping = true;
            var relaxedOptions = OneBot12Json.CreateSerializerOptions();
            var relaxedJson = OneBot12Json.Serialize(value);

            Assert.Same(JavaScriptEncoder.Default, safeOptions.Encoder);
            Assert.Same(JavaScriptEncoder.UnsafeRelaxedJsonEscaping, relaxedOptions.Encoder);
            Assert.DoesNotContain("<中文&>", safeJson);
            Assert.Contains("<中文&>", relaxedJson);
            Assert.Equal("<中文&>", OneBot12Json.Parse(relaxedJson)!["text"]!.GetValue<string>());
        }
        finally
        {
            OneBot12Json.UseUnsafeRelaxedJsonEscaping = previous;
        }
    }
}
