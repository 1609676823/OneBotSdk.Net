using System;
using System.Text.Json.Nodes;
using OneBotSdk.Net.V11.Json;
using OneBotSdk.Net.V11.Messages;

namespace OneBotSdk.Net.V11.Events;

/// <summary>
/// Parses standard and extension OneBot 11 events with field-level fault isolation.
/// 以字段级故障隔离方式解析标准和扩展 OneBot 11 事件。
/// </summary>
public static class OneBot11EventParser
{
    /// <summary>
    /// Parses an event object and returns an unknown fallback instead of rejecting unknown discriminators.
    /// 解析事件对象；遇到未知判别值时返回未知回退类型，而不是拒绝事件。
    /// </summary>
    public static OneBot11Event Parse(JsonObject source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var postType = TolerantJson.String(source, "post_type");
        OneBot11Event result;
        switch (postType)
        {
            case OneBot11EventTypes.Message:
                result = ParseMessage(source);
                break;
            case OneBot11EventTypes.Notice:
                result = ParseNotice(source);
                break;
            case OneBot11EventTypes.Request:
                result = ParseRequest(source);
                break;
            case OneBot11EventTypes.MetaEvent:
                result = ParseMetaEvent(source);
                break;
            default:
                result = new UnknownOneBot11Event();
                break;
        }

        // Common fields and raw JSON are assigned after subtype parsing so every fallback gets them.
        // 在子类型解析后统一赋公共字段和原始 JSON，确保所有回退类型均能获得这些信息。
        result.Time = TolerantJson.Int64(source, "time");
        result.SelfId = TolerantJson.Int64(source, "self_id");
        result.PostType = postType;
        result.RawJson = TolerantJson.CloneObject(source);
        return result;
    }

    private static OneBot11MessageEvent ParseMessage(JsonObject source)
    {
        var messageType = TolerantJson.String(source, "message_type");
        OneBot11MessageEvent result;
        switch (messageType)
        {
            case OneBot11EventTypes.PrivateMessage:
                result = new PrivateMessageEvent
                {
                    Sender = TolerantJson.Parse(source, "sender", node => ParsePrivateSender(TolerantJson.Object(node)))
                };
                break;
            case OneBot11EventTypes.GroupMessage:
                result = new GroupMessageEvent
                {
                    GroupId = TolerantJson.Int64(source, "group_id"),
                    Anonymous = TolerantJson.Parse(source, "anonymous", node => ParseAnonymous(TolerantJson.Object(node))),
                    Sender = TolerantJson.Parse(source, "sender", node => ParseGroupSender(TolerantJson.Object(node)))
                };
                break;
            default:
                result = new UnknownMessageEvent();
                break;
        }

        result.MessageType = messageType;
        result.SubType = TolerantJson.String(source, "sub_type");
        result.MessageId = TolerantJson.Int64(source, "message_id");
        result.UserId = TolerantJson.Int64(source, "user_id");
        result.MessageChain = TolerantJson.Parse(source, "message", OneBot11ReceivedMessage.Parse) ??
                              OneBot11ReceivedMessage.Empty;
        result.RawMessage = TolerantJson.String(source, "raw_message");
        result.Font = TolerantJson.Int64(source, "font");
        return result;
    }

