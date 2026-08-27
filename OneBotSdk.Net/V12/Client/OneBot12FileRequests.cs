using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Client;

/// <summary>Identifies a standard complete-file access representation. / 标识标准的完整文件访问表示。</summary>
public enum OneBot12FileAccessType
{
    /// <summary>Requests an HTTP(S) URL. / 请求 HTTP(S) URL。</summary>
    Url,
    /// <summary>Requests an implementation-visible path. / 请求实现端可访问路径。</summary>
    Path,
    /// <summary>Requests inline Base64-encoded bytes. / 请求 Base64 编码的内联字节。</summary>
    Data
}

/// <summary>Describes one complete-file upload source. / 描述一个完整文件上传来源。</summary>
public sealed class OneBot12UploadFileRequest
{
    private readonly byte[]? _data;

    private OneBot12UploadFileRequest(
        string type,
        string name,
        string? url,
        IReadOnlyDictionary<string, string>? headers,
        string? path,
        byte[]? data,
        string? sha256)
    {
        Type = type;
        Name = name;
        Url = url;
        Headers = headers;
        Path = path;
        _data = data == null ? null : (byte[])data.Clone();
        Sha256 = sha256;
    }

    /// <summary>Gets the upload source discriminator. / 获取上传来源判别值。</summary>
    public string Type { get; }
    /// <summary>Gets the file name. / 获取文件名。</summary>
    public string Name { get; }
    /// <summary>Gets the optional source URL. / 获取可选来源 URL。</summary>
    public string? Url { get; }
    /// <summary>Gets optional download request headers. / 获取可选下载请求头。</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; }
    /// <summary>Gets the optional implementation-visible path. / 获取可选的实现端可访问路径。</summary>
    public string? Path { get; }
    /// <summary>Gets a copy of optional inline bytes. / 获取可选内联字节的副本。</summary>
    public byte[]? Data => _data == null ? null : (byte[])_data.Clone();
    /// <summary>Gets the optional lowercase SHA-256 checksum. / 获取可选的小写 SHA-256 校验和。</summary>
    public string? Sha256 { get; }

    /// <summary>Creates a URL-backed upload request. / 创建 URL 来源的上传请求。</summary>
    public static OneBot12UploadFileRequest FromUrl(
        string name,
        string url,
        IReadOnlyDictionary<string, string>? headers = null,
        string? sha256 = null)
    {
        return new OneBot12UploadFileRequest(
            "url",
            Require(name, nameof(name)),
            Require(url, nameof(url)),
            CopyHeaders(headers),
            null,
            null,
            sha256);
    }

    /// <summary>Creates an implementation-visible path upload request. / 创建实现端可访问路径来源的上传请求。</summary>
    public static OneBot12UploadFileRequest FromPath(string name, string path, string? sha256 = null)
    {
        return new OneBot12UploadFileRequest(
            "path",
            Require(name, nameof(name)),
            null,
            null,
            Require(path, nameof(path)),
            null,
            sha256);
    }

    /// <summary>Creates an inline-byte upload request. / 创建内联字节来源的上传请求。</summary>
    public static OneBot12UploadFileRequest FromData(string name, byte[] data, string? sha256 = null)
    {
        return new OneBot12UploadFileRequest(
            "data",
            Require(name, nameof(name)),
            null,
            null,
            null,
            data ?? throw new ArgumentNullException(nameof(data)),
            sha256);
    }

    internal JsonObject ToJsonObject()
    {
        var result = new JsonObject
        {
            ["type"] = Type,
            ["name"] = Name
        };
        if (Url != null) result["url"] = Url;
        if (Path != null) result["path"] = Path;
        if (_data != null) result["data"] = Convert.ToBase64String(_data);
        if (Sha256 != null) result["sha256"] = Sha256;
        if (Headers != null)
        {
            var headers = new JsonObject();
            foreach (var header in Headers)
            {
                headers[header.Key] = header.Value;
            }

            result["headers"] = headers;
        }

        return result;
    }

    private static string Require(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("A non-empty value is required.", name);
        return value!;
    }

    private static IReadOnlyDictionary<string, string>? CopyHeaders(IReadOnlyDictionary<string, string>? source)
    {
        if (source == null) return null;
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var header in source)
        {
            result[header.Key] = header.Value;
        }

        return result;
    }
}
