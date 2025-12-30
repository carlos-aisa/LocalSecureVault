#if WINDOWS
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace Vault.App.Platform;

public sealed class WindowsFileDialogService : IFileDialogService
{
    public async Task<string?> PickSavePathAsync(string suggestedFileName)
    {
        var savePicker = new FileSavePicker();
        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add("Vault File", new List<string>() { ".vlt" });
        savePicker.SuggestedFileName = suggestedFileName;

        var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault<Window>();
        if (window != null && window.Handler.PlatformView is Microsoft.UI.Xaml.Window winuiWindow)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(winuiWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
        }

        StorageFile file = await savePicker.PickSaveFileAsync();
        return file?.Path;
    }
}
#endif
