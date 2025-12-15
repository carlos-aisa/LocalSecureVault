using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Vault.Application.Abstractions;

namespace Vault.Crypto;

public sealed class CryptoProvider : ICryptoProvider
{
    public KdfParams RecommendKdfParams(KdfProfile profile)
    {
        // Conservative defaults. Tune later with a local benchmark.
        return profile switch
        {
            KdfProfile.Interactive => new KdfParams(
                MemoryKiB: 64 * 1024,       // 64 MiB
                Iterations: 2,
                Parallelism: (ushort)Math.Min(2, Environment.ProcessorCount),
                KeyLengthBytes: 32),

            KdfProfile.Strong => new KdfParams(
                MemoryKiB: 256 * 1024,      // 256 MiB
                Iterations: 3,
                Parallelism: (ushort)Math.Min(4, Environment.ProcessorCount),
                KeyLengthBytes: 32),

            _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null)
        };
    }

    public byte[] DeriveKey(ReadOnlySpan<char> masterPassword, ReadOnlySpan<byte> salt, KdfParams kdfParams)
    {
        if (salt.Length != VaultFormatConstants.SaltSize)
            throw new ArgumentException($"Salt must be {VaultFormatConstants.SaltSize} bytes.", nameof(salt));

        if (kdfParams.KeyLengthBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(kdfParams), "Key length must be > 0.");

        // Convert char password to UTF-8 bytes (best-effort; secure string handling later).
        var passwordBytes = Encoding.UTF8.GetBytes(masterPassword.ToArray());

        try
        {
            using var argon2 = new Argon2id(passwordBytes)
            {
                Salt = salt.ToArray(),
                DegreeOfParallelism = kdfParams.Parallelism,
                Iterations = checked((int)kdfParams.Iterations),
                MemorySize = checked((int)kdfParams.MemoryKiB) // in KiB for this library
            };

            return argon2.GetBytes(kdfParams.KeyLengthBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
        }
    }

    public EncryptedBlob Encrypt(ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> aad, ReadOnlySpan<byte> key)
    {
        if (key.Length != 32)
            throw new ArgumentException("Key must be 32 bytes (AES-256).", nameof(key));

        var nonce = RandomNumberGenerator.GetBytes(VaultFormatConstants.NonceSize);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[VaultFormatConstants.TagSize];

        using var aes = new AesGcm(key, VaultFormatConstants.TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag, aad);

        return new EncryptedBlob(nonce, ciphertext, tag);
    }

    public byte[] Decrypt(EncryptedBlob blob, ReadOnlySpan<byte> aad, ReadOnlySpan<byte> key)
    {
        if (key.Length != 32)
            throw new ArgumentException("Key must be 32 bytes (AES-256).", nameof(key));

        if (blob.Nonce.Length != VaultFormatConstants.NonceSize)
            throw new ArgumentException($"Nonce must be {VaultFormatConstants.NonceSize} bytes.", nameof(blob));

        if (blob.Tag.Length != VaultFormatConstants.TagSize)
            throw new ArgumentException($"Tag must be {VaultFormatConstants.TagSize} bytes.", nameof(blob));

        var plaintext = new byte[blob.Ciphertext.Length];

        using var aes = new AesGcm(key, VaultFormatConstants.TagSize);
        aes.Decrypt(blob.Nonce, blob.Ciphertext, blob.Tag, plaintext, aad);

        return plaintext;
    }
}
