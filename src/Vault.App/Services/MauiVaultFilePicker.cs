using Microsoft.Maui.Storage;

namespace Vault.App.Services;

public sealed class MauiVaultFilePicker : IVaultFilePicker
{
    public async Task<string?> PickVaultFileAsync(CancellationToken ct = default)
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select vault file",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                // Windows: extensions
                [DevicePlatform.WinUI] = new[] { ".vlt" },
                // If in the future you support other platforms, add here appropriate types.:
                [DevicePlatform.MacCatalyst] = new[] { "public.data" },
                [DevicePlatform.iOS] = new[] { "public.data" },
                [DevicePlatform.Android] = new[] { "*/*" },
            })
        });

        return result?.FullPath;
    }
}
