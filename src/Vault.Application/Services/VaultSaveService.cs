using System.Threading;
using System.Threading.Tasks;
using Vault.Application.Abstractions;
using Vault.Application.Models;

namespace Vault.Application.Services;

public sealed class VaultSaveService
{
    private readonly IVaultStore _store;
    private readonly VaultCryptoService _crypto;

    public VaultSaveService(IVaultStore store, VaultCryptoService crypto)
    {
        _store = store;
        _crypto = crypto;
    }

    public async Task SaveAsync(
        string path,
        VaultDocument document,
        byte[] sessionKey,
        VaultFileHeader currentHeader,
        CancellationToken ct = default)
    {
        var file = _crypto.SealForSave(document, currentHeader, sessionKey);
        await _store.WriteAtomicAsync(path, file, ct);
    }
}
