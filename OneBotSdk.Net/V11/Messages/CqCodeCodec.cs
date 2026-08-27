using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>
/// Encodes and decodes the OneBot 11 string message (CQ-code) format.
/// 编码和解码 OneBot 11 字符串消息（CQ 码）格式。
/// </summary>
public static class CqCodeCodec
{
    private const string Prefix = "[CQ:";

    /// <summary>
    /// Encodes a message without changing a message that already uses the string representation.
    /// 编码消息；对于已使用字符串表示的消息不会改变其内容。
    /// </summary>
    public static string Encode(OneBot11Message message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (message.Kind == OneBot11MessageKind.String)
        {
            return message.StringValue ?? string.Empty;
        }

        return Encode(message.Segments);
    }

    /// <summary>
    /// Encodes an ordered message segment sequence as a CQ-code string.
    /// 将有序消息段序列编码为 CQ 码字符串。
    /// </summary>
    public static string Encode(IEnumerable<MessageSegment> segments)
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
                result.Append(EncodeSegment(segment));
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Decodes a CQ-code string into an array-format message while retaining unknown segment types.
    /// 将 CQ 码字符串解码为数组格式消息，同时保留未知消息段类型。
    /// </summary>
    public static OneBot11Message Decode(string cqCode)
    {
        if (cqCode == null)
        {
            throw new ArgumentNullException(nameof(cqCode));
        }

        var segments = new List<MessageSegment>();
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
            if (!TryParseSegment(body, out var segment))
            {
                // Only consume the opening bracket so a later valid CQ code can still be found.
                // 仅消耗起始方括号，使后续有效 CQ 码仍可被识别。
                pendingText.Append(cqCode[start]);
                cursor = start + 1;
                continue;
            }

            FlushText(pendingText, segments);
            segments.Add(segment!);
            cursor = end + 1;
        }

        FlushText(pendingText, segments);
        return OneBot11Message.FromSegments(segments);
    }

    /// <summary>
    /// Escapes plain text embedded outside CQ codes.
    /// 转义 CQ 码外部嵌入的纯文本。
    /// </summary>
    public static string EscapeText(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        // Ampersand must be encoded first to avoid escaping entities introduced below.
        // 必须先编码 &，避免再次转义随后产生的实体。
        return value
            .Replace("&", "&amp;")
            .Replace("[", "&#91;")
            .Replace("]", "&#93;");
    }

    /// <summary>
    /// Unescapes plain text embedded outside CQ codes.
    /// 反转义 CQ 码外部嵌入的纯文本。
    /// </summary>
    public static string UnescapeText(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        // Ampersand is decoded last so an escaped literal entity stays literal.
        // 最后解码 &，使被转义的字面实体仍保持字面含义。
        return value
            .Replace("&#91;", "[")
            .Replace("&#93;", "]")
            .Replace("&amp;", "&");
    }

    /// <summary>
    /// Escapes a CQ-code parameter value, including the comma separator.
    /// 转义 CQ 码参数值，包括参数分隔逗号。
    /// </summary>
    public static string EscapeParameter(string value)
    {
        return EscapeText(value).Replace(",", "&#44;");
    }

    /// <summary>
    /// Unescapes a CQ-code parameter value.
    /// 反转义 CQ 码参数值。
    /// </summary>
    public static string UnescapeParameter(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        return UnescapeText(value.Replace("&#44;", ","));
    }

    private static string EncodeSegment(MessageSegment segment)
    {
        var type = segment.Type;
        if (string.IsNullOrEmpty(type))
        {
            return string.Empty;
        }

        if (string.Equals(type, MessageSegmentTypes.Text, StringComparison.Ordinal))
        {
            return EscapeText(segment.GetString("text") ?? string.Empty);
        }

        var result = new StringBuilder(Prefix);
        result.Append(type);

        if (segment.Data != null)
        {
            foreach (var parameter in segment.Data)
            {
                result.Append(',');
                result.Append(parameter.Key);
                result.Append('=');
                result.Append(EscapeParameter(GetParameterText(type!, parameter.Key, parameter.Value)));
            }
        }

        result.Append(']');
        return result.ToString();
    }

    private static string GetParameterText(string segmentType, string parameterName, JsonNode? value)
    {
        if (string.Equals(segmentType, MessageSegmentTypes.Node, StringComparison.Ordinal) &&
            string.Equals(parameterName, "content", StringComparison.Ordinal) &&
            (value is JsonArray || value is JsonObject))
        {
            var nestedMessage = OneBot11Message.Parse(value);
            return nestedMessage == null ? string.Empty : Encode(nestedMessage);
        }

        return TolerantJson.String(value) ?? (value == null ? string.Empty : OneBot11Json.Serialize(value));
    }

    private static bool TryParseSegment(string body, out MessageSegment? segment)
    {
        segment = null;
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

                // Split at the first equals sign because equals is valid inside parameter values.
                // 仅按第一个等号拆分，因为参数值内部允许包含等号。
                data[name] = UnescapeParameter(parameter.Substring(equals + 1));
            }
        }

        segment = new MessageSegment(type, data);
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

    private static void FlushText(StringBuilder pendingText, ICollection<MessageSegment> segments)
    {
        if (pendingText.Length == 0)
        {
            return;
        }

        segments.Add(MessageSegment.Text(UnescapeText(pendingText.ToString())));
        pendingText.Clear();
    }
}
