using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents a custom merged-forward node with outgoing nested content. / 表示包含出站嵌套内容的自定义合并转发节点。</summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public sealed class CustomForwardNodeSendSegment : OneBot11SendSegment
{
    /// <summary>Initializes a custom merged-forward node. / 初始化自定义合并转发节点。</summary>
    public CustomForwardNodeSendSegment(string userId, string nickname, OneBot11SendMessage content)
        : base(MessageSegmentTypes.Node)
    {
        UserId = Require(userId, nameof(userId));
        Nickname = Require(nickname, nameof(nickname));
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    /// <summary>Gets the displayed sender ID. / 获取显示的发送者 ID。</summary>
    public string UserId { get; }

    /// <summary>Gets the displayed nickname. / 获取显示昵称。</summary>
    public string Nickname { get; }

    /// <summary>Gets the outgoing nested message. / 获取出站嵌套消息。</summary>
    public OneBot11SendMessage Content { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject
    {
        ["user_id"] = UserId,
        ["nickname"] = Nickname,
        ["content"] = Content.ToJsonNode()
    };
}
