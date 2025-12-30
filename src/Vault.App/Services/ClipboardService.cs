using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace Vault.App.Services;

public sealed class ClipboardService
{
    public Task SetTextAsync(string text)
        => Clipboard.Default.SetTextAsync(text ?? string.Empty);
}
