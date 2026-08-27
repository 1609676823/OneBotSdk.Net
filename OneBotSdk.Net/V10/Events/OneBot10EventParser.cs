using System;
using System.Text.Json.Nodes;
using OneBotSdk.Net.V10.Json;
using OneBotSdk.Net.V10.Messages;

namespace OneBotSdk.Net.V10.Events;

/// <summary>
/// Parses standard and extension OneBot 10 events with field-level fault isolation.
/// 以字段级故障隔离方式解析标准和扩展 OneBot 10 事件。
/// </summary>
public static class OneBot10EventParser
{
    /// <summary>
    /// Parses an event object and returns an unknown fallback instead of rejecting unknown discriminators.
    /// 解析事件对象；遇到未知判别值时返回未知回退类型，而不是拒绝事件。
    /// </summary>
    public static OneBot10Event Parse(JsonObject source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var postType = TolerantJson.String(source, "post_type");
        OneBot10Event result;
        switch (postType)
        {
            case OneBot10EventTypes.Message:
                result = ParseMessage(source);
                break;
            case OneBot10EventTypes.Notice:
                result = ParseNotice(source);
                break;
            case OneBot10EventTypes.Request:
                result = ParseRequest(source);
                break;
            case OneBot10EventTypes.MetaEvent:
                result = ParseMetaEvent(source);
                break;
            default:
                result = new UnknownOneBot10Event();
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

    private static OneBot10MessageEvent ParseMessage(JsonObject source)
    {
        var messageType = TolerantJson.String(source, "message_type");
        OneBot10MessageEvent result;
        switch (messageType)
        {
            case OneBot10EventTypes.PrivateMessage:
                result = new PrivateMessageEvent
                {
                    Sender = TolerantJson.Parse(source, "sender", node => ParsePrivateSender(TolerantJson.Object(node)))
                };
                break;
            case OneBot10EventTypes.GroupMessage:
                result = new GroupMessageEvent
                {
                    GroupId = TolerantJson.Int64(source, "group_id"),
                    Anonymous = TolerantJson.Parse(source, "anonymous", node => ParseAnonymous(TolerantJson.Object(node))),
                    Sender = TolerantJson.Parse(source, "sender", node => ParseGroupSender(TolerantJson.Object(node)))
                };
                break;
            case OneBot10EventTypes.DiscussMessage:
                result = new DiscussMessageEvent
                {
                    DiscussId = TolerantJson.Int64(source, "discuss_id"),
                    Sender = TolerantJson.Parse(source, "sender", node => ParseDiscussSender(TolerantJson.Object(node)))
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
        result.MessageChain = TolerantJson.Parse(source, "message", OneBot10ReceivedMessage.Parse) ??
                              OneBot10ReceivedMessage.Empty;
        result.RawMessage = TolerantJson.String(source, "raw_message");
        result.Font = TolerantJson.Int64(source, "font");
        return result;
    }

    private static OneBot10NoticeEvent ParseNotice(JsonObject source)
    {
        var noticeType = TolerantJson.String(source, "notice_type");
        OneBot10NoticeEvent result;
        switch (noticeType)
        {
            case OneBot10EventTypes.GroupUpload:
                result = new GroupUploadNoticeEvent
                {
                    GroupId = TolerantJson.Int64(source, "group_id"),
                    UserId = TolerantJson.Int64(source, "user_id"),
                    File = TolerantJson.Parse(source, "file", node => ParseUploadFile(TolerantJson.Object(node)))
                };
                break;
            case OneBot10EventTypes.GroupAdmin:
                result = new GroupAdminNoticeEvent
                {
                    SubType = TolerantJson.String(source, "sub_type"),
                    GroupId = TolerantJson.Int64(source, "group_id"),
                    UserId = TolerantJson.Int64(source, "user_id")
                };
                break;
            case OneBot10EventTypes.GroupDecrease:
                result = new GroupDecreaseNoticeEvent
                {
                    SubType = TolerantJson.String(source, "sub_type"),
                    GroupId = TolerantJson.Int64(source, "group_id"),
                    OperatorId = TolerantJson.Int64(source, "operator_id"),
                    UserId = TolerantJson.Int64(source, "user_id")
                };
                break;
            case OneBot10EventTypes.GroupIncrease:
                result = new GroupIncreaseNoticeEvent
                {
                    SubType = TolerantJson.String(source, "sub_type"),
                    GroupId = TolerantJson.Int64(source, "group_id"),
                    OperatorId = TolerantJson.Int64(source, "operator_id"),
                    UserId = TolerantJson.Int64(source, "user_id")
                };
                break;
            case OneBot10EventTypes.GroupBan:
                result = new GroupBanNoticeEvent
                {
                    SubType = TolerantJson.String(source, "sub_type"),
                    GroupId = TolerantJson.Int64(source, "group_id"),
                    OperatorId = TolerantJson.Int64(source, "operator_id"),
                    UserId = TolerantJson.Int64(source, "user_id"),
                    Duration = TolerantJson.Int64(source, "duration")
                };
                break;
            case OneBot10EventTypes.FriendAdd:
                result = new FriendAddNoticeEvent
                {
                    UserId = TolerantJson.Int64(source, "user_id")
                };
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

    private static OneBot10RequestEvent ParseRequest(JsonObject source)
    {
        var requestType = TolerantJson.String(source, "request_type");
        OneBot10RequestEvent result;
        switch (requestType)
        {
            case OneBot10EventTypes.FriendRequest:
                result = new FriendRequestEvent();
                break;
            case OneBot10EventTypes.GroupRequest:
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

    private static OneBot10MetaEvent ParseMetaEvent(JsonObject source)
    {
        var metaEventType = TolerantJson.String(source, "meta_event_type");
        OneBot10MetaEvent result;
        switch (metaEventType)
        {
            case OneBot10EventTypes.Lifecycle:
                result = new LifecycleMetaEvent
                {
                    SubType = TolerantJson.String(source, "sub_type")
                };
                break;
            case OneBot10EventTypes.Heartbeat:
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

    private static DiscussMessageSender? ParseDiscussSender(JsonObject? source)
    {
        if (source == null)
        {
            return null;
        }

        var result = new DiscussMessageSender();
        FillSender(result, source);
        return result;
    }

    private static void FillSender(OneBot10MessageSender target, JsonObject source)
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

    private static OneBot10Status? ParseStatus(JsonObject? source)
    {
        return source == null
            ? null
            : new OneBot10Status
            {
                AppInitialized = TolerantJson.Boolean(source, "app_initialized"),
                AppEnabled = TolerantJson.Boolean(source, "app_enabled"),
                PluginsGood = TolerantJson.Boolean(source, "plugins_good"),
                AppGood = TolerantJson.Boolean(source, "app_good"),
                Online = TolerantJson.Boolean(source, "online"),
                Good = TolerantJson.Boolean(source, "good"),
                RawJson = TolerantJson.CloneObject(source)
            };
    }
}
