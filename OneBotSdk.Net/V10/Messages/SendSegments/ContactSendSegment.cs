using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>Represents an outgoing recommended contact. / 表示出站推荐联系人。</summary>
[JsonConverter(typeof(OneBot10SendSegmentJsonConverter))]
public sealed class ContactSendSegment : OneBot10SendSegment
{
    /// <summary>Initializes a recommended friend or group. / 初始化推荐好友或群。</summary>
    public ContactSendSegment(OneBot10ContactTarget target, string id) : base(MessageSegmentTypes.Contact)
    {
        if (target != OneBot10ContactTarget.Friend && target != OneBot10ContactTarget.Group)
        {
            throw new ArgumentOutOfRangeException(nameof(target));
        }

        Target = target;
        Id = Require(id, nameof(id));
    }

    /// <summary>Gets the contact target kind. / 获取联系人目标类型。</summary>
    public OneBot10ContactTarget Target { get; }

    /// <summary>Gets the friend or group ID. / 获取好友或群 ID。</summary>
    public string Id { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject
    {
        ["type"] = Target == OneBot10ContactTarget.Friend ? "qq" : "group",
        ["id"] = Id
    };
}
