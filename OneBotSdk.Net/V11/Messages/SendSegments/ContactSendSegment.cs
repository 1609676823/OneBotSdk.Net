using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents an outgoing recommended contact. / 表示出站推荐联系人。</summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public sealed class ContactSendSegment : OneBot11SendSegment
{
    /// <summary>Initializes a recommended friend or group. / 初始化推荐好友或群。</summary>
    public ContactSendSegment(OneBot11ContactTarget target, string id) : base(MessageSegmentTypes.Contact)
    {
        if (target != OneBot11ContactTarget.Friend && target != OneBot11ContactTarget.Group)
        {
            throw new ArgumentOutOfRangeException(nameof(target));
        }

        Target = target;
        Id = Require(id, nameof(id));
    }

    /// <summary>Gets the contact target kind. / 获取联系人目标类型。</summary>
    public OneBot11ContactTarget Target { get; }

    /// <summary>Gets the friend or group ID. / 获取好友或群 ID。</summary>
    public string Id { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject
    {
        ["type"] = Target == OneBot11ContactTarget.Friend ? "qq" : "group",
        ["id"] = Id
    };
}
