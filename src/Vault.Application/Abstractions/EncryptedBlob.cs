namespace Vault.Application.Abstractions;

public sealed record EncryptedBlob(
    byte[] Nonce,       // 12 bytes
    byte[] Ciphertext,  // N bytes
    byte[] Tag          // 16 bytes
);
