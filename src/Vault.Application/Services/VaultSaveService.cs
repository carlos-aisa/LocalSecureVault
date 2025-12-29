using System.Threading;
using System.Threading.Tasks;
using Vault.Application.Abstractions;
using Vault.Application.Models;
using System.Security.Cryptography;

namespace Vault.Application.Services;

public sealed class VaultSaveService
{
    private readonly IVaultStore _store;
    private readonly IVaultCryptoService _crypto;

    public VaultSaveService(IVaultStore store, IVaultCryptoService crypto)
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

    public async Task<VaultResult<Unit>> TrySaveAsync(
        string path,
        VaultDocument document,
        byte[] sessionKey,
        VaultFileHeader currentHeader,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return VaultResult<Unit>.Fail(new(VaultErrorCode.InvalidPath, "Select a vault file."));

        try
        {
            var file = _crypto.SealForSave(document, currentHeader, sessionKey);
            await _store.WriteAtomicAsync(path, file, ct);
            return VaultResult<Unit>.Ok(Unit.Value);
        }
        catch (UnauthorizedAccessException ex)
        {
            return VaultResult<Unit>.Fail(new(VaultErrorCode.AccessDenied, "Access denied while saving the vault.", ex.Message));
        }
        catch (IOException ex)
        {
            return VaultResult<Unit>.Fail(new(VaultErrorCode.IoError, "I/O error while saving the vault.", ex.Message));
        }
        catch (CryptographicException ex)
        {
            return VaultResult<Unit>.Fail(new(VaultErrorCode.Unknown, "Cryptography error while saving the vault.", ex.Message));
        }
        catch (Exception ex)
        {
            return VaultResult<Unit>.Fail(new(VaultErrorCode.Unknown, "Unexpected error while saving the vault.", ex.Message));
        }
    }

    public readonly struct Unit
    {
        public static readonly Unit Value = new();
    }
}