    private static OneBot11NoticeEvent ParseNotice(JsonObject source)
    {
        var noticeType = TolerantJson.String(source, "notice_type");
        OneBot11NoticeEvent result;
        switch (noticeType)
        {
            case OneBot11EventTypes.GroupUpload:
                result = new GroupUploadNoticeEvent
                {
                    GroupId = TolerantJson.Int64(source, "group_id"),
                    UserId = TolerantJson.Int64(source, "user_id"),
                    File = TolerantJson.Parse(source, "file", node => ParseUploadFile(TolerantJson.Object(node)))
                };
                break;
            case OneBot11EventTypes.GroupAdmin:
                result = new GroupAdminNoticeEvent
                {
                    SubType = TolerantJson.String(source, "sub_type"),
                    GroupId = TolerantJson.Int64(source, "group_id"),
                    UserId = TolerantJson.Int64(source, "user_id")
                };
                break;
            case OneBot11EventTypes.GroupDecrease:
                result = new GroupDecreaseNoticeEvent
                {
                    SubType = TolerantJson.String(source, "sub_type"),
                    GroupId = TolerantJson.Int64(source, "group_id"),
                    OperatorId = TolerantJson.Int64(source, "operator_id"),
                    UserId = TolerantJson.Int64(source, "user_id")
                };
                break;
            case OneBot11EventTypes.GroupIncrease:
                result = new GroupIncreaseNoticeEvent
                {
                    SubType = TolerantJson.String(source, "sub_type"),
                    GroupId = TolerantJson.Int64(source, "group_id"),
                    OperatorId = TolerantJson.Int64(source, "operator_id"),
                    UserId = TolerantJson.Int64(source, "user_id")
                };
                break;
            case OneBot11EventTypes.GroupBan:
                result = new GroupBanNoticeEvent
                {
                    SubType = TolerantJson.String(source, "sub_type"),
                    GroupId = TolerantJson.Int64(source, "group_id"),
                    OperatorId = TolerantJson.Int64(source, "operator_id"),
                    UserId = TolerantJson.Int64(source, "user_id"),
                    Duration = TolerantJson.Int64(source, "duration")
                };
                break;
            case OneBot11EventTypes.FriendAdd:
                result = new FriendAddNoticeEvent
                {
                    UserId = TolerantJson.Int64(source, "user_id")
                };
                break;
            case OneBot11EventTypes.GroupRecall:
                result = new GroupRecallNoticeEvent
                {
                    GroupId = TolerantJson.Int64(source, "group_id"),
                    UserId = TolerantJson.Int64(source, "user_id"),
                    OperatorId = TolerantJson.Int64(source, "operator_id"),
                    MessageId = TolerantJson.Int64(source, "message_id")
                };
                break;
            case OneBot11EventTypes.FriendRecall:
                result = new FriendRecallNoticeEvent
                {
                    UserId = TolerantJson.Int64(source, "user_id"),
                    MessageId = TolerantJson.Int64(source, "message_id")
                };
                break;
            case OneBot11EventTypes.Notify:
                result = ParseNotify(source);
                break;
            default:
                result = new UnknownNoticeEvent
                {
                    SubType = TolerantJson.String(source, "sub_type")
                };
                break;
        }

        result.NoticeType = noticeType;
        return result;
    }

    private static OneBot11NoticeEvent ParseNotify(JsonObject source)
    {
        var subType = TolerantJson.String(source, "sub_type");
        switch (subType)
        {
            case "poke":
                return new GroupPokeNoticeEvent
                {
                    SubType = subType,
                    GroupId = TolerantJson.Int64(source, "group_id"),
                    UserId = TolerantJson.Int64(source, "user_id"),
                    TargetId = TolerantJson.Int64(source, "target_id")
                };
            case "lucky_king":
                return new LuckyKingNoticeEvent
                {
                    SubType = subType,
                    GroupId = TolerantJson.Int64(source, "group_id"),
                    UserId = TolerantJson.Int64(source, "user_id"),
                    TargetId = TolerantJson.Int64(source, "target_id")
                };
            case "honor":
                return new GroupHonorNoticeEvent
                {
                    SubType = subType,
                    GroupId = TolerantJson.Int64(source, "group_id"),
                    HonorType = TolerantJson.String(source, "honor_type"),
                    UserId = TolerantJson.Int64(source, "user_id")
                };
            default:
                return new UnknownNoticeEvent { SubType = subType };
        }
    }

