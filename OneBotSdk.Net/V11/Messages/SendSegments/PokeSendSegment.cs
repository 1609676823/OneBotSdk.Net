using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents an outgoing poke. / 表示出站戳一戳消息。</summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public sealed class PokeSendSegment : OneBot11SendSegment
{
    /// <summary>Initializes a poke by its protocol type and ID. / 通过协议类型和 ID 初始化戳一戳。</summary>
    public PokeSendSegment(string pokeType, string id) : base(MessageSegmentTypes.Poke)
    {
        PokeType = Require(pokeType, nameof(pokeType));
        Id = Require(id, nameof(id));
    }

    /// <summary>Gets the poke type. / 获取戳一戳类型。</summary>
    public string PokeType { get; }

    /// <summary>Gets the poke ID. / 获取戳一戳 ID。</summary>
    public string Id { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject { ["type"] = PokeType, ["id"] = Id };
}
