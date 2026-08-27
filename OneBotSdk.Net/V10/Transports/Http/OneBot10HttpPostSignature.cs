using System;
using System.Security.Cryptography;
using System.Text;

namespace OneBotSdk.Net.V10.Transports.Http;

/// <summary>
/// Computes and verifies the OneBot 10 reverse HTTP <c>X-Signature</c> value.
/// 计算并验证 OneBot 10 反向 HTTP 的 <c>X-Signature</c> 值。
/// </summary>
public static class OneBot10HttpPostSignature
{
    private const string Prefix = "sha1=";
    private static readonly char[] LowerHex = "0123456789abcdef".ToCharArray();

    /// <summary>
    /// Computes an HMAC-SHA1 signature over the exact request body bytes.
    /// 对原始请求正文的精确字节计算 HMAC-SHA1 签名。
    /// </summary>
    public static string Compute(byte[] requestBody, string secret)
    {
        if (requestBody == null)
        {
            throw new ArgumentNullException(nameof(requestBody));
        }

        if (secret == null)
        {
            throw new ArgumentNullException(nameof(secret));
        }

        var key = Encoding.UTF8.GetBytes(secret);
        byte[] digest;
        using (var hmac = new HMACSHA1(key))
        {
            digest = hmac.ComputeHash(requestBody);
        }

        return Prefix + ToLowerHex(digest);
    }

    /// <summary>
    /// Verifies a signature using a fixed-time digest comparison.
    /// 使用固定时间的摘要比较验证签名。
    /// </summary>
    public static bool Verify(byte[] requestBody, string? signatureHeader, string secret)
    {
        if (requestBody == null)
        {
            throw new ArgumentNullException(nameof(requestBody));
        }

        if (secret == null)
        {
            throw new ArgumentNullException(nameof(secret));
        }

        if (signatureHeader == null || string.IsNullOrWhiteSpace(signatureHeader))
        {
            return false;
        }

        var value = signatureHeader.Trim();
        if (!value.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        byte[] supplied;
        if (!TryParseHex(value.Substring(Prefix.Length), out supplied))
        {
            return false;
        }

        var key = Encoding.UTF8.GetBytes(secret);
        byte[] expected;
        using (var hmac = new HMACSHA1(key))
        {
            expected = hmac.ComputeHash(requestBody);
        }

        return FixedTimeEquals(expected, supplied);
    }

    internal static bool FixedTimeEquals(byte[] expected, byte[] supplied)
    {
        // Keep the comparison independent from secret byte values on legacy frameworks.
        // 在旧框架上也让比较耗时不依赖密钥字节的具体值。
        var difference = expected.Length ^ supplied.Length;
        var length = Math.Max(expected.Length, supplied.Length);
        for (var index = 0; index < length; index++)
        {
            var left = index < expected.Length ? expected[index] : (byte)0;
            var right = index < supplied.Length ? supplied[index] : (byte)0;
            difference |= left ^ right;
        }

        return difference == 0;
    }

    private static string ToLowerHex(byte[] bytes)
    {
        var chars = new char[bytes.Length * 2];
        for (var index = 0; index < bytes.Length; index++)
        {
            chars[index * 2] = LowerHex[bytes[index] >> 4];
            chars[index * 2 + 1] = LowerHex[bytes[index] & 0x0f];
        }

        return new string(chars);
    }

    private static bool TryParseHex(string value, out byte[] bytes)
    {
        if (value.Length == 0 || value.Length % 2 != 0)
        {
            bytes = new byte[0];
            return false;
        }

        bytes = new byte[value.Length / 2];
        for (var index = 0; index < bytes.Length; index++)
        {
            var high = HexValue(value[index * 2]);
            var low = HexValue(value[index * 2 + 1]);
            if (high < 0 || low < 0)
            {
                bytes = new byte[0];
                return false;
            }

            bytes[index] = (byte)((high << 4) | low);
        }

        return true;
    }

    private static int HexValue(char value)
    {
        if (value >= '0' && value <= '9')
        {
            return value - '0';
        }

        if (value >= 'a' && value <= 'f')
        {
            return value - 'a' + 10;
        }

        if (value >= 'A' && value <= 'F')
        {
            return value - 'A' + 10;
        }

        return -1;
    }
}
