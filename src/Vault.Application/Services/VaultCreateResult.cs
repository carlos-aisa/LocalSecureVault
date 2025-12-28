using Vault.Application.Abstractions;

namespace Vault.Application.Services;

public sealed record VaultCreateResult(
    VaultFile File,
    byte[] SessionKey
);
