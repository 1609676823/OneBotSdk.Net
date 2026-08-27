using System;
using System.Collections.Generic;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>
/// Provides explicit, direction-safe conversions from received models to outgoing models.
/// 提供从接收模型到出站模型的显式方向安全转换。
/// </summary>
public static class OneBot11MessageConversions
{
    /// <summary>
    /// Tries to create a new outgoing message and rejects receive-only or unknown segments.
    /// 尝试创建新的出站消息，并拒绝仅接收或未知消息段。
    /// </summary>
    public static bool TryToSendMessage(
        this OneBot11ReceivedMessage received,
        out OneBot11SendMessage? outgoing)
    {
        if (received == null)
        {
            throw new ArgumentNullException(nameof(received));
        }

        var segments = new List<OneBot11SendSegment>();
        foreach (var segment in received)
        {
            if (!segment.TryToSendSegment(out var outgoingSegment) || outgoingSegment == null)
            {
                outgoing = null;
                return false;
            }

            segments.Add(outgoingSegment);
        }

        if (segments.Count == 0 && received.Kind == OneBot11ReceivedMessageKind.String)
        {
            outgoing = OneBot11SendMessage.FromString(received.StringValue ?? string.Empty);
        }
        else
        {
            // Even a CQ-code input becomes a sanitized segment array so receive-only parameters cannot leak back out.
            // 即使输入为 CQ 码，也会生成经过净化的消息段数组，防止仅接收参数再次泄漏出去。
            outgoing = received.Kind == OneBot11ReceivedMessageKind.Segment && segments.Count == 1
                ? OneBot11SendMessage.FromSegment(segments[0])
                : OneBot11SendMessage.FromSegments(segments);
        }

        return true;
    }

    /// <summary>
    /// Tries to copy only fields that the official protocol permits in the outgoing direction.
    /// 尝试仅复制官方协议允许用于出站方向的字段。
    /// </summary>
    public static bool TryToSendSegment(
        this OneBot11ReceivedSegment received,
        out OneBot11SendSegment? outgoing)
    {
        if (received == null)
        {
            throw new ArgumentNullException(nameof(received));
        }

        switch (received)
        {
            case TextReceivedSegment text when text.Text != null:
                outgoing = new TextSendSegment(text.Text);
                return true;
            case FaceReceivedSegment face when HasValue(face.Id):
                outgoing = new FaceSendSegment(face.Id!);
                return true;
            case ImageReceivedSegment image when HasValue(image.File):
                // A received URL is metadata; the reusable received file name is the official outgoing parameter.
                // 收到的 URL 是元数据；可复用的已接收文件名才是官方出站参数。
                outgoing = new ImageSendSegment(
                    image.File!,
                    string.Equals(image.ImageType, "flash", StringComparison.Ordinal));
                return true;
            case RecordReceivedSegment record when HasValue(record.File):
                outgoing = new RecordSendSegment(record.File!, magic: record.Magic);
                return true;
            case VideoReceivedSegment video when HasValue(video.File):
                outgoing = new VideoSendSegment(video.File!);
                return true;
            case AtReceivedSegment at when HasValue(at.Target):
                outgoing = new AtSendSegment(at.Target!);
                return true;
            case RpsReceivedSegment:
                outgoing = new RpsSendSegment();
                return true;
            case DiceReceivedSegment:
                outgoing = new DiceSendSegment();
                return true;
            case ShakeReceivedSegment:
                outgoing = new ShakeSendSegment();
                return true;
            case PokeReceivedSegment poke when HasValue(poke.PokeType) && HasValue(poke.Id):
                outgoing = new PokeSendSegment(poke.PokeType!, poke.Id!);
                return true;
            case ShareReceivedSegment share when HasValue(share.Url) && HasValue(share.Title):
                outgoing = new ShareSendSegment(share.Url!, share.Title!, share.Content, share.Image);
                return true;
            case ContactReceivedSegment contact when HasValue(contact.Id) &&
                                                             string.Equals(contact.ContactType, "qq", StringComparison.Ordinal):
                outgoing = new ContactSendSegment(OneBot11ContactTarget.Friend, contact.Id!);
                return true;
            case ContactReceivedSegment contact when HasValue(contact.Id) &&
                                                             string.Equals(contact.ContactType, "group", StringComparison.Ordinal):
                outgoing = new ContactSendSegment(OneBot11ContactTarget.Group, contact.Id!);
                return true;
            case LocationReceivedSegment location when HasValue(location.Latitude) && HasValue(location.Longitude):
                outgoing = new LocationSendSegment(
                    location.Latitude!,
                    location.Longitude!,
                    location.Title,
                    location.Content);
                return true;
            case ReplyReceivedSegment reply when HasValue(reply.MessageId):
                outgoing = new ReplySendSegment(reply.MessageId!);
                return true;
            case ForwardNodeReceivedSegment node when HasValue(node.UserId) &&
                                                       HasValue(node.Nickname) &&
                                                       node.Content != null &&
                                                       node.Content.TryToSendMessage(out var nested) &&
                                                       nested != null:
                outgoing = new CustomForwardNodeSendSegment(node.UserId!, node.Nickname!, nested);
                return true;
            case XmlReceivedSegment xml when xml.Xml != null:
                outgoing = new XmlSendSegment(xml.Xml);
                return true;
            case JsonReceivedSegment json when json.Json != null:
                outgoing = new JsonSendSegment(json.Json);
                return true;
            default:
                // Forward references and unknown segments require an explicit user decision and are never copied implicitly.
                // 合并转发引用和未知消息段需要用户明确决定，绝不会被隐式复制。
                outgoing = null;
                return false;
        }
    }

    private static bool HasValue(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
}
