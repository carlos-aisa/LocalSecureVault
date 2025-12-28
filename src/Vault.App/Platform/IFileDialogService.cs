namespace Vault.App.Platform;

public interface IFileDialogService
{
    Task<string?> PickSavePathAsync(string suggestedFileName);
    Task<string?> PickOpenPathAsync(string fileExtensionFilter); // lo usaremos luego para Unlock
}
