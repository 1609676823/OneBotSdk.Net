using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>Represents a send-only provider-backed music share. / 表示仅发送的平台音乐分享。</summary>
[JsonConverter(typeof(OneBot10SendSegmentJsonConverter))]
public sealed class MusicSendSegment : OneBot10SendSegment
{
    /// <summary>Initializes a provider-backed music share. / 初始化平台音乐分享。</summary>
    public MusicSendSegment(OneBot10MusicProvider provider, string id) : base(MessageSegmentTypes.Music)
    {
        if (provider != OneBot10MusicProvider.QQ &&
            provider != OneBot10MusicProvider.NetEase &&
            provider != OneBot10MusicProvider.Xiami)
        {
            throw new ArgumentOutOfRangeException(nameof(provider));
        }

        Provider = provider;
        Id = Require(id, nameof(id));
    }

    /// <summary>Gets the provider. / 获取音乐平台。</summary>
    public OneBot10MusicProvider Provider { get; }

    /// <summary>Gets the provider song ID. / 获取平台歌曲 ID。</summary>
    public string Id { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject
    {
        ["type"] = Provider == OneBot10MusicProvider.QQ
            ? "qq"
            : Provider == OneBot10MusicProvider.NetEase ? "163" : "xm",
        ["id"] = Id
    };
}
