using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using DotNut.ApiModels;
using NBitcoin.Secp256k1;
using SHA256 = System.Security.Cryptography.SHA256;

namespace DotNut;

public static class MintQuoteSigner
{
    /// <summary>
    /// Domain separation tag, written as raw bytes without a length prefix.
    /// </summary>
    private static ReadOnlySpan<byte> DomainSeparator => "Cashu_MintQuoteSig_v1"u8;

    public static string SignMintQuote(
        this PrivKey pk,
        string quote,
        List<BlindedMessage> blindedMessages
    )
    {
        var msg = GetMessageToSign(quote, blindedMessages);
        var hash = SHA256.HashData(msg);
        return pk.Key.SignBIP340(hash).ToHex();
    }

    /// <summary>
    /// Builds the NUT-20 message committing to the quote id and every output:
    /// <c>"Cashu_MintQuoteSig_v1" || len32(quote) || quote || (len32(amount) || amount ||
    /// len32(B_) || B_)*</c>, where <c>len32</c> is a 32-bit big-endian byte length.
    /// </summary>
    internal static byte[] GetMessageToSign(string quote, IEnumerable<BlindedMessage> messages)
    {
        var writer = new ArrayBufferWriter<byte>(256);

        writer.Write(DomainSeparator);
        WriteLengthPrefixed(writer, Encoding.UTF8.GetBytes(quote));

        foreach (var blindedMessage in messages)
        {
            WriteLengthPrefixed(writer, ToMinimalBigEndian(blindedMessage.Amount));
            // The raw point, not its hex string.
            WriteLengthPrefixed(writer, blindedMessage.B_.Key.ToBytes());
        }

        return writer.WrittenSpan.ToArray();
    }

    private static void WriteLengthPrefixed(IBufferWriter<byte> writer, ReadOnlySpan<byte> value)
    {
        var span = writer.GetSpan(4 + value.Length);
        BinaryPrimitives.WriteUInt32BigEndian(span, (uint)value.Length);
        value.CopyTo(span[4..]);
        writer.Advance(4 + value.Length);
    }

    /// <summary>
    /// Canonical minimal big-endian encoding: no leading zero bytes, and zero is empty.
    /// </summary>
    internal static byte[] ToMinimalBigEndian(ulong amount)
    {
        if (amount == 0)
        {
            return [];
        }

        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(buffer, amount);

        var start = 0;
        while (buffer[start] == 0)
        {
            start++;
        }

        return buffer[start..].ToArray();
    }

    public static bool VerifySignature(this PostMintRequest quote, PubKey pk)
    {
        ArgumentNullException.ThrowIfNull(quote.Signature, nameof(quote.Signature));
        var msg = GetMessageToSign(quote.Quote, quote.Outputs);
        var hash = SHA256.HashData(msg);
        var xonly = pk.Key.ToXOnlyPubKey();
        if (!SecpSchnorrSignature.TryCreate(Convert.FromHexString(quote.Signature), out var sig))
        {
            return false;
        }
        return xonly.SigVerifyBIP340(sig, hash);
    }
}
