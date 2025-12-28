using Vault.Application.Models;
using Vault.Application.Abstractions;

namespace Vault.App;

public sealed class AppState
{
    public event Action? Changed;
    public VaultFileHeader? CurrentHeader { get; private set; }
    public string? CurrentVaultPath { get; private set; }
    public VaultDocument? UnlockedDocument { get; private set; }
    public bool IsDirty { get; private set; }

    // Session Key (in RAM). Do not persist.
    public byte[]? SessionKey { get; private set; }

    public bool IsUnlocked => UnlockedDocument is not null && SessionKey is not null;

    public void SetUnlocked(string path, VaultDocument doc, byte[] sessionKey, VaultFileHeader header)
    {
        CurrentVaultPath = path;
        UnlockedDocument = doc;
        SessionKey = sessionKey;
        CurrentHeader = header;
        IsDirty = false;
        Changed?.Invoke();
    }

    public void MarkDirty() => IsDirty = true;

    public void MarkSaved() => IsDirty = false;

    public void Lock()
    {
        CurrentVaultPath = null;
        UnlockedDocument = null;
        CurrentHeader = null;
        IsDirty = false;

        if (SessionKey is not null)
        {
            Array.Clear(SessionKey, 0, SessionKey.Length);
            SessionKey = null;
        }

        Changed?.Invoke();
    }
}

