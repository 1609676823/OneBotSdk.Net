using System;
using System.Text.Json.Nodes;
using OneBotSdk.Net.V12.Json;
using OneBotSdk.Net.V12.Messages;
using OneBotSdk.Net.V12.Responses;

namespace OneBotSdk.Net.V12.Events;

/// <summary>
/// Parses all standard OneBot 12 events with field-level fault isolation and unknown fallbacks.
/// 以字段级故障隔离及未知回退方式解析全部 OneBot 12 标准事件。
/// </summary>
public static class OneBot12EventParser
{
    /// <summary>Parses a detached, extensible event model from a JSON object. / 从 JSON 对象解析独立且可扩展的事件模型。</summary>
    public static OneBot12Event Parse(JsonObject source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        // Clone before dispatch so later caller mutation cannot alter an event already delivered to subscribers.
        // 在分发前创建副本，避免调用方后续修改已经交付给订阅者的事件。
        var rawJson = TolerantJson.CloneObject(source);
        var type = TolerantJson.String(source, "type");
        var detailType = TolerantJson.String(source, "detail_type");

        OneBot12Event result;
        switch (type)
        {
            case OneBot12EventTypes.Message:
                result = ParseMessage(source, rawJson, detailType);
                break;
            case OneBot12EventTypes.Notice:
                result = ParseNotice(source, rawJson, detailType);
                break;
            case OneBot12EventTypes.Request:
                result = new UnknownRequestEvent(rawJson);
                break;
            case OneBot12EventTypes.Meta:
                result = ParseMeta(source, rawJson, detailType);
                break;
            default:
                result = new UnknownOneBot12Event(rawJson);
                break;
        }

        // Every common field is independent; malformed identity or time data cannot hide usable payload fields.
        // 每个公共字段均独立解析；异常身份或时间字段不得遮蔽其它可用负载字段。
        result.Id = TolerantJson.String(source, "id");
        result.Time = TolerantJson.Double(source, "time");
        result.Type = type;
        result.DetailType = detailType;
        result.SubType = TolerantJson.String(source, "sub_type");
        result.Self = TryParse(() => OneBot12Self.Parse(TolerantJson.Node(source, "self")));
        return result;
    }

    private static OneBot12MessageEvent ParseMessage(
        JsonObject source,
        JsonObject rawJson,
        string? detailType)
    {
        OneBot12MessageEvent result;
        switch (detailType)
        {
            case OneBot12EventTypes.Private:
                result = new PrivateMessageEvent(rawJson);
                break;
            case OneBot12EventTypes.Group:
                result = new GroupMessageEvent(rawJson)
                {
                    GroupId = TolerantJson.String(source, "group_id")
                };
                break;
            case OneBot12EventTypes.Channel:
                result = new ChannelMessageEvent(rawJson)
                {
                    GuildId = TolerantJson.String(source, "guild_id"),
                    ChannelId = TolerantJson.String(source, "channel_id")
                };
                break;
            default:
                result = new UnknownMessageEvent(rawJson);
                break;
        }

        result.MessageId = TolerantJson.String(source, "message_id");
        result.Message = TryParse(() => OneBot12ReceivedMessage.Parse(TolerantJson.Node(source, "message")));
        result.AltMessage = TolerantJson.String(source, "alt_message");
        result.UserId = TolerantJson.String(source, "user_id");
        return result;
    }

