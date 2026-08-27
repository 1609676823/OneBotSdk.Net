using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Responses;

/// <summary>Contains OneBot implementation version information. / 包含 OneBot 实现端版本信息。</summary>
public sealed class OneBot12VersionData : OneBot12JsonModel
{
    private OneBot12VersionData(JsonObject raw, string? impl, string? version, string? oneBotVersion) : base(raw)
    {
        Impl = impl;
        Version = version;
        OneBotVersion = oneBotVersion;
    }

    /// <summary>Gets the normalized implementation name. / 获取规范化的实现端名称。</summary>
    [JsonPropertyName("impl")]
    public string? Impl { get; }

    /// <summary>Gets the implementation version. / 获取实现端版本。</summary>
    [JsonPropertyName("version")]
    public string? Version { get; }

    /// <summary>Gets the implemented OneBot standard version. / 获取实现的 OneBot 标准版本。</summary>
    [JsonPropertyName("onebot_version")]
    public string? OneBotVersion { get; }

    internal static OneBot12VersionData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null ? null : new OneBot12VersionData(
            TolerantJson.CloneObject(source),
            TolerantJson.String(source, "impl"),
            TolerantJson.String(source, "version"),
            TolerantJson.String(source, "onebot_version"));
    }
}

/// <summary>Contains one bot account's online status. / 包含一个机器人账号的在线状态。</summary>
public sealed class OneBot12BotStatus : OneBot12JsonModel
{
    private OneBot12BotStatus(JsonObject raw, OneBot12Self? self, bool? online) : base(raw)
    {
        Self = self;
        Online = online;
    }

    /// <summary>Gets the bot identity. / 获取机器人身份。</summary>
    [JsonPropertyName("self")]
    public OneBot12Self? Self { get; }

    /// <summary>Gets whether the bot account is online. / 获取机器人账号是否在线。</summary>
    [JsonPropertyName("online")]
    public bool? Online { get; }

    internal static OneBot12BotStatus? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null ? null : new OneBot12BotStatus(
            TolerantJson.CloneObject(source),
            OneBot12Self.Parse(TolerantJson.Node(source, "self")),
            TolerantJson.Boolean(source, "online"));
    }
}

/// <summary>Contains the implementation-wide and per-bot runtime status. / 包含实现端整体及各机器人运行状态。</summary>
public sealed class OneBot12StatusData : OneBot12JsonModel
{
    private OneBot12StatusData(JsonObject raw, bool? good, IReadOnlyList<OneBot12BotStatus> bots) : base(raw)
    {
        Good = good;
        Bots = bots;
    }

    /// <summary>Gets whether every implementation module is healthy. / 获取实现端全部模块是否健康。</summary>
    [JsonPropertyName("good")]
    public bool? Good { get; }

    /// <summary>Gets status entries for bot accounts on the connection. / 获取连接上机器人账号的状态条目。</summary>
    [JsonPropertyName("bots")]
    public IReadOnlyList<OneBot12BotStatus> Bots { get; }

    internal static OneBot12StatusData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null ? null : new OneBot12StatusData(
            TolerantJson.CloneObject(source),
            TolerantJson.Boolean(source, "good"),
            OneBot12ResponseDataParsers.ParseList(
                TolerantJson.Node(source, "bots"),
                OneBot12BotStatus.Parse));
    }
}
