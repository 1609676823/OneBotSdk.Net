using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Json;

/// <summary>
/// Retains the complete source object for tolerant OneBot 12 models.
/// 为按字段容错的 OneBot 12 模型保留完整源对象。
/// </summary>
public abstract class OneBot12JsonModel
{
    /// <summary>Initializes a model with an empty raw object. / 使用空原始对象初始化模型。</summary>
    protected OneBot12JsonModel()
        : this(new JsonObject())
    {
    }

    /// <summary>Initializes a model with a detached raw object. / 使用独立的原始对象初始化模型。</summary>
    protected OneBot12JsonModel(JsonObject rawJson)
    {
        RawJson = rawJson ?? new JsonObject();
    }

    /// <summary>Gets the complete detached source object, including implementation extensions. / 获取包含实现扩展的完整独立源对象。</summary>
    [JsonIgnore]
    public JsonObject RawJson { get; }
}
