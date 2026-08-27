using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Json;

/// <summary>
/// Provides access to the unmodified JSON object from which a tolerant model was parsed.
/// 提供对容错模型解析来源的未改写 JSON 对象的访问。
/// </summary>
public abstract class OneBot10JsonModel
{
    /// <summary>
    /// Gets the original JSON object, including implementation-specific extension fields.
    /// 获取原始 JSON 对象，其中包含实现端特有的扩展字段。
    /// </summary>
    [JsonIgnore]
    public JsonObject RawJson { get; internal set; } = new JsonObject();
}
