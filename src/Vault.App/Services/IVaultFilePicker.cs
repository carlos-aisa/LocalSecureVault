namespace Vault.App.Services;

public interface IVaultFilePicker
{
    Task<string?> PickVaultFileAsync(CancellationToken ct = default);
}

public interface IVaultExportPicker
{
    Task<string?> PickExportDestinationAsync(string suggestedFileName, CancellationToken ct = default);
}