    private static OneBot11RequestEvent ParseRequest(JsonObject source)
    {
        var requestType = TolerantJson.String(source, "request_type");
        OneBot11RequestEvent result;
        switch (requestType)
        {
            case OneBot11EventTypes.FriendRequest:
                result = new FriendRequestEvent();
                break;
            case OneBot11EventTypes.GroupRequest:
                result = new GroupRequestEvent
                {
                    SubType = TolerantJson.String(source, "sub_type"),
                    GroupId = TolerantJson.Int64(source, "group_id")
                };
                break;
            default:
                result = new UnknownRequestEvent
                {
                    SubType = TolerantJson.String(source, "sub_type")
                };
                break;
        }

        result.RequestType = requestType;
        result.UserId = TolerantJson.Int64(source, "user_id");
        result.Comment = TolerantJson.String(source, "comment");
        result.Flag = TolerantJson.String(source, "flag");
        return result;
    }

    private static OneBot11MetaEvent ParseMetaEvent(JsonObject source)
    {
        var metaEventType = TolerantJson.String(source, "meta_event_type");
        OneBot11MetaEvent result;
        switch (metaEventType)
        {
            case OneBot11EventTypes.Lifecycle:
                result = new LifecycleMetaEvent
                {
                    SubType = TolerantJson.String(source, "sub_type")
                };
                break;
            case OneBot11EventTypes.Heartbeat:
                result = new HeartbeatMetaEvent
                {
                    Status = TolerantJson.Parse(source, "status", node => ParseStatus(TolerantJson.Object(node))),
                    Interval = TolerantJson.Int64(source, "interval")
                };
                break;
            default:
                result = new UnknownMetaEvent
                {
                    SubType = TolerantJson.String(source, "sub_type")
                };
                break;
        }

        result.MetaEventType = metaEventType;
        return result;
    }

    private static PrivateMessageSender? ParsePrivateSender(JsonObject? source)
    {
        if (source == null)
        {
            return null;
        }

        var result = new PrivateMessageSender();
        FillSender(result, source);
        return result;
    }

    private static GroupMessageSender? ParseGroupSender(JsonObject? source)
    {
        if (source == null)
        {
            return null;
        }

        var result = new GroupMessageSender
        {
            Card = TolerantJson.String(source, "card"),
            Area = TolerantJson.String(source, "area"),
            Level = TolerantJson.String(source, "level"),
            Role = TolerantJson.String(source, "role"),
            Title = TolerantJson.String(source, "title")
        };
        FillSender(result, source);
        return result;
    }

    private static void FillSender(OneBot11MessageSender target, JsonObject source)
    {
        target.UserId = TolerantJson.Int64(source, "user_id");
        target.Nickname = TolerantJson.String(source, "nickname");
        target.Sex = TolerantJson.String(source, "sex");
        target.Age = TolerantJson.Int64(source, "age");
        target.RawJson = TolerantJson.CloneObject(source);
    }

    private static AnonymousInfo? ParseAnonymous(JsonObject? source)
    {
        return source == null
            ? null
            : new AnonymousInfo
            {
                Id = TolerantJson.Int64(source, "id"),
                Name = TolerantJson.String(source, "name"),
                Flag = TolerantJson.String(source, "flag"),
                RawJson = TolerantJson.CloneObject(source)
            };
    }

    private static GroupUploadFileInfo? ParseUploadFile(JsonObject? source)
    {
        return source == null
            ? null
            : new GroupUploadFileInfo
            {
                Id = TolerantJson.String(source, "id"),
                Name = TolerantJson.String(source, "name"),
                Size = TolerantJson.Int64(source, "size"),
                BusId = TolerantJson.Int64(source, "busid"),
                RawJson = TolerantJson.CloneObject(source)
            };
    }

    private static OneBot11Status? ParseStatus(JsonObject? source)
    {
        return source == null
            ? null
            : new OneBot11Status
            {
                Online = TolerantJson.Boolean(source, "online"),
                Good = TolerantJson.Boolean(source, "good"),
                RawJson = TolerantJson.CloneObject(source)
            };
    }
}
