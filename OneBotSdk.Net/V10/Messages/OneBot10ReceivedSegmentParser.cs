using System.Text.Json.Nodes;
using OneBotSdk.Net.V10.Json;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>
/// Maps incoming OneBot 10 wire objects to their concrete received segment types.
/// 将入站 OneBot 10 线协议对象映射为具体接收消息段类型。
/// </summary>
internal static class OneBot10ReceivedSegmentParser
{
    /// <summary>
    /// Parses one segment and retains both unknown data fields and top-level implementation extensions.
    /// 解析单个消息段，并保留未知数据字段及顶层实现端扩展。
    /// </summary>
    internal static OneBot10ReceivedSegment? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        if (source == null)
        {
            return null;
        }

        var type = TolerantJson.String(source, "type");
        var data = TolerantJson.Object(TolerantJson.Node(source, "data"));
        OneBot10ReceivedSegment result;
        switch (type)
        {
            case MessageSegmentTypes.Text:
                result = new TextReceivedSegment { Text = TolerantJson.String(data, "text") };
                break;
            case MessageSegmentTypes.Face:
                result = new FaceReceivedSegment { Id = TolerantJson.String(data, "id") };
                break;
            case MessageSegmentTypes.Image:
                result = new ImageReceivedSegment
                {
                    File = TolerantJson.String(data, "file"),
                    Url = TolerantJson.String(data, "url")
                };
                break;
            case MessageSegmentTypes.Record:
                result = new RecordReceivedSegment
                {
                    File = TolerantJson.String(data, "file"),
                    Magic = TolerantJson.Boolean(data, "magic"),
                    Url = TolerantJson.String(data, "url")
                };
                break;
            case MessageSegmentTypes.At:
                result = new AtReceivedSegment { Target = TolerantJson.String(data, "qq") };
                break;
            case MessageSegmentTypes.Rps:
                result = new RpsReceivedSegment();
                break;
            case MessageSegmentTypes.Dice:
                result = new DiceReceivedSegment();
                break;
            case MessageSegmentTypes.Shake:
                result = new ShakeReceivedSegment();
                break;
            case MessageSegmentTypes.Share:
                result = new ShareReceivedSegment
                {
                    Url = TolerantJson.String(data, "url"),
                    Title = TolerantJson.String(data, "title"),
                    Content = TolerantJson.String(data, "content"),
                    Image = TolerantJson.String(data, "image")
                };
                break;
            case MessageSegmentTypes.Contact:
                result = new ContactReceivedSegment
                {
                    ContactType = TolerantJson.String(data, "type"),
                    Id = TolerantJson.String(data, "id")
                };
                break;
            case MessageSegmentTypes.Location:
                result = new LocationReceivedSegment
                {
                    Latitude = TolerantJson.String(data, "lat"),
                    Longitude = TolerantJson.String(data, "lon"),
                    Title = TolerantJson.String(data, "title"),
                    Content = TolerantJson.String(data, "content")
                };
                break;
            case MessageSegmentTypes.Rich:
                result = new RichReceivedSegment();
                break;
            default:
                // Outgoing-only standard types and implementation extensions remain visible but cannot masquerade as received standard types.
                // 仅出站标准类型和实现端扩展仍保持可见，但不能伪装成接收标准类型。
                result = new UnknownReceivedSegment();
                break;
        }

        result.Type = type;
        result.Data = TolerantJson.Clone(data) as JsonObject;
        result.RawJson = TolerantJson.CloneObject(source);
        return result;
    }
}
