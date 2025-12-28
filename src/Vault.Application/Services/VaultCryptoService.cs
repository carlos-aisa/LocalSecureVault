using System.Security.Cryptography;
using Vault.Application.Abstractions;
using Vault.Application.Models;

namespace Vault.Application.Services;

public sealed class VaultCryptoService
{
    private readonly ICryptoProvider _crypto;
    private readonly IVaultPayloadSerializer _serializer;

    public VaultCryptoService(
        ICryptoProvider crypto,
        IVaultPayloadSerializer  serializer)
    {
        _crypto = crypto ?? throw new ArgumentNullException(nameof(crypto));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public VaultCreateResult CreateVault(
        VaultDocument document,
        ReadOnlySpan<char> masterPassword,
        KdfProfile profile,
        DateTimeOffset? nowUtc = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        var now = nowUtc ?? DateTimeOffset.UtcNow;

        var kdfParams = _crypto.RecommendKdfParams(profile);
        var salt = RandomNumberGenerator.GetBytes(VaultFormatConstants.SaltSize);
        var key = _crypto.DeriveKey(masterPassword, salt, kdfParams);

        var plaintext = _serializer.SerializeToUtf8(document);

        var header = new VaultFileHeader(
            Magic: VaultFormatConstants.Magic,
            Version: VaultFormatConstants.Version,
            Flags: 0,
            KdfId: VaultFormatConstants.KdfIdArgon2id,
            PayloadEncoding: VaultFormatConstants.PayloadJsonUtf8,
            SchemaVersion: (ushort)document.Meta.SchemaVersion,
            Argon2MemoryKiB: kdfParams.MemoryKiB,
            Argon2Iterations: kdfParams.Iterations,
            Argon2Parallelism: kdfParams.Parallelism,
            Salt: salt,
            Nonce: new byte[VaultFormatConstants.NonceSize], // will be overwritten
            CreatedUtcTicks: now.UtcTicks,
            UpdatedUtcTicks: now.UtcTicks,
            Reserved: new byte[VaultFormatConstants.ReservedSize]
        );

        // AAD must use the header bytes, including the final nonce that will be stored
        // So: first encrypt to get nonce, then set it in header, then re-encrypt OR compute AAD with nonce.
        // We do the clean approach: set nonce first and encrypt once.
        var nonce = RandomNumberGenerator.GetBytes(VaultFormatConstants.NonceSize);
        header = header with { Nonce = nonce };

        var aad = VaultHeaderSerializer.SerializeHeader(header);

        // Encrypt using the nonce we already decided (to match header)
        var blob = EncryptWithNonce(plaintext, aad, key, nonce);

        var file = new VaultFile(header, blob.Ciphertext, blob.Tag);
        return new VaultCreateResult(file, key); 
    }

    public VaultUnlockResult UnlockVault(VaultFile file, ReadOnlySpan<char> masterPassword)
    {
        ArgumentNullException.ThrowIfNull(file);

        var h = file.Header;

        if (h.Magic != VaultFormatConstants.Magic)
            throw new InvalidOperationException("Invalid vault file.");

        var kdfParams = new KdfParams(h.Argon2MemoryKiB, h.Argon2Iterations, h.Argon2Parallelism, 32);
        var key = _crypto.DeriveKey(masterPassword, h.Salt, kdfParams);

        var aad = VaultHeaderSerializer.SerializeHeader(h);

        var blob = new EncryptedBlob(h.Nonce, file.Ciphertext, file.Tag);
        var plaintext = _crypto.Decrypt(blob, aad, key);

        var document =  _serializer.DeserializeFromUtf8(plaintext);
        return new VaultUnlockResult(document, key);
    }

    private EncryptedBlob EncryptWithNonce(
        ReadOnlySpan<byte> plaintext,
        ReadOnlySpan<byte> aad,
        ReadOnlySpan<byte> key,
        byte[] nonce)
    {
        // Use AES-GCM directly here to avoid changing ICryptoProvider.
        // (Alternative: extend ICryptoProvider to accept a caller-provided nonce.)
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[VaultFormatConstants.TagSize];

        using var aes = new AesGcm(key, VaultFormatConstants.TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag, aad);

        return new EncryptedBlob(nonce, ciphertext, tag);
    }

}
