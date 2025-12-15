namespace Vault.Application.Abstractions;

public interface IVaultStore
{
    Task<VaultFile> ReadAsync(string path, CancellationToken ct = default);

    Task WriteAtomicAsync(string path, VaultFile file, CancellationToken ct = default);
}
