namespace Vault.App.Services;

public interface IBiometricAuthService
{
    /// <summary>
    /// Checks if biometric authentication is available on this device.
    /// </summary>
    Task<bool> IsBiometricAvailableAsync();

    /// <summary>
    /// Authenticates the user using biometric authentication.
    /// </summary>
    /// <param name="reason">Message to display to the user</param>
    /// <returns>True if authentication was successful, false otherwise</returns>
    Task<bool> AuthenticateAsync(string reason);

    /// <summary>
    /// Stores the vault password securely with biometric protection.
    /// </summary>
    Task<bool> StorePasswordAsync(string vaultPath, string password);

    /// <summary>
    /// Retrieves the stored vault password after biometric authentication.
    /// </summary>
    Task<string?> GetPasswordAsync(string vaultPath);

    /// <summary>
    /// Checks if a password is stored for the given vault path.
    /// </summary>
    Task<bool> HasStoredPasswordAsync(string vaultPath);

    /// <summary>
    /// Clears the stored password for the given vault path.
    /// </summary>
    Task ClearStoredPasswordAsync(string vaultPath);
}
