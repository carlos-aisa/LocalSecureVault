using Vault.Application.Abstractions;
using Vault.Application.Models;
using System.Security.Cryptography;


namespace Vault.Application.Services;

public sealed class VaultAppService
{
    private readonly IVaultStore _store;
    private readonly IVaultCryptoService _crypto;

    public VaultAppService(IVaultStore store, IVaultCryptoService crypto)
    {
        _store = store;
        _crypto = crypto;
    }

    public async Task<VaultResult<UnlockedVault>> OpenAsync(string path, ReadOnlyMemory<char> password)
    {
        if (string.IsNullOrWhiteSpace(path))
            return VaultResult<UnlockedVault>.Fail(new(VaultErrorCode.InvalidFormat, "Select a vault file."));

        try
        {
            var file = await _store.ReadAsync(path);

            try
            {
                var res = _crypto.UnlockVault(file, password.Span);

                return VaultResult<UnlockedVault>.Ok(
                    new UnlockedVault(path, res.Document, res.SessionKey, file.Header));
            }
            catch (InvalidOperationException ex) // magic incorrecto / formato
            {
                return VaultResult<UnlockedVault>.Fail(new(
                    VaultErrorCode.InvalidFormat,
                    "This file is not a valid vault.",
                    ex.Message));
            }
            catch (CryptographicException ex) // AES-GCM tag mismatch: password incorrecta o tamper
            {
                return VaultResult<UnlockedVault>.Fail(new(
                    VaultErrorCode.UnsupportedOrCorrupted,
                    "Unable to decrypt the vault. Check the password or the vault file integrity.",
                    ex.Message));
            }
        }
        catch (FileNotFoundException ex)
        {
            return VaultResult<UnlockedVault>.Fail(new(VaultErrorCode.FileNotFound, "Vault file not found.", ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return VaultResult<UnlockedVault>.Fail(new(VaultErrorCode.AccessDenied, "Access denied to the vault file.", ex.Message));
        }
        catch (IOException ex)
        {
            return VaultResult<UnlockedVault>.Fail(new(VaultErrorCode.IoError, "I/O error while reading the vault file.", ex.Message));
        }
        catch (Exception ex)
        {
            return VaultResult<UnlockedVault>.Fail(new(VaultErrorCode.Unknown, "Unexpected error while opening the vault.", ex.Message));
        }
    }
}