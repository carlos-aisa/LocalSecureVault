#if ANDROID
using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace Vault.App.Services;

/// <summary>
/// Android implementation of IVaultFilePicker using Storage Access Framework (SAF).
/// Uses content:// URIs with persisted permissions to access vault files.
/// </summary>
public sealed class AndroidVaultFilePicker : IVaultFilePicker
{
    private const string LastUriKey = "android_last_vault_uri";
    private const int PickFileRequestCode = 1001;
    private static TaskCompletionSource<Intent?>? _pickFileTaskCompletionSource;

    public async Task<string?> PickVaultFileAsync(CancellationToken ct = default)
    {
        try
        {
            var intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("*/*");
            
            // Filter for .vlt files (though Android will show all, user can search)
            intent.PutExtra(Intent.ExtraMimeTypes, new[] { "application/octet-stream", "*/*" });

            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (activity == null)
                return null;

            _pickFileTaskCompletionSource = new TaskCompletionSource<Intent?>();
            
            activity.StartActivityForResult(intent, PickFileRequestCode);
            
            var resultIntent = await _pickFileTaskCompletionSource.Task;
            
            if (resultIntent?.Data == null)
                return null;

            var uri = resultIntent.Data;
            var uriString = uri.ToString();

            // Take persistable URI permission so we can access it later
            var takeFlags = ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission;
            try
            {
                activity.ContentResolver?.TakePersistableUriPermission(uri, takeFlags);
                
                // Save this URI for "Open Last Vault" functionality
                Preferences.Default.Set(LastUriKey, uriString);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not take persistable permission: {ex.Message}");
                // Continue anyway - we can still use it in this session
            }

            return uriString;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking vault file: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Called from MainActivity.OnActivityResult to complete the file picking task.
    /// </summary>
    internal static void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        if (requestCode == PickFileRequestCode)
        {
            if (resultCode == Result.Ok && data != null)
            {
                _pickFileTaskCompletionSource?.TrySetResult(data);
            }
            else
            {
                _pickFileTaskCompletionSource?.TrySetResult(null);
            }
        }
    }

    /// <summary>
    /// Gets the last opened vault URI if it still has valid permissions.
    /// </summary>
    public string? GetLastVaultUri()
    {
        var lastUri = Preferences.Default.Get(LastUriKey, (string?)null);
        
        if (string.IsNullOrEmpty(lastUri))
            return null;

        // Verify we still have permission to access this URI
        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        if (activity?.ContentResolver == null)
            return null;

        try
        {
            var uri = Android.Net.Uri.Parse(lastUri);
            var persistedUris = activity.ContentResolver.PersistedUriPermissions;
            
            var hasPermission = persistedUris?.Any(p => 
                p.Uri?.ToString() == lastUri && 
                p.IsReadPermission && 
                p.IsWritePermission) ?? false;

            return hasPermission ? lastUri : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Clears the saved last vault URI.
    /// </summary>
    public void ClearLastVaultUri()
    {
        Preferences.Default.Remove(LastUriKey);
    }
}
#endif
