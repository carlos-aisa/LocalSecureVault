using Vault.Application.Abstractions;
using Vault.Application.Models;
using System.Security.Cryptography;


namespace Vault.Application.Services;

public sealed class VaultAppService
{
    private readonly IVaultStore _store;
    private readonly IVaultCryptoService _crypto;
    private readonly VaultSaveService _saver;

    public VaultAppService(IVaultStore store, IVaultCryptoService crypto, VaultSaveService saver)
    {
        _store = store;
        _crypto = crypto;
        _saver = saver;
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

    public Task<VaultResult<Unit>> SaveAsync(
        string path,
        VaultDocument document,
        byte[] sessionKey,
        VaultFileHeader header,
        CancellationToken ct = default)
        => _saver.TrySaveAsync(path, document, sessionKey, header, ct);

    public VaultResult<CreatedVault> CreateInMemory(
        string vaultName,
        ReadOnlyMemory<char> masterPassword,
        KdfProfile profile)
    {
        if (string.IsNullOrWhiteSpace(vaultName))
            return VaultResult<CreatedVault>.Fail(new(VaultErrorCode.InvalidFormat, "Vault name is required."));

        if (masterPassword.Length == 0)
            return VaultResult<CreatedVault>.Fail(new(VaultErrorCode.InvalidFormat, "Master password is required."));

        try
        {
            var doc = VaultDocument.CreateNew(vaultName);
            var created = _crypto.CreateVault(doc, masterPassword.Span, profile);

            return VaultResult<CreatedVault>.Ok(new CreatedVault(
                Document: doc,
                File: created.File,
                SessionKey: created.SessionKey));
        }
        catch (Exception)
        {
            return VaultResult<CreatedVault>.Fail(new(
                VaultErrorCode.Unknown,
                "Unexpected error while creating the vault."));
        }
    }

    public async Task<VaultResult<Unit>> WriteNewVaultAsync(
        string path,
        VaultFile file,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return VaultResult<Unit>.Fail(new(VaultErrorCode.InvalidPath, "Select a save location."));

        try
        {
            await _store.WriteAtomicAsync(path, file, ct);
            return VaultResult<Unit>.Ok(Unit.Value);
        }
        catch (UnauthorizedAccessException)
        {
            return VaultResult<Unit>.Fail(new(VaultErrorCode.AccessDenied, "Access denied while saving the vault."));
        }
        catch (IOException)
        {
            return VaultResult<Unit>.Fail(new(VaultErrorCode.IoError, "I/O error while saving the vault."));
        }
        catch (Exception)
        {
            return VaultResult<Unit>.Fail(new(VaultErrorCode.Unknown, "Unexpected error while saving the vault."));
        }
    }
}

// CreateInMemory Result. (UI use it for State.SetUnlocked after save)
public sealed record CreatedVault(
    VaultDocument Document,
    VaultFile File,
    byte[] SessionKey
);
