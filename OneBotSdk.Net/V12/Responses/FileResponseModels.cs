using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Responses;

/// <summary>Contains a file identifier produced by upload actions. / 包含上传动作生成的文件标识。</summary>
public sealed class OneBot12FileIdData : OneBot12JsonModel
{
    private OneBot12FileIdData(JsonObject raw, string? fileId) : base(raw) => FileId = fileId;

    /// <summary>Gets the reusable file ID. / 获取可复用的文件 ID。</summary>
    [JsonPropertyName("file_id")]
    public string? FileId { get; }

    internal static OneBot12FileIdData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null ? null : new OneBot12FileIdData(
            TolerantJson.CloneObject(source),
            TolerantJson.String(source, "file_id"));
    }
}

/// <summary>Contains a file returned as URL, path, or inline bytes. / 包含以 URL、路径或内联字节返回的文件。</summary>
public sealed class OneBot12FileData : OneBot12JsonModel
{
    private OneBot12FileData(
        JsonObject raw,
        string? name,
        string? url,
        IReadOnlyDictionary<string, string> headers,
        string? path,
        byte[]? data,
        string? sha256) : base(raw)
    {
        Name = name;
        Url = url;
        Headers = headers;
        Path = path;
        Data = data;
        Sha256 = sha256;
    }

    /// <summary>Gets the file name. / 获取文件名。</summary>
    [JsonPropertyName("name")]
    public string? Name { get; }

    /// <summary>Gets the downloadable URL when requested. / 请求 URL 表示时获取下载 URL。</summary>
    [JsonPropertyName("url")]
    public string? Url { get; }

    /// <summary>Gets headers required to download the URL. / 获取下载 URL 所需的请求头。</summary>
    [JsonPropertyName("headers")]
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Gets the implementation-visible path when requested. / 请求路径表示时获取实现端可访问路径。</summary>
    [JsonPropertyName("path")]
    public string? Path { get; }

    /// <summary>Gets decoded inline bytes when requested and valid. / 请求内联数据且格式有效时获取解码字节。</summary>
    [JsonPropertyName("data")]
    public byte[]? Data { get; }

    /// <summary>Gets the optional lowercase SHA-256 checksum. / 获取可选的小写 SHA-256 校验和。</summary>
    [JsonPropertyName("sha256")]
    public string? Sha256 { get; }

    internal static OneBot12FileData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null ? null : new OneBot12FileData(
            TolerantJson.CloneObject(source),
            TolerantJson.String(source, "name"),
            TolerantJson.String(source, "url"),
            TolerantJson.StringMap(TolerantJson.Node(source, "headers")),
            TolerantJson.String(source, "path"),
            TolerantJson.Bytes(source, "data"),
            TolerantJson.String(source, "sha256"));
    }
}

/// <summary>Contains metadata returned before a fragmented download. / 包含分片下载准备阶段返回的元数据。</summary>
public sealed class OneBot12FileDownloadPreparationData : OneBot12JsonModel
{
    private OneBot12FileDownloadPreparationData(JsonObject raw, string? name, long? size, string? sha256) : base(raw)
    {
        Name = name;
        TotalSize = size;
        Sha256 = sha256;
    }

    /// <summary>Gets the file name. / 获取文件名。</summary>
    [JsonPropertyName("name")]
    public string? Name { get; }

    /// <summary>Gets the complete byte size. / 获取完整字节大小。</summary>
    [JsonPropertyName("total_size")]
    public long? TotalSize { get; }

    /// <summary>Gets the complete-file SHA-256 checksum. / 获取完整文件 SHA-256 校验和。</summary>
    [JsonPropertyName("sha256")]
    public string? Sha256 { get; }

    internal static OneBot12FileDownloadPreparationData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null ? null : new OneBot12FileDownloadPreparationData(
            TolerantJson.CloneObject(source),
            TolerantJson.String(source, "name"),
            TolerantJson.Int64(source, "total_size"),
            TolerantJson.String(source, "sha256"));
    }
}

/// <summary>Contains bytes returned by one fragmented-download transfer. / 包含一次分片下载传输返回的字节。</summary>
public sealed class OneBot12FileFragmentData : OneBot12JsonModel
{
    private OneBot12FileFragmentData(JsonObject raw, byte[]? data) : base(raw) => Data = data;

    /// <summary>Gets decoded fragment bytes when valid. / 格式有效时获取解码后的分片字节。</summary>
    [JsonPropertyName("data")]
    public byte[]? Data { get; }

    internal static OneBot12FileFragmentData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null ? null : new OneBot12FileFragmentData(
            TolerantJson.CloneObject(source),
            TolerantJson.Bytes(source, "data"));
    }
}