    private static OneBot12NoticeEvent ParseNotice(
        JsonObject source,
        JsonObject rawJson,
        string? detailType)
    {
        switch (detailType)
        {
            case OneBot12EventTypes.FriendIncrease:
                return new FriendIncreaseNoticeEvent(rawJson)
                {
                    UserId = TolerantJson.String(source, "user_id")
                };
            case OneBot12EventTypes.FriendDecrease:
                return new FriendDecreaseNoticeEvent(rawJson)
                {
                    UserId = TolerantJson.String(source, "user_id")
                };
            case OneBot12EventTypes.PrivateMessageDelete:
                return new PrivateMessageDeleteNoticeEvent(rawJson)
                {
                    MessageId = TolerantJson.String(source, "message_id"),
                    UserId = TolerantJson.String(source, "user_id")
                };
            case OneBot12EventTypes.GroupMemberIncrease:
                return new GroupMemberIncreaseNoticeEvent(rawJson)
                {
                    GroupId = TolerantJson.String(source, "group_id"),
                    UserId = TolerantJson.String(source, "user_id"),
                    OperatorId = TolerantJson.String(source, "operator_id")
                };
            case OneBot12EventTypes.GroupMemberDecrease:
                return new GroupMemberDecreaseNoticeEvent(rawJson)
                {
                    GroupId = TolerantJson.String(source, "group_id"),
                    UserId = TolerantJson.String(source, "user_id"),
                    OperatorId = TolerantJson.String(source, "operator_id")
                };
            case OneBot12EventTypes.GroupMessageDelete:
                return new GroupMessageDeleteNoticeEvent(rawJson)
                {
                    GroupId = TolerantJson.String(source, "group_id"),
                    MessageId = TolerantJson.String(source, "message_id"),
                    UserId = TolerantJson.String(source, "user_id"),
                    OperatorId = TolerantJson.String(source, "operator_id")
                };
            case OneBot12EventTypes.GuildMemberIncrease:
                return new GuildMemberIncreaseNoticeEvent(rawJson)
                {
                    GuildId = TolerantJson.String(source, "guild_id"),
                    UserId = TolerantJson.String(source, "user_id"),
                    OperatorId = TolerantJson.String(source, "operator_id")
                };
            case OneBot12EventTypes.GuildMemberDecrease:
                return new GuildMemberDecreaseNoticeEvent(rawJson)
                {
                    GuildId = TolerantJson.String(source, "guild_id"),
                    UserId = TolerantJson.String(source, "user_id"),
                    OperatorId = TolerantJson.String(source, "operator_id")
                };
            case OneBot12EventTypes.ChannelMemberIncrease:
                return new ChannelMemberIncreaseNoticeEvent(rawJson)
                {
                    GuildId = TolerantJson.String(source, "guild_id"),
                    ChannelId = TolerantJson.String(source, "channel_id"),
                    UserId = TolerantJson.String(source, "user_id"),
                    OperatorId = TolerantJson.String(source, "operator_id")
                };
            case OneBot12EventTypes.ChannelMemberDecrease:
                return new ChannelMemberDecreaseNoticeEvent(rawJson)
                {
                    GuildId = TolerantJson.String(source, "guild_id"),
                    ChannelId = TolerantJson.String(source, "channel_id"),
                    UserId = TolerantJson.String(source, "user_id"),
                    OperatorId = TolerantJson.String(source, "operator_id")
                };
            case OneBot12EventTypes.ChannelMessageDelete:
                return new ChannelMessageDeleteNoticeEvent(rawJson)
                {
                    GuildId = TolerantJson.String(source, "guild_id"),
                    ChannelId = TolerantJson.String(source, "channel_id"),
                    MessageId = TolerantJson.String(source, "message_id"),
                    UserId = TolerantJson.String(source, "user_id"),
                    OperatorId = TolerantJson.String(source, "operator_id")
                };
            case OneBot12EventTypes.ChannelCreate:
                return new ChannelCreateNoticeEvent(rawJson)
                {
                    GuildId = TolerantJson.String(source, "guild_id"),
                    ChannelId = TolerantJson.String(source, "channel_id"),
                    OperatorId = TolerantJson.String(source, "operator_id")
                };
            case OneBot12EventTypes.ChannelDelete:
                return new ChannelDeleteNoticeEvent(rawJson)
                {
                    GuildId = TolerantJson.String(source, "guild_id"),
                    ChannelId = TolerantJson.String(source, "channel_id"),
                    OperatorId = TolerantJson.String(source, "operator_id")
                };
            default:
                return new UnknownNoticeEvent(rawJson);
        }
    }

    private static OneBot12MetaEvent ParseMeta(
        JsonObject source,
        JsonObject rawJson,
        string? detailType)
    {
        switch (detailType)
        {
            case OneBot12EventTypes.Connect:
                return new ConnectMetaEvent(rawJson)
                {
                    Version = TryParse(() => OneBot12VersionData.Parse(TolerantJson.Node(source, "version")))
                };
            case OneBot12EventTypes.Heartbeat:
                return new HeartbeatMetaEvent(rawJson)
                {
                    Interval = TolerantJson.Int64(source, "interval")
                };
            case OneBot12EventTypes.StatusUpdate:
                return new StatusUpdateMetaEvent(rawJson)
                {
                    Status = TryParse(() => OneBot12StatusData.Parse(TolerantJson.Node(source, "status")))
                };
            default:
                return new UnknownMetaEvent(rawJson);
        }
    }

    private static T? TryParse<T>(Func<T?> parser)
        where T : class
    {
        try
        {
            return parser();
        }
        catch (Exception)
        {
            // A nested parser failure is contained within its field and never rejects the whole event.
            // 嵌套解析器失败仅影响对应字段，绝不会拒绝整个事件。
            return null;
        }
    }
}
