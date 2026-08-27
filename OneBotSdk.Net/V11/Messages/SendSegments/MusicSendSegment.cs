using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents a send-only provider-backed music share. / 表示仅发送的平台音乐分享。</summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public sealed class MusicSendSegment : OneBot11SendSegment
{
    /// <summary>Initializes a provider-backed music share. / 初始化平台音乐分享。</summary>
    public MusicSendSegment(OneBot11MusicProvider provider, string id) : base(MessageSegmentTypes.Music)
    {
        if (provider != OneBot11MusicProvider.QQ &&
            provider != OneBot11MusicProvider.NetEase &&
            provider != OneBot11MusicProvider.Xiami)
        {
            throw new ArgumentOutOfRangeException(nameof(provider));
        }

        Provider = provider;
        Id = Require(id, nameof(id));
    }

    /// <summary>Gets the provider. / 获取音乐平台。</summary>
    public OneBot11MusicProvider Provider { get; }

    /// <summary>Gets the provider song ID. / 获取平台歌曲 ID。</summary>
    public string Id { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject
    {
        ["type"] = Provider == OneBot11MusicProvider.QQ
            ? "qq"
            : Provider == OneBot11MusicProvider.NetEase ? "163" : "xm",
        ["id"] = Id
    };
}
