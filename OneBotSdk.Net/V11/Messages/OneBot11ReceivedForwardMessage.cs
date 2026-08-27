using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>
/// Represents the strongly typed node chain returned specifically by <c>get_forward_msg</c>.
/// 表示由 <c>get_forward_msg</c> 专门返回的强类型节点链。
/// </summary>
public sealed class OneBot11ReceivedForwardMessage : IReadOnlyList<ForwardNodeReceivedSegment>
{
    private readonly IReadOnlyList<ForwardNodeReceivedSegment> _nodes;

    private OneBot11ReceivedForwardMessage(
        IReadOnlyList<ForwardNodeReceivedSegment> nodes,
        JsonArray rawJson)
    {
        _nodes = nodes;
        RawJson = rawJson;
    }

    /// <summary>Gets a new empty received forward-message chain. / 获取一个新的空入站合并转发消息链。</summary>
    public static OneBot11ReceivedForwardMessage Empty => new OneBot11ReceivedForwardMessage(
        Array.Empty<ForwardNodeReceivedSegment>(),
        new JsonArray());

    /// <summary>Gets an independent copy of the complete returned node array. / 获取完整返回节点数组的独立副本。</summary>
    [JsonIgnore]
    public JsonArray RawJson { get; }

    /// <inheritdoc />
    public int Count => _nodes.Count;

    /// <inheritdoc />
    public ForwardNodeReceivedSegment this[int index] => _nodes[index];

    /// <summary>
    /// Parses only custom node segments from the official array response while retaining the complete raw array.
    /// 仅从官方数组响应解析自定义节点消息段，同时保留完整原始数组。
    /// </summary>
    public static OneBot11ReceivedForwardMessage? Parse(JsonNode? node)
    {
        var source = TolerantJson.Array(node);
        if (source == null)
        {
            return null;
        }

        var nodes = new List<ForwardNodeReceivedSegment>();
        var count = TolerantJson.Count(source);
        for (var index = 0; index < count; index++)
        {
            try
            {
                if (OneBot11ReceivedSegment.Parse(TolerantJson.Item(source, index)) is ForwardNodeReceivedSegment forwardNode)
                {
                    nodes.Add(forwardNode);
                }
            }
            catch (Exception)
            {
                // Continue parsing the remaining forward nodes.
                // 继续解析其余转发节点。
            }
        }

        return new OneBot11ReceivedForwardMessage(
            nodes.AsReadOnly(),
            TolerantJson.Clone(source) as JsonArray ?? new JsonArray());
    }

    /// <inheritdoc />
    public IEnumerator<ForwardNodeReceivedSegment> GetEnumerator() => _nodes.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
