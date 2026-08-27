using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12.Responses;

namespace OneBotSdk.Net.V12.Client;

public sealed partial class OneBot12Client
{
    /// <summary>Uploads a complete file from URL, path, or inline bytes. / 从 URL、路径或内联字节上传完整文件。</summary>
    public Task<OneBot12Response<OneBot12FileIdData>> UploadFileAsync(
        OneBot12UploadFileRequest request,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        return SendTypedAsync(
            OneBot12Actions.UploadFile,
            request.ToJsonObject(),
            OneBot12FileIdData.Parse,
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Calls the prepare stage of the standard <c>upload_file_fragmented</c> action. / 调用标准 <c>upload_file_fragmented</c> 动作的准备阶段。</summary>
    public Task<OneBot12Response<OneBot12FileIdData>> UploadFileFragmentedAsync(
        string name,
        long totalSize,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return PrepareUploadFileFragmentedAsync(name, totalSize, echo, self, cancellationToken);
    }

    /// <summary>Calls the transfer stage of the standard <c>upload_file_fragmented</c> action. / 调用标准 <c>upload_file_fragmented</c> 动作的传输阶段。</summary>
    public Task<OneBot12Response> UploadFileFragmentedAsync(
        string fileId,
        long offset,
        byte[] data,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return TransferUploadFileFragmentAsync(fileId, offset, data, echo, self, cancellationToken);
    }

    /// <summary>Calls the finish stage of the standard <c>upload_file_fragmented</c> action. / 调用标准 <c>upload_file_fragmented</c> 动作的结束阶段。</summary>
    public Task<OneBot12Response<OneBot12FileIdData>> UploadFileFragmentedAsync(
        string fileId,
        string sha256,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return FinishUploadFileFragmentedAsync(fileId, sha256, echo, self, cancellationToken);
    }

    /// <summary>Prepares a fragmented upload and returns its temporary file ID. / 准备分片上传并返回临时文件 ID。</summary>
    public Task<OneBot12Response<OneBot12FileIdData>> PrepareUploadFileFragmentedAsync(
        string name,
        long totalSize,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        ValidateNonNegative(totalSize, nameof(totalSize));
        return SendTypedAsync(
            OneBot12Actions.UploadFileFragmented,
            new JsonObject
            {
                ["stage"] = "prepare",
                ["name"] = Require(name, nameof(name)),
                ["total_size"] = totalSize
            },
            OneBot12FileIdData.Parse,
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Transfers one fragment of a prepared upload. / 传输已准备上传任务的一个分片。</summary>
    public Task<OneBot12Response> TransferUploadFileFragmentAsync(
        string fileId,
        long offset,
        byte[] data,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        ValidateNonNegative(offset, nameof(offset));
        if (data == null) throw new ArgumentNullException(nameof(data));
        return SendWithoutDataAsync(
            OneBot12Actions.UploadFileFragmented,
            new JsonObject
            {
                ["stage"] = "transfer",
                ["file_id"] = Require(fileId, nameof(fileId)),
                ["offset"] = offset,
                ["data"] = Convert.ToBase64String(data)
            },
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Finishes a fragmented upload and returns its final file ID. / 完成分片上传并返回最终文件 ID。</summary>
    public Task<OneBot12Response<OneBot12FileIdData>> FinishUploadFileFragmentedAsync(
        string fileId,
        string sha256,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot12Actions.UploadFileFragmented,
            new JsonObject
            {
                ["stage"] = "finish",
                ["file_id"] = Require(fileId, nameof(fileId)),
                ["sha256"] = Require(sha256, nameof(sha256))
            },
            OneBot12FileIdData.Parse,
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Gets a complete file in a standard URL, path, or inline-data representation. / 以标准 URL、路径或内联数据表示获取完整文件。</summary>
    public Task<OneBot12Response<OneBot12FileData>> GetFileAsync(
        string fileId,
        OneBot12FileAccessType type,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return GetFileAsync(fileId, ToProtocolValue(type), echo, self, cancellationToken);
    }

    /// <summary>Gets a complete file using a standard or implementation-defined representation. / 使用标准或实现扩展表示获取完整文件。</summary>
    public Task<OneBot12Response<OneBot12FileData>> GetFileAsync(
        string fileId,
        string type,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot12Actions.GetFile,
            new JsonObject
            {
                ["file_id"] = Require(fileId, nameof(fileId)),
                ["type"] = Require(type, nameof(type))
            },
            OneBot12FileData.Parse,
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Calls the prepare stage of the standard <c>get_file_fragmented</c> action. / 调用标准 <c>get_file_fragmented</c> 动作的准备阶段。</summary>
    public Task<OneBot12Response<OneBot12FileDownloadPreparationData>> GetFileFragmentedAsync(
        string fileId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return PrepareGetFileFragmentedAsync(fileId, echo, self, cancellationToken);
    }

    /// <summary>Calls the transfer stage of the standard <c>get_file_fragmented</c> action. / 调用标准 <c>get_file_fragmented</c> 动作的传输阶段。</summary>
    public Task<OneBot12Response<OneBot12FileFragmentData>> GetFileFragmentedAsync(
        string fileId,
        long offset,
        long size,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return GetFileFragmentAsync(fileId, offset, size, echo, self, cancellationToken);
    }

    /// <summary>Gets metadata needed before a fragmented download. / 获取分片下载前所需的元数据。</summary>
    public Task<OneBot12Response<OneBot12FileDownloadPreparationData>> PrepareGetFileFragmentedAsync(
        string fileId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot12Actions.GetFileFragmented,
            new JsonObject
            {
                ["stage"] = "prepare",
                ["file_id"] = Require(fileId, nameof(fileId))
            },
            OneBot12FileDownloadPreparationData.Parse,
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Gets one byte range from a fragmented download. / 获取分片下载的一个字节范围。</summary>
    public Task<OneBot12Response<OneBot12FileFragmentData>> GetFileFragmentAsync(
        string fileId,
        long offset,
        long size,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        ValidateNonNegative(offset, nameof(offset));
        ValidateNonNegative(size, nameof(size));
        return SendTypedAsync(
            OneBot12Actions.GetFileFragmented,
            new JsonObject
            {
                ["stage"] = "transfer",
                ["file_id"] = Require(fileId, nameof(fileId)),
                ["offset"] = offset,
                ["size"] = size
            },
            OneBot12FileFragmentData.Parse,
            echo,
            self,
            cancellationToken);
    }

    private static string ToProtocolValue(OneBot12FileAccessType value)
    {
        switch (value)
        {
            case OneBot12FileAccessType.Url: return "url";
            case OneBot12FileAccessType.Path: return "path";
            case OneBot12FileAccessType.Data: return "data";
            default: throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    private static void ValidateNonNegative(long value, string name)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(name, "The value cannot be negative.");
    }
}
