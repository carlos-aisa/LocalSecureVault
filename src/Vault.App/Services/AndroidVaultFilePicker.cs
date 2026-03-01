#if ANDROID
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Provider;
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
            intent.AddFlags(
                ActivityFlags.GrantPersistableUriPermission |
                ActivityFlags.GrantReadUriPermission |
                ActivityFlags.GrantWriteUriPermission);
            
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
            var takeFlags = resultIntent.Flags & (ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
            if (takeFlags == 0)
                takeFlags = ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission;
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

    public string? TryGetDisplayName(string uriString)
    {
        if (string.IsNullOrWhiteSpace(uriString))
            return null;

        try
        {
            var context = Android.App.Application.Context;
            var resolver = context.ContentResolver;
            if (resolver == null)
                return null;

            var uri = Android.Net.Uri.Parse(uriString);
            if (uri == null)
                return null;

            var projection = new[] { IOpenableColumns.DisplayName };
            using var cursor = resolver.Query(uri, projection, null, null, null);
            if (cursor == null || !cursor.MoveToFirst())
                return null;

            var nameIndex = cursor.GetColumnIndex(IOpenableColumns.DisplayName);
            if (nameIndex < 0)
                return null;

            return cursor.GetString(nameIndex);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Android implementation for selecting an export destination using SAF.
/// Returns a content:// URI that can be written by IVaultStore.
/// </summary>
public sealed class AndroidVaultExportPicker : IVaultExportPicker
{
    private const int CreateFileRequestCode = 1002;
    private static TaskCompletionSource<Intent?>? _createFileTaskCompletionSource;

    public async Task<string?> PickExportDestinationAsync(string suggestedFileName, CancellationToken ct = default)
    {
        try
        {
            var safeName = NormalizeFileName(suggestedFileName);

            var intent = new Intent(Intent.ActionCreateDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("application/octet-stream");
            intent.PutExtra(Intent.ExtraTitle, safeName);
            intent.AddFlags(
                ActivityFlags.GrantPersistableUriPermission |
                ActivityFlags.GrantReadUriPermission |
                ActivityFlags.GrantWriteUriPermission);

            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (activity == null)
                return null;

            _createFileTaskCompletionSource = new TaskCompletionSource<Intent?>();
            activity.StartActivityForResult(intent, CreateFileRequestCode);

            var resultIntent = await _createFileTaskCompletionSource.Task;
            if (resultIntent?.Data == null)
                return null;

            var uri = resultIntent.Data;
            var uriString = uri.ToString();

            var takeFlags = resultIntent.Flags & (ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
            if (takeFlags == 0)
                takeFlags = ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission;

            try
            {
                activity.ContentResolver?.TakePersistableUriPermission(uri, takeFlags);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not persist export URI permission: {ex.Message}");
            }

            return uriString;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking export destination: {ex.Message}");
            return null;
        }
    }

    internal static void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        if (requestCode == CreateFileRequestCode)
        {
            if (resultCode == Result.Ok && data != null)
            {
                _createFileTaskCompletionSource?.TrySetResult(data);
            }
            else
            {
                _createFileTaskCompletionSource?.TrySetResult(null);
            }
        }
    }

    private static string NormalizeFileName(string? fileName)
    {
        var fallback = "vault-export.vlt";

        if (string.IsNullOrWhiteSpace(fileName))
            return fallback;

        var trimmed = fileName.Trim();
        if (!trimmed.EndsWith(".vlt", StringComparison.OrdinalIgnoreCase))
            trimmed += ".vlt";

        foreach (var c in Path.GetInvalidFileNameChars())
            trimmed = trimmed.Replace(c, '_');

        return string.IsNullOrWhiteSpace(trimmed) ? fallback : trimmed;
    }
}
#endif
