using Microsoft.Maui.Storage;

namespace Vault.App.Services;

public sealed class PreferencesRecentVaultPathStore : IRecentVaultPathStore
{
    private const string Key = "last_vault_path";

    public string? GetLastPath()
        => Preferences.Default.Get(Key, (string?)null);

    public void SetLastPath(string path)
        => Preferences.Default.Set(Key, path);

    public void Clear()
        => Preferences.Default.Remove(Key);
}
