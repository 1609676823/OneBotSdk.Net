using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.V11.Transports.Internal;

/// <summary>
/// Keeps payload size enforcement and strict UTF-8 parsing identical across transports.
/// 让不同传输使用一致的载荷大小限制和严格 UTF-8 解析行为。
/// </summary>
internal static class OneBot11TransportPayload
{
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);

    internal static async Task<byte[]> ReadBoundedAsync(Stream source, int maximumBytes, CancellationToken cancellationToken)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (source.CanSeek)
        {
            try
            {
                var remaining = source.Length - source.Position;
                if (remaining > maximumBytes)
                {
                    throw TooLarge(maximumBytes);
                }
            }
            catch (NotSupportedException)
            {
                // Some streams report CanSeek but do not expose Length; enforce the limit while reading.
                // 某些流虽然报告可定位却不支持 Length；此时在读取过程中执行大小限制。
            }
        }

        var buffer = new byte[Math.Min(16 * 1024, maximumBytes)];
        using (var destination = new MemoryStream())
        {
            while (true)
            {
                var count = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                if (count == 0)
                {
                    return destination.ToArray();
                }

                if (destination.Length + count > maximumBytes)
                {
                    throw TooLarge(maximumBytes);
                }

                destination.Write(buffer, 0, count);
            }
        }
    }

    internal static string DecodeUtf8(byte[] payload)
    {
        try
        {
            return StrictUtf8.GetString(payload);
        }
        catch (DecoderFallbackException exception)
        {
            throw new OneBot11TransportException(
                OneBot11TransportError.ProtocolViolation,
                "The transport payload is not valid UTF-8.",
                exception);
        }
    }

    internal static JsonObject ParseObject(byte[] payload)
    {
        return ParseObject(DecodeUtf8(payload));
    }

    internal static JsonObject ParseObject(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new OneBot11TransportException(
                OneBot11TransportError.ProtocolViolation,
                "The transport returned an empty JSON payload.");
        }

        try
        {
            var root = OneBot11Json.Parse(payload) as JsonObject;
            if (root == null)
            {
                throw new OneBot11TransportException(
                    OneBot11TransportError.ProtocolViolation,
                    "A OneBot transport payload must have a JSON object root.");
            }

            return root;
        }
        catch (OneBot11TransportException)
        {
            throw;
        }
        catch (JsonException exception)
        {
            throw new OneBot11TransportException(
                OneBot11TransportError.ProtocolViolation,
                "The transport payload is not valid JSON.",
                exception);
        }
    }

    internal static JsonNode? Clone(JsonNode? source)
    {
        if (source == null)
        {
            return null;
        }

        // A JsonNode can have only one parent, so transport envelopes must own a detached copy.
        // JsonNode 只能拥有一个父节点，因此传输信封必须使用独立副本。
        return OneBot11Json.Clone(source);
    }

    internal static OneBot11TransportException TooLarge(int maximumBytes)
    {
        return new OneBot11TransportException(
            OneBot11TransportError.MessageTooLarge,
            "The transport payload exceeded the configured limit of " + maximumBytes + " bytes.");
    }
}
