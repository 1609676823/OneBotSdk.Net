using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Json;
using OneBotSdk.Net.V11.Messages;

namespace OneBotSdk.Net.V11.Responses;

/// <summary>
/// Contains a merged-forward message returned by <c>get_forward_msg</c>.
/// 包含 <c>get_forward_msg</c> 返回的合并转发消息。
/// </summary>
public sealed class OneBot11ForwardMessageData : OneBot11JsonModel
{
    internal static OneBot11ForwardMessageData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        if (source == null)
        {
            return null;
        }

        return new OneBot11ForwardMessageData
        {
            RawJson = TolerantJson.CloneObject(source),
            MessageChain = TolerantJson.Parse(source, "message", OneBot11ReceivedForwardMessage.Parse) ??
                           OneBot11ReceivedForwardMessage.Empty
        };
    }

    /// <summary>
    /// Gets the merged-forward message chain whose segments are normally custom nodes.
    /// 获取通常由自定义节点消息段组成的合并转发消息链。
    /// </summary>
    [JsonPropertyName("message")]
    public OneBot11ReceivedForwardMessage MessageChain { get; private set; } = OneBot11ReceivedForwardMessage.Empty;
}
