using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12.Client;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Transports.Http;

/// <summary>Sends complete OneBot 12 action envelopes as JSON POST requests to the root endpoint. / 将完整 OneBot 12 动作信封作为 JSON POST 请求发送到根终结点。</summary>
public sealed class OneBot12HttpActionTransport : IOneBot12ActionTransport, IDisposable
{
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);
    private readonly Uri _endpoint;
    private readonly AuthenticationHeaderValue? _authorization;
    private readonly int _maxResponseBodyBytes;
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private int _disposed;

    /// <summary>Initializes a transport with an internally owned HttpClient. / 使用内部拥有的 HttpClient 初始化传输。</summary>
    public OneBot12HttpActionTransport(OneBot12HttpActionTransportOptions options)
        : this(options, null)
    {
    }

    /// <summary>Initializes a transport with an optional caller-owned HttpClient. / 使用可选的调用方拥有的 HttpClient 初始化传输。</summary>
    public OneBot12HttpActionTransport(OneBot12HttpActionTransportOptions options, HttpClient? httpClient)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        var snapshot = options.Snapshot();
        _endpoint = CreateAuthenticatedEndpoint(snapshot.Endpoint, snapshot.AccessToken, out _authorization);
        _maxResponseBodyBytes = snapshot.MaxResponseBodyBytes;
        _httpClient = httpClient ?? new HttpClient();
        _ownsHttpClient = httpClient == null;
    }

    /// <inheritdoc />
    public async Task<OneBot12ActionTransportResult> SendAsync(
        string action,
        JsonObject? parameters,
        string? echo,
        OneBot12Self? self,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("A OneBot action name is required.", nameof(action));
        cancellationToken.ThrowIfCancellationRequested();

        var detachedParameters = TolerantJson.Clone(parameters) as JsonObject ?? new JsonObject();
        var detachedSelf = self?.Clone();
        var envelope = new JsonObject
        {
            ["action"] = action,
            ["params"] = TolerantJson.Clone(detachedParameters)
        };
        if (echo != null) envelope["echo"] = echo;
        if (detachedSelf != null) envelope["self"] = detachedSelf.ToJsonObject();
        var rawRequestJson = OneBot12Json.Serialize(envelope);

        using (var request = new HttpRequestMessage(HttpMethod.Post, _endpoint))
        using (var content = new StringContent(rawRequestJson, Encoding.UTF8, "application/json"))
        {
            request.Content = content;
            if (_authorization != null)
            {
                request.Headers.Authorization = _authorization;
            }

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw AttachTrace(
                    new OneBot12TransportException(
                        OneBot12TransportError.ConnectionFailed,
                        "The HTTP OneBot 12 action request could not be completed.",
                        exception),
                    action,
                    detachedParameters,
                    echo,
                    detachedSelf,
                    rawRequestJson,
                    null);
            }

            using (response)
            {
                byte[] responseBytes;
                try
                {
                    responseBytes = await ReadBoundedAsync(response, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (OneBot12TransportException exception)
                {
                    exception.HttpStatusCode = response.StatusCode;
                    throw AttachTrace(exception, action, detachedParameters, echo, detachedSelf, rawRequestJson, null);
                }

                var rawResponseJson = TryDecode(responseBytes);
                if (!response.IsSuccessStatusCode)
                {
                    var failure = new OneBot12TransportException(
                        OneBot12TransportError.HttpFailure,
                        "The HTTP OneBot 12 endpoint returned status " + (int)response.StatusCode + " (" + response.StatusCode + ").")
                    {
                        HttpStatusCode = response.StatusCode
                    };
                    throw AttachTrace(failure, action, detachedParameters, echo, detachedSelf, rawRequestJson, rawResponseJson);
                }

                if (rawResponseJson == null)
                {
                    throw AttachTrace(
                        new OneBot12TransportException(OneBot12TransportError.ProtocolViolation, "The response is not valid strict UTF-8."),
                        action,
                        detachedParameters,
                        echo,
                        detachedSelf,
                        rawRequestJson,
                        null);
                }

                JsonObject responseObject;
                try
                {
                    responseObject = JsonNode.Parse(rawResponseJson) as JsonObject
                        ?? throw new OneBot12TransportException(OneBot12TransportError.ProtocolViolation, "The response JSON root must be an object.");
                }
                catch (OneBot12TransportException exception)
                {
                    throw AttachTrace(exception, action, detachedParameters, echo, detachedSelf, rawRequestJson, rawResponseJson);
                }
                catch (Exception exception)
                {
                    throw AttachTrace(
                        new OneBot12TransportException(OneBot12TransportError.ProtocolViolation, "The response is not valid JSON.", exception),
                        action,
                        detachedParameters,
                        echo,
                        detachedSelf,
                        rawRequestJson,
                        rawResponseJson);
                }

                return new OneBot12ActionTransportResult(
                    action,
                    detachedParameters,
                    echo,
                    detachedSelf,
                    rawRequestJson,
                    responseObject,
                    rawResponseJson);
            }
        }
    }

    /// <summary>Releases only an internally owned HttpClient. / 仅释放内部拥有的 HttpClient。</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0 && _ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private async Task<byte[]> ReadBoundedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content == null) return new byte[0];
        Stream stream;
        try
        {
            stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw new OneBot12TransportException(OneBot12TransportError.ConnectionFailed, "The response body could not be opened.", exception);
        }

        using (stream)
        using (var buffer = new MemoryStream())
        {
            var chunk = new byte[16 * 1024];
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var read = await stream.ReadAsync(chunk, 0, chunk.Length, cancellationToken).ConfigureAwait(false);
                if (read == 0) break;
                if (buffer.Length + read > _maxResponseBodyBytes)
                {
                    throw new OneBot12TransportException(OneBot12TransportError.MessageTooLarge, "The response exceeded the configured size limit.");
                }

                buffer.Write(chunk, 0, read);
            }

            return buffer.ToArray();
        }
    }

    private static string? TryDecode(byte[] bytes)
    {
        try { return StrictUtf8.GetString(bytes); }
        catch (DecoderFallbackException) { return null; }
    }

    private static Uri CreateAuthenticatedEndpoint(
        Uri endpoint,
        string? accessToken,
        out AuthenticationHeaderValue? authorization)
    {
        if (accessToken == null)
        {
            authorization = null;
            return endpoint;
        }

        if (CanUseBearerHeader(accessToken))
        {
            authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return endpoint;
        }

        // HTTP headers cannot represent every non-empty protocol token exactly; the standard query fallback can.
        // HTTP 请求头无法精确表示所有非空协议令牌；此时使用标准定义的查询参数回退。
        authorization = null;
        var builder = new UriBuilder(endpoint)
        {
            Query = "access_token=" + Uri.EscapeDataString(accessToken)
        };
        return builder.Uri;
    }

    private static bool CanUseBearerHeader(string accessToken)
    {
        // RFC token68 is a conservative subset that AuthenticationHeaderValue writes without quoting or trimming.
        // RFC token68 是 AuthenticationHeaderValue 无需引号或裁剪即可写出的保守字符子集。
        var paddingStarted = false;
        for (var index = 0; index < accessToken.Length; index++)
        {
            var character = accessToken[index];
            if (character == '=')
            {
                paddingStarted = true;
                continue;
            }

            if (paddingStarted ||
                !((character >= 'a' && character <= 'z') ||
                  (character >= 'A' && character <= 'Z') ||
                  (character >= '0' && character <= '9') ||
                  character == '-' || character == '.' || character == '_' || character == '~' ||
                  character == '+' || character == '/'))
            {
                return false;
            }
        }

        return accessToken.Length > 0;
    }

    private static OneBot12TransportException AttachTrace(
        OneBot12TransportException exception,
        string action,
        JsonObject parameters,
        string? echo,
        OneBot12Self? self,
        string rawRequestJson,
        string? rawResponseJson)
    {
        exception.Action = action;
        exception.RequestParameters = TolerantJson.CloneObject(parameters);
        exception.RequestEcho = echo;
        exception.RequestSelf = self?.Clone();
        exception.RawRequestJson = rawRequestJson;
        exception.RawResponseJson = rawResponseJson;
        return exception;
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed) != 0) throw new ObjectDisposedException(nameof(OneBot12HttpActionTransport));
    }
}
