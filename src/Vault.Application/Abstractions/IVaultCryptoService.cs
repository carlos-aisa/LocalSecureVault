using Vault.Application.Models;
using Vault.Application.Services;

namespace Vault.Application.Abstractions;

public interface IVaultCryptoService
{
    VaultCreateResult CreateVault(VaultDocument document,ReadOnlySpan<char> masterPassword,
                                 KdfProfile profile,DateTimeOffset? nowUtc = null);
    VaultUnlockResult UnlockVault(VaultFile file, ReadOnlySpan<char> masterPassword);

    VaultFile SealForSave(VaultDocument document,VaultFileHeader currentHeader,
                            ReadOnlySpan<byte> sessionKey,DateTimeOffset? nowUtc = null);           
}