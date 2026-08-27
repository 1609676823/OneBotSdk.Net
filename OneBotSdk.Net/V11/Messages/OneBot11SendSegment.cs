using System;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>
/// Defines a segment that is valid as an outgoing OneBot 11 API parameter.
/// 定义可作为 OneBot 11 出站 API 参数的消息段。
/// </summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public abstract class OneBot11SendSegment
{
    /// <summary>Initializes an outgoing segment discriminator. / 初始化出站消息段判别值。</summary>
    protected OneBot11SendSegment(string type)
    {
        Type = Require(type, nameof(type));
    }

    /// <summary>Gets the protocol segment discriminator. / 获取协议消息段判别值。</summary>
    public string Type { get; }

    /// <summary>Creates an independent array-format segment object. / 创建独立的数组格式消息段对象。</summary>
    public JsonObject ToJsonObject()
    {
        return new JsonObject
        {
            ["type"] = Type,
            ["data"] = CreateData()
        };
    }

    /// <summary>Creates the protocol data object for this segment. / 创建当前消息段的协议数据对象。</summary>
    protected abstract JsonObject? CreateData();

    /// <summary>Validates a required protocol string. / 校验必填协议字符串。</summary>
    protected static string Require(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty protocol value is required.", parameterName);
        }

        return value!;
    }

    /// <summary>Adds an optional string parameter. / 添加可选字符串参数。</summary>
    protected static void Add(JsonObject data, string name, string? value)
    {
        if (value != null)
        {
            data[name] = value;
        }
    }

    /// <summary>Adds an optional OneBot boolean as its canonical string value. / 以规范字符串值添加可选 OneBot 布尔参数。</summary>
    protected static void AddBoolean(JsonObject data, string name, bool? value)
    {
        if (value.HasValue)
        {
            data[name] = value.Value ? "1" : "0";
        }
    }

    /// <summary>Adds an optional integer as the message-array string representation. / 以消息数组字符串表示添加可选整数。</summary>
    protected static void AddInteger(JsonObject data, string name, long? value)
    {
        if (value.HasValue)
        {
            data[name] = value.Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
