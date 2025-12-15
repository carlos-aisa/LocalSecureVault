namespace Vault.Application.Abstractions;

public enum KdfProfile
{
    Interactive,   // fast unlock (dev/testing)
    Strong         // ~0.5–1.0s target
}
