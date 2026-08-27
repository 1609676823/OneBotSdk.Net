using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Defines a segment valid in an outgoing OneBot 12 message. / 定义可用于 OneBot 12 出站消息的消息段。</summary>
[JsonConverter(typeof(OneBot12SendSegmentJsonConverter))]
public abstract class OneBot12SendSegment
{
    /// <summary>Initializes a standard or extension segment. / 初始化标准或扩展消息段。</summary>
    protected OneBot12SendSegment(string type)
    {
        Type = Require(type, nameof(type));
    }

    /// <summary>Gets the wire discriminator. / 获取线协议判别值。</summary>
    public string Type { get; }

    /// <summary>Creates an independent type/data wire object. / 创建独立的 type/data 线协议对象。</summary>
    public JsonObject ToJsonObject()
    {
        return new JsonObject
        {
            ["type"] = Type,
            ["data"] = CreateData()
        };
    }

    /// <summary>Creates this segment's data object. / 创建当前消息段的 data 对象。</summary>
    protected abstract JsonObject CreateData();

    /// <summary>Validates a required non-empty protocol identifier. / 校验必填且非空的协议标识。</summary>
    protected static string Require(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty OneBot protocol value is required.", parameterName);
        }

        return value!;
    }

    /// <summary>Clones a caller-owned extension object. / 克隆调用方拥有的扩展对象。</summary>
    protected static JsonObject Clone(JsonObject value) => TolerantJson.CloneObject(value);
}
