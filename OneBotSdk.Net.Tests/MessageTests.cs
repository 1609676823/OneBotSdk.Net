using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using OneBotSdk.Net.V11.Json;
using OneBotSdk.Net.V11.Messages;
using Xunit;

namespace OneBotSdk.Net.Tests;

public sealed class MessageTests
{
    [Fact]
    public void CqCodec_RoundTripsEscapedTextAndParameterCharacters()
    {
        var original = OneBot11Message.FromSegments(
            MessageSegment.Text("A&[B]"),
            new MessageSegment("extension", new JsonObject
            {
                ["value"] = "a,b&[]=x"
            }));

        var encoded = CqCodeCodec.Encode(original);
        var decoded = CqCodeCodec.Decode(encoded);

        Assert.Equal("A&amp;&#91;B&#93;[CQ:extension,value=a&#44;b&amp;&#91;&#93;=x]", encoded);
        Assert.Equal("A&[B]", decoded.Segments[0].GetString("text"));
        Assert.Equal("extension", decoded.Segments[1].Type);
        Assert.Equal("a,b&[]=x", decoded.Segments[1].GetString("value"));
    }

    [Fact]
    public void CqCodec_SplitsAParameterOnlyAtItsFirstEqualsSign()
    {
        var decoded = CqCodeCodec.Decode("[CQ:json,data={\"a\":\"x=y\"}]");

        Assert.Single(decoded.Segments);
        Assert.Equal("json", decoded.Segments[0].Type);
        Assert.Equal("{\"a\":\"x=y\"}", decoded.Segments[0].GetString("data"));
    }

    [Fact]
    public void MessageParser_IsolatesMalformedArrayElementsAndPreservesUnknownSegments()
    {
        var source = JsonNode.Parse("""
            [
              { "type": "text", "data": { "text": "hello" } },
              123,
              { "type": "vendor_extension", "data": null, "extra": true }
            ]
            """);

        var message = OneBot11Message.Parse(source);

        Assert.NotNull(message);
        Assert.Equal(OneBot11MessageKind.SegmentArray, message!.Kind);
        Assert.Equal(2, message.Segments.Count);
        Assert.Equal("hello", message.Segments[0].GetString("text"));
        Assert.Equal("vendor_extension", message.Segments[1].Type);
        Assert.Null(message.Segments[1].Data);
        Assert.Equal(3, message.RawJson!.AsArray().Count);
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("yes", true)]
    [InlineData("true", true)]
    [InlineData("0", false)]
    [InlineData("no", false)]
    [InlineData("false", false)]
    public void SegmentBooleanReader_AcceptsAllProtocolSpellings(string value, bool expected)
    {
        var segment = new MessageSegment("extension", new JsonObject { ["enabled"] = value });
        Assert.Equal(expected, segment.GetBoolean("enabled"));
    }

    [Fact]
    public void MessageJsonConverter_RetainsTheChosenWireShape()
    {
        var text = OneBot11Message.FromString("plain");
        var single = OneBot11Message.FromSegment(MessageSegment.Face("14"));
        var chain = OneBot11Message.FromSegments(MessageSegment.Text("hello"), MessageSegment.At("all"));

        Assert.Equal("\"plain\"", JsonSerializer.Serialize(text));
        Assert.Equal(JsonValueKind.Object, JsonDocument.Parse(JsonSerializer.Serialize(single)).RootElement.ValueKind);
        Assert.Equal(JsonValueKind.Array, JsonDocument.Parse(JsonSerializer.Serialize(chain)).RootElement.ValueKind);
    }

    [Fact]
    public void JsonSegment_StoresJsonAsAStringPerTheStandard()
    {
        var segment = MessageSegment.Json("{\"app\":\"demo\"}");
        Assert.Equal("{\"app\":\"demo\"}", segment.GetString("data"));
        Assert.IsAssignableFrom<JsonValue>(segment.Data!["data"]);
    }

    [Fact]
    public void MessageChain_SupportsCollectionFluentBuildingAndDirectMessageConversion()
    {
        var chain = new OneBot11MessageChain
        {
            MessageSegment.Text("hello ")
        };
        chain.At(123456789L)
            .Image("https://example.test/image.png")
            .Text(" done");

        OneBot11Message message = chain;
        var json = JsonNode.Parse(JsonSerializer.Serialize(message))!.AsArray();

        Assert.Equal(4, chain.Count);
        Assert.Equal("hello  done", chain.PlainText);
        Assert.Equal(OneBot11MessageSegmentKind.At, chain[1].Kind);
        Assert.Equal(OneBot11MessageSegmentKind.Image, chain[2].Kind);
        Assert.Equal("123456789", chain.FirstOrDefault(MessageSegmentTypes.At)!.GetString("qq"));
        Assert.Equal(4, json.Count);
    }

