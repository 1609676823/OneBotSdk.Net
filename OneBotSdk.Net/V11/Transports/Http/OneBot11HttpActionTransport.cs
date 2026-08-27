using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Client;
using OneBotSdk.Net.V11.Json;
using OneBotSdk.Net.V11.Transports.Internal;

namespace OneBotSdk.Net.V11.Transports.Http;

/// <summary>
/// Sends standard OneBot actions to HTTP endpoints using JSON POST requests.
/// 使用 JSON POST 请求向 HTTP 终结点发送标准 OneBot 动作。
/// </summary>
public sealed class OneBot11HttpActionTransport : IOneBot11ActionTransport, IDisposable
{
    private readonly Uri _baseUri;
    private readonly string? _accessToken;
    private readonly int _maxResponseBodyBytes;
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private int _disposed;

    /// <summary>
    /// Initializes a transport with an internally owned <see cref="HttpClient"/>.
    /// 使用内部拥有的 <see cref="HttpClient"/> 初始化传输。
    /// </summary>
    public OneBot11HttpActionTransport(OneBot11HttpActionTransportOptions options)
        : this(options, null)
    {
    }

    /// <summary>
    /// Initializes a transport with an optional caller-owned <see cref="HttpClient"/>.
    /// 使用可选的调用方拥有的 <see cref="HttpClient"/> 初始化传输。
    /// </summary>
    /// <remarks>
    /// An injected client is never disposed by this transport.
    /// 注入的客户端永远不会由此传输释放。
    /// </remarks>
    public OneBot11HttpActionTransport(OneBot11HttpActionTransportOptions options, HttpClient? httpClient)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var snapshot = options.Snapshot();
        _baseUri = NormalizeBaseUri(snapshot.BaseUri);
        _accessToken = string.IsNullOrWhiteSpace(snapshot.AccessToken) ? null : snapshot.AccessToken;
        _maxResponseBodyBytes = snapshot.MaxResponseBodyBytes;
        _httpClient = httpClient ?? new HttpClient();
        _ownsHttpClient = httpClient == null;
    }

    /// <inheritdoc />
    public async Task<OneBot11ActionTransportResult> SendAsync(
        string action,
        JsonObject? parameters,
        JsonNode? echo,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        ValidateAction(action);
        cancellationToken.ThrowIfCancellationRequested();

        // HTTP action endpoints receive the params object directly; echo belongs to WebSocket envelopes only.
        // HTTP 动作终结点直接接收 params 对象；echo 仅属于 WebSocket 信封。
        _ = echo;
        var detachedParameters = OneBot11TransportPayload.Clone(parameters) as JsonObject ?? new JsonObject();
        var body = OneBot11Json.Serialize(detachedParameters);
        var endpoint = new Uri(_baseUri, Uri.EscapeDataString(action));

        using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
        using (var content = new StringContent(body, Encoding.UTF8, "application/json"))
        {
            request.Content = content;
            if (_accessToken != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }

            HttpResponseMessage response;
            try
            {
                response = await _httpClient
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw AttachTrace(
                    ForAction(
                        OneBot11TransportError.ConnectionFailed,
                        action,
                        "The HTTP OneBot action request could not be completed.",
                        exception),
                    detachedParameters,
                    body,
                    null);
            }

            using (response)
            {
                byte[] responseBody;
                try
                {
                    responseBody = await ReadResponseBodyAsync(response, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (OneBot11TransportException exception)
                {
                    exception.Action = action;
                    exception.HttpStatusCode = response.StatusCode;
                    AttachTrace(exception, detachedParameters, body, null);
                    throw;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var failure = ForAction(
                        OneBot11TransportError.HttpFailure,
                        action,
                        "The HTTP OneBot endpoint returned status " + (int)response.StatusCode + " (" + response.StatusCode + ").");
                    failure.HttpStatusCode = response.StatusCode;
                    throw AttachTrace(
                        failure,
                        detachedParameters,
                        body,
                        TryDecodeUtf8(responseBody));
                }

                string responseJson;
                try
                {
                    responseJson = OneBot11TransportPayload.DecodeUtf8(responseBody);
                }
                catch (OneBot11TransportException exception)
                {
                    exception.Action = action;
                    exception.HttpStatusCode = response.StatusCode;
                    AttachTrace(exception, detachedParameters, body, null);
                    throw;
                }

                JsonObject responseObject;
                try
                {
                    responseObject = OneBot11TransportPayload.ParseObject(responseJson);
                }
                catch (OneBot11TransportException exception)
                {
                    exception.Action = action;
                    exception.HttpStatusCode = response.StatusCode;
                    AttachTrace(exception, detachedParameters, body, responseJson);
                    throw;
                }

                return new OneBot11ActionTransportResult(
                    action,
                    detachedParameters,
                    null,
                    body,
                    responseObject,
                    responseJson);
            }
        }
    }

    /// <summary>
    /// Releases an internally owned HTTP client.
    /// 释放内部拥有的 HTTP 客户端。
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0 && _ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private async Task<byte[]> ReadResponseBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content == null)
        {
            return new byte[0];
        }

        Stream stream;
        try
        {
            // The CancellationToken overload is not available on all supported TFMs.
            // 并非所有支持的 TFM 都提供带 CancellationToken 的重载。
            stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw new OneBot11TransportException(
                OneBot11TransportError.ConnectionFailed,
                "The HTTP response body could not be opened.",
                exception);
        }

        using (stream)
        {
            return await OneBot11TransportPayload
                .ReadBoundedAsync(stream, _maxResponseBodyBytes, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static Uri NormalizeBaseUri(Uri baseUri)
    {
        var absolute = baseUri.AbsoluteUri;
        return absolute.EndsWith("/", StringComparison.Ordinal)
            ? baseUri
            : new Uri(absolute + "/", UriKind.Absolute);
    }

    private static void ValidateAction(string action)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("A OneBot action name is required.", nameof(action));
        }
    }

    private static OneBot11TransportException ForAction(
        OneBot11TransportError error,
        string action,
        string message,
        Exception? innerException = null)
    {
        var exception = innerException == null
            ? new OneBot11TransportException(error, message)
            : new OneBot11TransportException(error, message, innerException);
        exception.Action = action;
        return exception;
    }

    private static OneBot11TransportException AttachTrace(
        OneBot11TransportException exception,
        JsonObject requestParameters,
        string rawRequestJson,
        string? rawResponseJson)
    {
        exception.RequestParameters = OneBot11TransportPayload.Clone(requestParameters) as JsonObject ?? new JsonObject();
        exception.RequestEcho = null;
        exception.RawRequestJson = rawRequestJson;
        exception.RawResponseJson = rawResponseJson;
        return exception;
    }

    private static string? TryDecodeUtf8(byte[] payload)
    {
        try
        {
            return OneBot11TransportPayload.DecodeUtf8(payload);
        }
        catch (OneBot11TransportException)
        {
            return null;
        }
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            throw new ObjectDisposedException(nameof(OneBot11HttpActionTransport));
        }
    }
}
