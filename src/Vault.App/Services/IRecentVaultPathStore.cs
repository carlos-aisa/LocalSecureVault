namespace Vault.App.Services;

public interface IRecentVaultPathStore
{
    string? GetLastPath();
    void SetLastPath(string path);
    void Clear();
}