    [Fact]
    public void MessageChain_NormalizesReceivedCqStringsAndRetainsUnknownSegmentKinds()
    {
        var message = OneBot11Message.FromString("hello[CQ:face,id=14][CQ:vendor,value=x]");
        var chain = OneBot11MessageChain.FromMessage(message);

        Assert.Equal(3, chain.Count);
        Assert.Equal("hello", chain.PlainText);
        Assert.Equal(OneBot11MessageSegmentKind.Face, chain[1].Kind);
        Assert.Equal(OneBot11MessageSegmentKind.Unknown, chain[2].Kind);
        Assert.Equal("vendor", chain[2].Type);
        Assert.Equal("x", chain[2].GetString("value"));
    }

    [Fact]
    public void MessageChain_DeepCopiesInboundAndOutboundSegments()
    {
        var sourceSegment = new MessageSegment("vendor", new JsonObject
        {
            ["nested"] = new JsonObject { ["value"] = "original" }
        });
        var sourceMessage = OneBot11Message.FromSegments(sourceSegment);
        var chain = OneBot11MessageChain.FromMessage(sourceMessage);

        chain[0].Type = "changed";
        chain[0].Data!["nested"]!["value"] = "chain";
        var outbound = chain.ToMessage();
        chain[0].Data!["nested"]!["value"] = "changed-after-conversion";

        Assert.Equal("vendor", sourceMessage.Segments[0].Type);
        Assert.Equal("original", sourceMessage.Segments[0].Data!["nested"]!["value"]!.GetValue<string>());
        Assert.Equal("changed", outbound.Segments[0].Type);
        Assert.Equal("chain", outbound.Segments[0].Data!["nested"]!["value"]!.GetValue<string>());
    }

    [Fact]
    public void SendMessage_UsesIndependentConcreteClassesAndOnlyWritesOutgoingFields()
    {
        var message = new OneBot11SendMessage
        {
            new TextSendSegment("hello"),
            new ImageSendSegment(
                "https://example.test/image.png",
                flash: true,
                cache: false,
                proxy: true,
                timeoutSeconds: 12),
            new PokeSendSegment("126", "2003"),
            new AnonymousSendSegment(ignoreFailure: true)
        };

        var json = JsonNode.Parse(JsonSerializer.Serialize(message))!.AsArray();
        var imageData = json[1]!["data"]!.AsObject();
        var pokeData = json[2]!["data"]!.AsObject();

        Assert.IsType<TextSendSegment>(message[0]);
        Assert.Equal("flash", imageData["type"]!.GetValue<string>());
        Assert.Equal("0", imageData["cache"]!.GetValue<string>());
        Assert.Equal("1", imageData["proxy"]!.GetValue<string>());
        Assert.Equal("12", imageData["timeout"]!.GetValue<string>());
        Assert.False(imageData.ContainsKey("url"));
        Assert.False(pokeData.ContainsKey("name"));
        Assert.Equal("1", json[3]!["data"]!["ignore"]!.GetValue<string>());
    }

    [Fact]
    public void SendMessage_SerializesAllThreeOfficialApiParameterShapes()
    {
        var text = OneBot11SendMessage.FromString("plain[CQ:at,qq=all]");
        var single = OneBot11SendMessage.FromSegment(new FaceSendSegment(14));
        var array = OneBot11SendMessage.FromSegments(
            new TextSendSegment("hello"),
            new AtSendSegment("all"));

        Assert.Equal("\"plain[CQ:at,qq=all]\"", JsonSerializer.Serialize(text));
        Assert.Equal(JsonValueKind.Object, JsonDocument.Parse(JsonSerializer.Serialize(single)).RootElement.ValueKind);
        Assert.Equal(JsonValueKind.Array, JsonDocument.Parse(JsonSerializer.Serialize(array)).RootElement.ValueKind);
    }

    [Fact]
    public void SendSegment_DirectSerializationUsesWireShapeForBaseAndConcreteStaticTypes()
    {
        var concrete = new ImageSendSegment("image.jpg", cache: true);
        OneBot11SendSegment asBase = concrete;

        var concreteJson = JsonNode.Parse(JsonSerializer.Serialize(concrete))!.AsObject();
        var baseJson = JsonNode.Parse(JsonSerializer.Serialize(asBase))!.AsObject();

        Assert.Equal("image", concreteJson["type"]!.GetValue<string>());
        Assert.Equal("image.jpg", concreteJson["data"]!["file"]!.GetValue<string>());
        Assert.Equal("1", concreteJson["data"]!["cache"]!.GetValue<string>());
        Assert.Equal(OneBot11Json.Serialize(concreteJson), OneBot11Json.Serialize(baseJson));
        Assert.False(concreteJson.ContainsKey("File"));
    }

