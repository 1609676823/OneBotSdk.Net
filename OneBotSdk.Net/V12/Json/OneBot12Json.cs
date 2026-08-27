using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

namespace OneBotSdk.Net.V12.Json;

/// <summary>
/// Provides the shared System.Text.Json configuration for OneBot 12.
/// 提供 OneBot 12 统一使用的 System.Text.Json 配置。
/// </summary>
public static class OneBot12Json
{
    private static readonly JsonSerializerOptions DefaultOptions = CreateSharedOptions(false);
    private static readonly JsonSerializerOptions UnsafeOptions = CreateSharedOptions(true);
    private static int _useUnsafeRelaxedJsonEscaping;

    /// <summary>
    /// Gets or sets whether serialization globally uses JavaScriptEncoder.UnsafeRelaxedJsonEscaping.
    /// 获取或设置序列化是否全局使用 JavaScriptEncoder.UnsafeRelaxedJsonEscaping。
    /// </summary>
    /// <remarks>
    /// The default is false and this setting changes writing only, never JSON parsing rules.
    /// 默认值为 false；此设置只影响 JSON 写出，不会放宽 JSON 解析规则。
    /// </remarks>
    public static bool UseUnsafeRelaxedJsonEscaping
    {
        get => Volatile.Read(ref _useUnsafeRelaxedJsonEscaping) != 0;
        set => Volatile.Write(ref _useUnsafeRelaxedJsonEscaping, value ? 1 : 0);
    }

    /// <summary>Creates caller-owned serializer options. / 创建由调用方拥有的序列化选项。</summary>
    public static JsonSerializerOptions CreateSerializerOptions()
    {
        return CreateOptions(UseUnsafeRelaxedJsonEscaping);
    }

    /// <summary>Serializes with System.Text.Json and the current encoder selection. / 使用 System.Text.Json 和当前编码器选项进行序列化。</summary>
    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, CurrentOptions);
    }

    /// <summary>Deserializes with System.Text.Json. / 使用 System.Text.Json 进行反序列化。</summary>
    public static T? Deserialize<T>(string json)
    {
        if (json == null)
        {
            throw new ArgumentNullException(nameof(json));
        }

        return JsonSerializer.Deserialize<T>(json, CurrentOptions);
    }

    /// <summary>Parses strict JSON into a mutable node. / 将严格 JSON 解析为可变节点。</summary>
    public static JsonNode? Parse(string json)
    {
        if (json == null)
        {
            throw new ArgumentNullException(nameof(json));
        }

        return JsonNode.Parse(json);
    }

    internal static JsonNode? Clone(JsonNode? value)
    {
        // SerializeToNode creates a detached graph on every supported target framework.
        // SerializeToNode 可在所有受支持目标框架上创建独立对象图。
        return value == null ? null : JsonSerializer.SerializeToNode(value, DefaultOptions);
    }

    private static JsonSerializerOptions CurrentOptions =>
        UseUnsafeRelaxedJsonEscaping ? UnsafeOptions : DefaultOptions;

    private static JsonSerializerOptions CreateOptions(bool unsafeRelaxed)
    {
        return new JsonSerializerOptions
        {
            Encoder = unsafeRelaxed
                ? JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                : JavaScriptEncoder.Default
        };
    }

    private static JsonSerializerOptions CreateSharedOptions(bool unsafeRelaxed)
    {
        var options = CreateOptions(unsafeRelaxed);
        JsonSerializer.Serialize(string.Empty, options);
        return options;
    }
}
