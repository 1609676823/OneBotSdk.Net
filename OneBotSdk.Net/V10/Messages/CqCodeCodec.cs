using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using OneBotSdk.Net.V10.Json;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>
/// Encodes outgoing messages and decodes CQ-code text into received-only message objects.
/// 将出站消息编码为 CQ 码，并将 CQ 码文本解码为仅接收消息对象。
/// </summary>
public static class CqCodeCodec
{
    private const string Prefix = "[CQ:";

    /// <summary>Encodes an outgoing message without introducing a shared send/receive model. / 编码出站消息，不引入收发共享模型。</summary>
    public static string Encode(OneBot10SendMessage message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (message.Kind == OneBot10SendMessageKind.String)
        {
            return message.StringValue ?? string.Empty;
        }

        return EncodeSegments(message);
    }

    /// <summary>Encodes ordered outgoing segments as CQ-code text. / 将有序出站消息段编码为 CQ 码文本。</summary>
    public static string EncodeSegments(IEnumerable<OneBot10SendSegment> segments)
    {
        if (segments == null)
        {
            throw new ArgumentNullException(nameof(segments));
        }

        var result = new StringBuilder();
        foreach (var segment in segments)
        {
            if (segment != null)
            {
                result.Append(EncodeObject(segment.ToJsonObject()));
            }
        }

        return result.ToString();
    }

    /// <summary>Decodes CQ-code text into an independently typed received message chain. / 将 CQ 码文本解码为独立强类型接收消息链。</summary>
    public static OneBot10ReceivedMessage Decode(string cqCode)
    {
        if (cqCode == null)
        {
            throw new ArgumentNullException(nameof(cqCode));
        }

        var segments = new List<OneBot10ReceivedSegment>();
        var pendingText = new StringBuilder();
        var cursor = 0;

        while (cursor < cqCode.Length)
        {
            var start = cqCode.IndexOf(Prefix, cursor, StringComparison.Ordinal);
            if (start < 0)
            {
                pendingText.Append(cqCode, cursor, cqCode.Length - cursor);
                break;
            }

            pendingText.Append(cqCode, cursor, start - cursor);
            var end = cqCode.IndexOf(']', start + Prefix.Length);
            if (end < 0)
            {
                pendingText.Append(cqCode, start, cqCode.Length - start);
                break;
            }

            var body = cqCode.Substring(start + Prefix.Length, end - start - Prefix.Length);
            if (!TryParseObject(body, out var segmentObject))
            {
                // Consume only the opening bracket so a later valid CQ code can still be found.
                // 仅消费起始方括号，使后续有效 CQ 码仍可被识别。
                pendingText.Append(cqCode[start]);
                cursor = start + 1;
                continue;
            }

            FlushText(pendingText, segments);
            var received = OneBot10ReceivedSegment.Parse(segmentObject);
            if (received != null)
            {
                segments.Add(received);
            }

            cursor = end + 1;
        }

        FlushText(pendingText, segments);
        return OneBot10ReceivedMessage.FromCqCode(cqCode, segments.AsReadOnly());
    }

    /// <summary>Escapes plain text embedded outside CQ codes. / 转义 CQ 码外部嵌入的纯文本。</summary>
    public static string EscapeText(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        // Ampersand must be encoded first to avoid escaping entities introduced below.
        // 必须先编码 &，避免再次转义随后产生的实体。
        return value.Replace("&", "&amp;").Replace("[", "&#91;").Replace("]", "&#93;");
    }

    /// <summary>Unescapes plain text embedded outside CQ codes. / 反转义 CQ 码外部嵌入的纯文本。</summary>
    public static string UnescapeText(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        // Ampersand is decoded last so an escaped literal entity remains literal.
        // 最后解码 &，使被转义的字面实体仍保持字面含义。
        return value.Replace("&#91;", "[").Replace("&#93;", "]").Replace("&amp;", "&");
    }

    /// <summary>Escapes a CQ-code parameter value, including the comma separator. / 转义 CQ 码参数值，包括逗号分隔符。</summary>
    public static string EscapeParameter(string value)
    {
        return EscapeText(value).Replace(",", "&#44;");
    }

    /// <summary>Unescapes a CQ-code parameter value. / 反转义 CQ 码参数值。</summary>
    public static string UnescapeParameter(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        return UnescapeText(value.Replace("&#44;", ","));
    }

    internal static string EncodeReceived(IEnumerable<OneBot10ReceivedSegment> segments)
    {
        var result = new StringBuilder();
        foreach (var segment in segments)
        {
            result.Append(EncodeObject(segment.ToJsonObject()));
        }

        return result.ToString();
    }

    private static string EncodeObject(JsonObject segment)
    {
        var type = TolerantJson.String(segment, "type");
        if (string.IsNullOrEmpty(type))
        {
            return string.Empty;
        }

        var data = TolerantJson.Object(TolerantJson.Node(segment, "data"));
        if (string.Equals(type, MessageSegmentTypes.Text, StringComparison.Ordinal))
        {
            return EscapeText(TolerantJson.String(data, "text") ?? string.Empty);
        }

        var result = new StringBuilder(Prefix);
        result.Append(type);
        if (data != null)
        {
            foreach (var parameter in data)
            {
                result.Append(',');
                result.Append(parameter.Key);
                result.Append('=');
                result.Append(EscapeParameter(GetParameterText(parameter.Value)));
            }
        }

        result.Append(']');
        return result.ToString();
    }

    private static string GetParameterText(JsonNode? value)
    {
        // Array-format parameters are strings; extension nodes retain their JSON representation.
        // 数组格式参数为字符串；扩展节点保留其 JSON 表示。
        return TolerantJson.String(value) ?? (value == null ? string.Empty : OneBot10Json.Serialize(value));
    }

    private static bool TryParseObject(string body, out JsonObject segment)
    {
        segment = new JsonObject();
        var separator = body.IndexOf(',');
        var type = separator < 0 ? body : body.Substring(0, separator);
        if (string.IsNullOrEmpty(type) || ContainsWhitespace(type))
        {
            return false;
        }

        var data = new JsonObject();
        if (separator >= 0)
        {
            var parameters = body.Substring(separator + 1).Split(',');
            foreach (var parameter in parameters)
            {
                var equals = parameter.IndexOf('=');
                if (equals <= 0)
                {
                    return false;
                }

                var name = parameter.Substring(0, equals);
                if (ContainsWhitespace(name))
                {
                    return false;
                }

                // Split only at the first equals sign because equals is valid inside parameter values.
                // 仅按第一个等号拆分，因为参数值内部允许等号。
                data[name] = UnescapeParameter(parameter.Substring(equals + 1));
            }
        }

        segment["type"] = type;
        segment["data"] = data;
        return true;
    }

    private static bool ContainsWhitespace(string value)
    {
        for (var index = 0; index < value.Length; index++)
        {
            if (char.IsWhiteSpace(value[index]))
            {
                return true;
            }
        }

        return false;
    }

    private static void FlushText(StringBuilder pendingText, ICollection<OneBot10ReceivedSegment> segments)
    {
        if (pendingText.Length == 0)
        {
            return;
        }

        var textObject = new JsonObject
        {
            ["type"] = MessageSegmentTypes.Text,
            ["data"] = new JsonObject { ["text"] = UnescapeText(pendingText.ToString()) }
        };
        var received = OneBot10ReceivedSegment.Parse(textObject);
        if (received != null)
        {
            segments.Add(received);
        }

        pendingText.Clear();
    }
}
