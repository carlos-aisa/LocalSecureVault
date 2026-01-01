namespace Vault.App.Services;

/// <summary>
/// Windows implementation of biometric authentication - currently not supported.
/// </summary>
public class WindowsBiometricAuthService : IBiometricAuthService
{
    public Task<bool> IsBiometricAvailableAsync()
    {
        // Windows Hello could be implemented here in the future
        return Task.FromResult(false);
    }

    public Task<bool> AuthenticateAsync(string reason)
    {
        return Task.FromResult(false);
    }

    public Task<bool> StorePasswordAsync(string vaultPath, string password)
    {
        return Task.FromResult(false);
    }

    public Task<string?> GetPasswordAsync(string vaultPath)
    {
        return Task.FromResult<string?>(null);
    }

    public Task<bool> HasStoredPasswordAsync(string vaultPath)
    {
        return Task.FromResult(false);
    }

    public Task ClearStoredPasswordAsync(string vaultPath)
    {
        return Task.CompletedTask;
    }
}
