namespace Vault.Application.Abstractions;

public sealed record VaultFile(
    VaultFileHeader Header,
    byte[] Ciphertext,
    byte[] Tag);
