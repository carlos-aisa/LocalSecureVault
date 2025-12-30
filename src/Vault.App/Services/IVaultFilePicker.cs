namespace Vault.App.Services;

public interface IVaultFilePicker
{
    Task<string?> PickVaultFileAsync(CancellationToken ct = default);
}
