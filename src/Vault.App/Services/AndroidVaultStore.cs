#if ANDROID
using Vault.Application.Abstractions;
using Vault.Storage;

namespace Vault.App.Services;

public sealed class AndroidVaultStore : IVaultStore
{
    private readonly FileVaultStore _fileStore = new();

    public Task<VaultFile> ReadAsync(string path, CancellationToken ct = default)
    {
        if (!IsContentUri(path)) return _fileStore.ReadAsync(path, ct);
        return ReadContentUriAsync(path, ct);
    }

    public Task WriteAtomicAsync(string path, VaultFile file, CancellationToken ct = default)
    {
        if (!IsContentUri(path)) return _fileStore.WriteAtomicAsync(path, file, ct);
        return WriteContentUriAsync(path, file, ct);
    }

    private static async Task<VaultFile> ReadContentUriAsync(string path, CancellationToken ct)
    {
        var resolver = Android.App.Application.Context.ContentResolver
            ?? throw new InvalidOperationException("ContentResolver not available.");
        var uri = Android.Net.Uri.Parse(path) ?? throw new ArgumentException("Invalid content URI.", nameof(path));
        using var stream = resolver.OpenInputStream(uri)
            ?? throw new IOException("Could not open the selected vault file.");
        return await FileVaultStore.ReadFromStreamAsync(stream, ct);
    }

    private static async Task WriteContentUriAsync(string path, VaultFile file, CancellationToken ct)
    {
        var resolver = Android.App.Application.Context.ContentResolver
            ?? throw new InvalidOperationException("ContentResolver not available.");
        var uri = Android.Net.Uri.Parse(path) ?? throw new ArgumentException("Invalid content URI.", nameof(path));
        await using var stream = resolver.OpenOutputStream(uri, "rwt") ?? resolver.OpenOutputStream(uri, "wt")
            ?? throw new IOException("Could not open the selected export destination.");
        await FileVaultStore.WriteToStreamAsync(stream, file, ct);
    }

    private static bool IsContentUri(string path) => path.StartsWith("content://", StringComparison.OrdinalIgnoreCase);
}
#endif
