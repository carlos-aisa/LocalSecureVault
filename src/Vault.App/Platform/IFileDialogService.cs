namespace Vault.App.Platform;

public interface IFileDialogService
{
    Task<string?> PickSavePathAsync(string suggestedFileName);
}