    [Fact]
    public void ReceivedMessage_ParsesConcreteReceiveTypesAndRetainsUnknownSegments()
    {
        var source = JsonNode.Parse("""
            [
              { "type": "text", "data": { "text": "hello" } },
              { "type": "image", "data": { "file": "a.jpg", "type": "flash", "url": "https://example.test/a.jpg", "vendor": 7 } },
              { "type": "poke", "data": { "type": "126", "id": "2003", "name": "poke name" } },
              { "type": "forward", "data": { "id": "forward-id" } },
              { "type": "music", "data": { "type": "qq", "id": "1" } },
              { "type": "vendor", "data": { "usable": true } },
              42
            ]
            """);

        var message = OneBot11ReceivedMessage.Parse(source);

        Assert.NotNull(message);
        Assert.Equal(6, message!.Count);
        Assert.Equal("hello", Assert.IsType<TextReceivedSegment>(message[0]).Text);
        var image = Assert.IsType<ImageReceivedSegment>(message[1]);
        Assert.Equal("a.jpg", image.File);
        Assert.Equal("flash", image.ImageType);
        Assert.Equal("https://example.test/a.jpg", image.Url);
        Assert.Equal(7, image.Data!["vendor"]!.GetValue<int>());
        Assert.Equal("poke name", Assert.IsType<PokeReceivedSegment>(message[2]).Name);
        Assert.Equal("forward-id", Assert.IsType<ForwardReceivedSegment>(message[3]).ForwardId);
        Assert.IsType<UnknownReceivedSegment>(message[4]);
        Assert.Equal("vendor", Assert.IsType<UnknownReceivedSegment>(message[5]).Type);
    }

