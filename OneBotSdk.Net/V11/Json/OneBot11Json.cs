using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

namespace OneBotSdk.Net.V11.Json;

/// <summary>
/// Provides the shared <see cref="System.Text.Json"/> configuration used by OneBotSdk.Net.
/// 提供 OneBotSdk.Net 统一使用的 <see cref="System.Text.Json"/> 配置。
/// </summary>
public static class OneBot11Json
{
    private static readonly JsonSerializerOptions DefaultSerializerOptions = CreateSharedSerializerOptions(false);
    private static readonly JsonSerializerOptions UnsafeRelaxedSerializerOptions = CreateSharedSerializerOptions(true);
    private static int _useUnsafeRelaxedJsonEscaping;

    /// <summary>
    /// Gets or sets whether JSON writing uses <see cref="JavaScriptEncoder.UnsafeRelaxedJsonEscaping"/> globally.
    /// 获取或设置 JSON 写出是否全局使用 <see cref="JavaScriptEncoder.UnsafeRelaxedJsonEscaping"/>。
    /// </summary>
    /// <remarks>
    /// The default is <see langword="false"/>. This option changes string escaping only during serialization;
    /// it does not relax JSON parsing or deserialization rules.
    /// 默认值为 <see langword="false"/>。该选项只改变序列化时的字符串转义，
    /// 不会放宽 JSON 解析或反序列化规则。
    /// </remarks>
    public static bool UseUnsafeRelaxedJsonEscaping
    {
        get => Volatile.Read(ref _useUnsafeRelaxedJsonEscaping) != 0;
        set => Volatile.Write(ref _useUnsafeRelaxedJsonEscaping, value ? 1 : 0);
    }

    /// <summary>
    /// Creates caller-owned serializer options that reflect the current global encoder selection.
    /// 创建由调用方拥有且反映当前全局编码器选择的序列化选项。
    /// </summary>
    public static JsonSerializerOptions CreateSerializerOptions()
    {
        return CreateSerializerOptionsCore(UseUnsafeRelaxedJsonEscaping);
    }

    /// <summary>
    /// Serializes a value with <see cref="System.Text.Json"/> and the current global encoder selection.
    /// 使用 <see cref="System.Text.Json"/> 和当前全局编码器选择序列化值。
    /// </summary>
    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, CurrentSerializerOptions);
    }

    /// <summary>
    /// Deserializes a value with <see cref="System.Text.Json"/>.
    /// 使用 <see cref="System.Text.Json"/> 反序列化值。
    /// </summary>
    /// <remarks>
    /// OneBot protocol response models use their dedicated field-tolerant parsers instead of this whole-object helper.
    /// OneBot 协议响应模型使用专用的按字段容错解析器，而不是此整对象辅助方法。
    /// </remarks>
    public static T? Deserialize<T>(string json)
    {
        if (json == null)
        {
            throw new ArgumentNullException(nameof(json));
        }

        return JsonSerializer.Deserialize<T>(json, CurrentSerializerOptions);
    }

    /// <summary>
    /// Parses JSON into a mutable node by using <see cref="System.Text.Json"/> strict defaults.
    /// 使用 <see cref="System.Text.Json"/> 的严格默认规则将 JSON 解析为可变节点。
    /// </summary>
    public static JsonNode? Parse(string json)
    {
        if (json == null)
        {
            throw new ArgumentNullException(nameof(json));
        }

        return JsonNode.Parse(json);
    }

    /// <summary>
    /// Creates a detached node without relying on newer-framework-only cloning APIs.
    /// 在不依赖较新框架专有克隆 API 的情况下创建独立节点。
    /// </summary>
    internal static JsonNode? Clone(JsonNode? value)
    {
        // SerializeToNode provides a detached graph on every supported System.Text.Json version without a text round trip.
        // SerializeToNode 可在所有受支持的 System.Text.Json 版本上直接创建独立节点，无需经过字符串往返。
        return value == null ? null : JsonSerializer.SerializeToNode(value, DefaultSerializerOptions);
    }

    private static JsonSerializerOptions CurrentSerializerOptions =>
        UseUnsafeRelaxedJsonEscaping ? UnsafeRelaxedSerializerOptions : DefaultSerializerOptions;

    private static JsonSerializerOptions CreateSerializerOptionsCore(bool useUnsafeRelaxedJsonEscaping)
    {
        return new JsonSerializerOptions
        {
            Encoder = useUnsafeRelaxedJsonEscaping
                ? JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                : JavaScriptEncoder.Default
        };
    }

    private static JsonSerializerOptions CreateSharedSerializerOptions(bool useUnsafeRelaxedJsonEscaping)
    {
        var options = CreateSerializerOptionsCore(useUnsafeRelaxedJsonEscaping);

        // First use makes JsonSerializerOptions read-only, so shared instances are never mutated concurrently.
        // 首次使用会使 JsonSerializerOptions 变为只读，因此共享实例不会发生并发修改。
        JsonSerializer.Serialize(string.Empty, options);
        return options;
    }
}
