using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12.Events;

namespace OneBotSdk.Net.V12.Transports.Http;

/// <summary>
/// Validates, parses, and dispatches framework-independent OneBot 12 HTTP Webhook requests.
/// 校验、解析并分发与 Web 框架无关的 OneBot 12 HTTP Webhook 请求。
/// </summary>
public sealed class OneBot12HttpWebhookIngress
{
    private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);
    private readonly OneBot12EventDispatcher _dispatcher;
    private readonly OneBot12HttpWebhookIngressOptions _options;

    /// <summary>
    /// Initializes an ingress adapter with a dispatcher and an immutable options snapshot.
    /// 使用分发器及不可变的选项快照初始化接入适配器。
    /// </summary>
    /// <param name="dispatcher">The dispatcher receiving successfully parsed events. / 接收已成功解析事件的分发器。</param>
    /// <param name="options">The header, authentication, and body-size policy. / 请求头、身份验证与正文大小策略。</param>
    public OneBot12HttpWebhookIngress(
        OneBot12EventDispatcher dispatcher,
        OneBot12HttpWebhookIngressOptions? options = null)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _options = (options ?? new OneBot12HttpWebhookIngressOptions()).Snapshot();
    }

    /// <summary>
    /// Reads one bounded strict-UTF-8 request body, validates its metadata, and dispatches the event.
    /// 读取一个受限的严格 UTF-8 请求正文，校验元数据并分发事件。
    /// </summary>
    /// <param name="requestBody">The caller-owned readable request stream. / 由调用方拥有的可读请求流。</param>
    /// <param name="metadata">The headers and query value captured by the web host. / Web 宿主捕获的请求头与查询参数值。</param>
    /// <param name="cancellationToken">Cancels body reading before dispatch. / 在分发之前取消正文读取。</param>
    /// <returns>The same parsed event instance delivered to subscribers. / 返回交付给订阅者的同一已解析事件实例。</returns>
    public async Task<OneBot12Event> ReadAndDispatchAsync(
        Stream requestBody,
        OneBot12HttpWebhookIngressMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        if (requestBody == null)
        {
            throw new ArgumentNullException(nameof(requestBody));
        }

        ValidateMetadata(metadata);
        var body = await ReadBoundedAsync(requestBody, cancellationToken).ConfigureAwait(false);
        return ParseAndDispatchCore(body);
    }

    /// <summary>
    /// Validates metadata, parses one complete strict-UTF-8 request body, and dispatches the event.
    /// 校验元数据，解析一个完整的严格 UTF-8 请求正文，并分发事件。
    /// </summary>
    /// <param name="requestBody">The complete request-body bytes. / 完整的请求正文字节。</param>
    /// <param name="metadata">The headers and query value captured by the web host. / Web 宿主捕获的请求头与查询参数值。</param>
    /// <returns>The same parsed event instance delivered to subscribers. / 返回交付给订阅者的同一已解析事件实例。</returns>
    public OneBot12Event ParseAndDispatch(
        byte[] requestBody,
        OneBot12HttpWebhookIngressMetadata metadata)
    {
        if (requestBody == null)
        {
            throw new ArgumentNullException(nameof(requestBody));
        }

        ValidateMetadata(metadata);
        return ParseAndDispatchCore(requestBody);
    }

    private OneBot12Event ParseAndDispatchCore(byte[] requestBody)
    {
        if (requestBody.Length > _options.MaxRequestBodyBytes)
        {
            throw new OneBot12TransportException(
                OneBot12TransportError.MessageTooLarge,
                "The OneBot 12 HTTP Webhook request exceeded the configured byte limit.");
        }

        string rawJson;
        try
        {
            rawJson = StrictUtf8.GetString(requestBody);
        }
        catch (DecoderFallbackException exception)
        {
            throw new OneBot12TransportException(
                OneBot12TransportError.ProtocolViolation,
                "The OneBot 12 HTTP Webhook request body is not valid strict UTF-8.",
                exception);
        }

        JsonObject source;
        try
        {
            source = JsonNode.Parse(rawJson) as JsonObject
                ?? throw new OneBot12TransportException(
                    OneBot12TransportError.ProtocolViolation,
                    "The OneBot 12 HTTP Webhook JSON root must be an object.");
        }
        catch (OneBot12TransportException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new OneBot12TransportException(
                OneBot12TransportError.ProtocolViolation,
                "The OneBot 12 HTTP Webhook request body is not valid JSON.",
                exception);
        }

        OneBot12Event parsed;
        try
        {
            parsed = OneBot12EventParser.Parse(source);
            _dispatcher.Dispatch(parsed);
        }
        catch (OneBot12TransportException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new OneBot12TransportException(
                OneBot12TransportError.ProtocolViolation,
                "The OneBot 12 HTTP Webhook event could not be parsed or dispatched.",
                exception);
        }

        return parsed;
    }

    private async Task<byte[]> ReadBoundedAsync(Stream requestBody, CancellationToken cancellationToken)
    {
        using (var buffer = new MemoryStream())
        {
            var chunk = new byte[Math.Min(16 * 1024, _options.MaxRequestBodyBytes)];
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int read;
                try
                {
                    read = await requestBody
                        .ReadAsync(chunk, 0, chunk.Length, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    throw new OneBot12TransportException(
                        OneBot12TransportError.ConnectionFailed,
                        "The OneBot 12 HTTP Webhook request body could not be read.",
                        exception);
                }
                if (read == 0)
                {
                    return buffer.ToArray();
                }

                if (buffer.Length + read > _options.MaxRequestBodyBytes)
                {
                    throw new OneBot12TransportException(
                        OneBot12TransportError.MessageTooLarge,
                        "The OneBot 12 HTTP Webhook request exceeded the configured byte limit.");
                }

                buffer.Write(chunk, 0, read);
            }
        }
    }

    private void ValidateMetadata(OneBot12HttpWebhookIngressMetadata metadata)
    {
        if (metadata == null)
        {
            throw new ArgumentNullException(nameof(metadata));
        }

        if (_options.RequireStandardHeaders)
        {
            MediaTypeHeaderValue? contentType;
            if (!MediaTypeHeaderValue.TryParse(metadata.ContentType, out contentType) ||
                contentType == null ||
                !string.Equals(contentType.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                throw new OneBot12TransportException(
                    OneBot12TransportError.ProtocolViolation,
                    "The OneBot 12 HTTP Webhook Content-Type must be application/json.");
            }

            if (string.IsNullOrWhiteSpace(metadata.UserAgent) ||
                !string.Equals(metadata.OneBotVersion, "12", StringComparison.Ordinal) ||
                !IsValidImplementationName(metadata.Implementation))
            {
                throw new OneBot12TransportException(
                    OneBot12TransportError.ProtocolViolation,
                    "The OneBot 12 HTTP Webhook requires User-Agent, X-OneBot-Version: 12, and X-Impl headers.");
            }
        }

        if (_options.AccessToken != null && !HasAccessToken(metadata, _options.AccessToken))
        {
            throw new OneBot12TransportException(
                OneBot12TransportError.AuthenticationFailed,
                "The OneBot 12 HTTP Webhook access token is missing or invalid.");
        }
    }

    private static bool HasAccessToken(
        OneBot12HttpWebhookIngressMetadata metadata,
        string expectedAccessToken)
    {
        // An explicitly supplied header always wins; a wrong header cannot be bypassed by a query token.
        // 显式提供的请求头始终优先；错误请求头不得通过查询令牌绕过。
        return metadata.Authorization != null
            ? FixedTimeEquals(metadata.Authorization, "Bearer " + expectedAccessToken)
            : FixedTimeEquals(metadata.AccessTokenQuery, expectedAccessToken);
    }

    private static bool FixedTimeEquals(string? left, string right)
    {
        if (left == null)
        {
            return false;
        }

        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        var difference = leftBytes.Length ^ rightBytes.Length;
        var length = Math.Max(leftBytes.Length, rightBytes.Length);
        for (var index = 0; index < length; index++)
        {
            var leftByte = index < leftBytes.Length ? leftBytes[index] : (byte)0;
            var rightByte = index < rightBytes.Length ? rightBytes[index] : (byte)0;
            difference |= leftByte ^ rightByte;
        }

        return difference == 0;
    }

    private static bool IsValidImplementationName(string? value)
    {
        if (string.IsNullOrEmpty(value) || value![0] < 'a' || value[0] > 'z')
        {
            return false;
        }

        var previousWasDot = false;
        for (var index = 1; index < value.Length; index++)
        {
            var character = value[index];
            if (character == '.')
            {
                if (previousWasDot || index == value.Length - 1)
                {
                    return false;
                }

                previousWasDot = true;
                continue;
            }

            if (!((character >= 'a' && character <= 'z') ||
                  (character >= '0' && character <= '9') ||
                  character == '-'))
            {
                return false;
            }

            previousWasDot = false;
        }

        return true;
    }
}