    [Fact]
    public void ReceivedSegment_DirectJsonParsingAndWritingPreserveConcreteTypeAndExtensions()
    {
        const string json = "{\"type\":\"image\",\"data\":{\"file\":\"a.jpg\",\"url\":\"https://example.test/a.jpg\",\"vendor\":7},\"root_extension\":true}";

        OneBot11ReceivedSegment? asBase = JsonSerializer.Deserialize<OneBot11ReceivedSegment>(json);
        ImageReceivedSegment? concrete = JsonSerializer.Deserialize<ImageReceivedSegment>(json);
        var serialized = JsonNode.Parse(JsonSerializer.Serialize(concrete))!.AsObject();

        Assert.IsType<ImageReceivedSegment>(asBase);
        Assert.Equal("https://example.test/a.jpg", concrete!.Url);
        Assert.Equal(7, serialized["data"]!["vendor"]!.GetValue<int>());
        Assert.True(serialized["root_extension"]!.GetValue<bool>());
        Assert.False(serialized.ContainsKey("Url"));
        Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<TextReceivedSegment>(json));
    }

    [Fact]
    public void ReceivedMessage_NormalizesCqCodeIntoConcreteReadModelsWithoutChangingWireKind()
    {
        var message = OneBot11ReceivedMessage.Parse(
            JsonValue.Create("hello[CQ:image,file=a.jpg,url=https://example.test/a.jpg]"));

        Assert.NotNull(message);
        Assert.Equal(OneBot11ReceivedMessageKind.String, message!.Kind);
        Assert.Equal("hello", message.PlainText);
        Assert.Equal("https://example.test/a.jpg", Assert.IsType<ImageReceivedSegment>(message[1]).Url);
        Assert.Equal("hello[CQ:image,file=a.jpg,url=https://example.test/a.jpg]", message.StringValue);
    }

    [Fact]
    public void ForwardMessage_ParsesOnlyCustomNodesWithNestedReceivedContent()
    {
        var source = JsonNode.Parse("""
            [
              {
                "type": "node",
                "data": {
                  "user_id": "10001",
                  "nickname": "tester",
                  "content": [
                    { "type": "text", "data": { "text": "nested" } }
                  ]
                }
              },
              { "type": "node", "data": { "id": "send-only-reference" } },
              { "type": "text", "data": { "text": "invalid sibling" } }
            ]
            """);

        var forward = OneBot11ReceivedForwardMessage.Parse(source);

        var node = Assert.Single(forward!);
        Assert.Equal("10001", node.UserId);
        Assert.Equal("tester", node.Nickname);
        Assert.Equal("nested", node.Content!.PlainText);
        Assert.Equal(3, forward!.RawJson.Count);
    }

    [Fact]
    public void ReceivedToSendConversion_IsExplicitDropsReceiveMetadataAndRejectsForwardReferences()
    {
        var reusableImage = OneBot11ReceivedMessage.Parse(JsonNode.Parse("""
            [
              { "type": "image", "data": { "file": "received.jpg", "url": "https://private.example/received.jpg" } }
            ]
            """));
        var forward = OneBot11ReceivedMessage.Parse(JsonNode.Parse("""
            [
              { "type": "forward", "data": { "id": "forward-id" } }
            ]
            """));
        var cqForward = OneBot11ReceivedMessage.Parse(JsonValue.Create("[CQ:forward,id=forward-id]"));
        var cqImage = OneBot11ReceivedMessage.Parse(
            JsonValue.Create("[CQ:image,file=received.jpg,url=https://private.example/received.jpg]"));

        Assert.True(reusableImage!.TryToSendMessage(out var outgoing));
        var imageData = outgoing!.ToJsonNode()![0]!["data"]!.AsObject();
        Assert.Equal("received.jpg", imageData["file"]!.GetValue<string>());
        Assert.False(imageData.ContainsKey("url"));
        Assert.False(forward!.TryToSendMessage(out var rejected));
        Assert.Null(rejected);
        Assert.False(cqForward!.TryToSendMessage(out var rejectedCq));
        Assert.Null(rejectedCq);
        Assert.True(cqImage!.TryToSendMessage(out var outgoingCqImage));
        var cqImageData = outgoingCqImage!.ToJsonNode()![0]!["data"]!.AsObject();
        Assert.Equal("received.jpg", cqImageData["file"]!.GetValue<string>());
        Assert.False(cqImageData.ContainsKey("url"));
    }

    [Fact]
    public void ReceivedShakeSegment_ArraySupportsTypedQueriesRawJsonAndExplicitSendConversion()
    {
        var source = JsonNode.Parse("""
            [
              {
                "type": "shake",
                "data": { "vendor_trace": "array-trace" },
                "root_extension": 7
              }
            ]
            """);

        var message = OneBot11ReceivedMessage.Parse(source)!;
        var byOfType = Assert.Single(message.OfType<ShakeReceivedSegment>());
        var byGetSegments = Assert.Single(message.GetSegments<ShakeReceivedSegment>());

        Assert.Same(byOfType, byGetSegments);
        Assert.Equal(OneBot11MessageSegmentKind.Shake, byOfType.Kind);
        Assert.Equal("array-trace", byOfType.RawJson["data"]!["vendor_trace"]!.GetValue<string>());
        Assert.Equal(7, byOfType.RawJson["root_extension"]!.GetValue<int>());
        Assert.True(message.TryToSendMessage(out var outgoing));
        Assert.IsType<ShakeSendSegment>(Assert.Single(outgoing!));
        Assert.Empty(outgoing!.ToJsonNode()![0]!["data"]!.AsObject());
    }

    [Fact]
    public void ReceivedShakeSegment_CqStringSupportsTypedQueriesAndRetainsNormalizedRawJson()
    {
        var message = OneBot11ReceivedMessage.Parse(
            JsonValue.Create("before[CQ:shake,vendor_trace=cq-trace]after"))!;

        var byOfType = Assert.Single(message.OfType<ShakeReceivedSegment>());
        var byGetSegments = Assert.Single(message.GetSegments<ShakeReceivedSegment>());

        Assert.Same(byOfType, byGetSegments);
        Assert.Equal(OneBot11ReceivedMessageKind.String, message.Kind);
        Assert.Equal(OneBot11MessageSegmentKind.Shake, byOfType.Kind);
        Assert.Equal("shake", byOfType.RawJson["type"]!.GetValue<string>());
        Assert.Equal("cq-trace", byOfType.RawJson["data"]!["vendor_trace"]!.GetValue<string>());
    }

    [Fact]
    public void SendSegmentCatalog_SerializesEveryOfficialOutgoingWireType()
    {
        var nested = new OneBot11SendMessage { new TextSendSegment("nested") };
        var message = new OneBot11SendMessage
        {
            new TextSendSegment("text"),
            new FaceSendSegment(14),
            new ImageSendSegment("image.jpg"),
            new RecordSendSegment("record.amr"),
            new VideoSendSegment("video.mp4"),
            new AtSendSegment(10001),
            new RpsSendSegment(),
            new DiceSendSegment(),
            new ShakeSendSegment(),
            new PokeSendSegment("126", "2003"),
            new AnonymousSendSegment(),
            new ShareSendSegment("https://example.test", "title"),
            new ContactSendSegment(OneBot11ContactTarget.Friend, "10001"),
            new ContactSendSegment(OneBot11ContactTarget.Group, "20001"),
            new LocationSendSegment("39.9", "116.3"),
            new MusicSendSegment(OneBot11MusicProvider.NetEase, "28949129"),
            new CustomMusicSendSegment("https://example.test", "https://example.test/a.mp3", "music"),
            new ReplySendSegment(12),
            new ForwardNodeSendSegment(13),
            new CustomForwardNodeSendSegment("10001", "tester", nested),
            new XmlSendSegment("<msg />"),
            new JsonSendSegment("{\"app\":1}"),
            new CustomSendSegment("vendor", new JsonObject { ["value"] = "x" })
        };

        var json = message.ToJsonNode()!.AsArray();
        var types = new string[json.Count];
        for (var index = 0; index < json.Count; index++)
        {
            types[index] = json[index]!["type"]!.GetValue<string>();
        }

        Assert.Equal(
            new[]
            {
                "text", "face", "image", "record", "video", "at", "rps", "dice", "shake", "poke",
                "anonymous", "share", "contact", "contact", "location", "music", "music", "reply", "node",
                "node", "xml", "json", "vendor"
            },
            types);
        Assert.DoesNotContain("forward", types);
        Assert.Equal("163", json[15]!["data"]!["type"]!.GetValue<string>());
        Assert.Equal("text", json[19]!["data"]!["content"]![0]!["type"]!.GetValue<string>());
    }

    [Fact]
    public void ReceivedSegmentCatalog_MapsEveryOfficialReceiveCapableWireType()
    {
        var source = JsonNode.Parse("""
            [
              { "type": "text", "data": { "text": "x" } },
              { "type": "face", "data": { "id": "14" } },
              { "type": "image", "data": { "file": "a.jpg", "url": "https://example.test/a.jpg" } },
              { "type": "record", "data": { "file": "a.amr", "magic": "1", "url": "https://example.test/a.amr" } },
              { "type": "video", "data": { "file": "a.mp4", "url": "https://example.test/a.mp4" } },
              { "type": "at", "data": { "qq": "all" } },
              { "type": "rps", "data": {} },
              { "type": "dice", "data": {} },
              { "type": "shake", "data": {} },
              { "type": "poke", "data": { "type": "126", "id": "2003", "name": "poke" } },
              { "type": "share", "data": { "url": "https://example.test", "title": "title" } },
              { "type": "contact", "data": { "type": "group", "id": "20001" } },
              { "type": "location", "data": { "lat": "39.9", "lon": "116.3" } },
              { "type": "reply", "data": { "id": "12" } },
              { "type": "forward", "data": { "id": "forward-id" } },
              { "type": "node", "data": { "user_id": "10001", "nickname": "tester", "content": "nested" } },
              { "type": "xml", "data": { "data": "<msg />" } },
              { "type": "json", "data": { "data": "{\"app\":1}" } }
            ]
            """);

        var message = OneBot11ReceivedMessage.Parse(source)!;

        Assert.Collection(
            message,
            segment => Assert.IsType<TextReceivedSegment>(segment),
            segment => Assert.IsType<FaceReceivedSegment>(segment),
            segment => Assert.IsType<ImageReceivedSegment>(segment),
            segment => Assert.IsType<RecordReceivedSegment>(segment),
            segment => Assert.IsType<VideoReceivedSegment>(segment),
            segment => Assert.IsType<AtReceivedSegment>(segment),
            segment => Assert.IsType<RpsReceivedSegment>(segment),
            segment => Assert.IsType<DiceReceivedSegment>(segment),
            segment => Assert.IsType<ShakeReceivedSegment>(segment),
            segment => Assert.IsType<PokeReceivedSegment>(segment),
            segment => Assert.IsType<ShareReceivedSegment>(segment),
            segment => Assert.IsType<ContactReceivedSegment>(segment),
            segment => Assert.IsType<LocationReceivedSegment>(segment),
            segment => Assert.IsType<ReplyReceivedSegment>(segment),
            segment => Assert.IsType<ForwardReceivedSegment>(segment),
            segment => Assert.IsType<ForwardNodeReceivedSegment>(segment),
            segment => Assert.IsType<XmlReceivedSegment>(segment),
            segment => Assert.IsType<JsonReceivedSegment>(segment));
        Assert.True(Assert.IsType<RecordReceivedSegment>(message[3]).Magic);
        Assert.Equal("nested", Assert.IsType<ForwardNodeReceivedSegment>(message[15]).Content!.PlainText);
    }
}
