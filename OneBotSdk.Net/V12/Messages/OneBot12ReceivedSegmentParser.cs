using System.Text.Json.Nodes;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Messages;

internal static class OneBot12ReceivedSegmentParser
{
    internal static OneBot12ReceivedSegment? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        if (source == null)
        {
            return null;
        }

        var type = TolerantJson.String(source, "type");
        var data = TolerantJson.Object(TolerantJson.Node(source, "data"));

        // A malformed or absent data field becomes an empty object while the full raw segment remains available.
        // data 字段缺失或异常时使用空对象，同时仍保留完整原始消息段。
        var detachedData = data == null ? new JsonObject() : TolerantJson.CloneObject(data);
        var rawJson = TolerantJson.CloneObject(source);

        switch (type)
        {
            case OneBot12MessageSegmentTypes.Text:
                return new OneBot12TextReceivedSegment(type, detachedData, rawJson, TolerantJson.String(data, "text"));
            case OneBot12MessageSegmentTypes.Mention:
                return new OneBot12MentionReceivedSegment(type, detachedData, rawJson, TolerantJson.String(data, "user_id"));
            case OneBot12MessageSegmentTypes.MentionAll:
                return new OneBot12MentionAllReceivedSegment(type, detachedData, rawJson);
            case OneBot12MessageSegmentTypes.Image:
                return new OneBot12ImageReceivedSegment(type, detachedData, rawJson, TolerantJson.String(data, "file_id"));
            case OneBot12MessageSegmentTypes.Voice:
                return new OneBot12VoiceReceivedSegment(type, detachedData, rawJson, TolerantJson.String(data, "file_id"));
            case OneBot12MessageSegmentTypes.Audio:
                return new OneBot12AudioReceivedSegment(type, detachedData, rawJson, TolerantJson.String(data, "file_id"));
            case OneBot12MessageSegmentTypes.Video:
                return new OneBot12VideoReceivedSegment(type, detachedData, rawJson, TolerantJson.String(data, "file_id"));
            case OneBot12MessageSegmentTypes.File:
                return new OneBot12FileReceivedSegment(type, detachedData, rawJson, TolerantJson.String(data, "file_id"));
            case OneBot12MessageSegmentTypes.Location:
                return new OneBot12LocationReceivedSegment(
                    type,
                    detachedData,
                    rawJson,
                    TolerantJson.Double(data, "latitude"),
                    TolerantJson.Double(data, "longitude"),
                    TolerantJson.String(data, "title"),
                    TolerantJson.String(data, "content"));
            case OneBot12MessageSegmentTypes.Reply:
                return new OneBot12ReplyReceivedSegment(
                    type,
                    detachedData,
                    rawJson,
                    TolerantJson.String(data, "message_id"),
                    TolerantJson.String(data, "user_id"));
            default:
                return new OneBot12UnknownReceivedSegment(type, detachedData, rawJson);
        }
    }
}
