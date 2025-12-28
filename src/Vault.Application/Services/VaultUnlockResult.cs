using Vault.Application.Models;

namespace Vault.Application.Services;

public sealed record VaultUnlockResult(
    VaultDocument Document,
    byte[] SessionKey
);
