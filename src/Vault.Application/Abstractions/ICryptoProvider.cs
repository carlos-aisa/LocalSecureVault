namespace Vault.Application.Abstractions;

public interface ICryptoProvider
{
    KdfParams RecommendKdfParams(KdfProfile profile);

    byte[] DeriveKey(
        ReadOnlySpan<char> masterPassword,
        ReadOnlySpan<byte> salt,
        KdfParams kdfParams);

    EncryptedBlob Encrypt(
        ReadOnlySpan<byte> plaintext,
        ReadOnlySpan<byte> aad,
        ReadOnlySpan<byte> key);

    byte[] Decrypt(
        EncryptedBlob blob,
        ReadOnlySpan<byte> aad,
        ReadOnlySpan<byte> key);
}
