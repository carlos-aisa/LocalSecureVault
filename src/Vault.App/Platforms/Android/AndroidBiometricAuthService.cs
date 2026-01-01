using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace Vault.App.Services;

public class AndroidBiometricAuthService : IBiometricAuthService
{
    private const string KeyPrefix = "vault_password_";

    public async Task<bool> IsBiometricAvailableAsync()
    {
        try
        {
            var availability = await CrossFingerprint.Current.GetAvailabilityAsync();
            return availability == FingerprintAvailability.Available;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AuthenticateAsync(string reason)
    {
        try
        {
            var request = new AuthenticationRequestConfiguration("Biometric Authentication", reason)
            {
                CancelTitle = "Cancel",
                FallbackTitle = "Use password",
                AllowAlternativeAuthentication = false
            };

            var result = await CrossFingerprint.Current.AuthenticateAsync(request);
            return result.Authenticated;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Biometric auth error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> StorePasswordAsync(string vaultPath, string password)
    {
        try
        {
            var key = GetStorageKey(vaultPath);
            await SecureStorage.SetAsync(key, password);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> GetPasswordAsync(string vaultPath)
    {
        try
        {
            var key = GetStorageKey(vaultPath);
            return await SecureStorage.GetAsync(key);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> HasStoredPasswordAsync(string vaultPath)
    {
        var password = await GetPasswordAsync(vaultPath);
        return !string.IsNullOrEmpty(password);
    }

    public Task ClearStoredPasswordAsync(string vaultPath)
    {
        try
        {
            var key = GetStorageKey(vaultPath);
            SecureStorage.Remove(key);
            return Task.CompletedTask;
        }
        catch
        {
            return Task.CompletedTask;
        }
    }

    private static string GetStorageKey(string vaultPath)
    {
        // Use a hash of the path to create a consistent key
        var pathHash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(vaultPath)));
        return KeyPrefix + pathHash;
    }
}
