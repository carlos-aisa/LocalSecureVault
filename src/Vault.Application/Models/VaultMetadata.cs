namespace Vault.Application.Models;

public sealed record VaultMetadata(
    string VaultName,
    int SchemaVersion,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc)
{
    public static VaultMetadata CreateNew(
        string vaultName,
        int schemaVersion,
        DateTimeOffset createdUtc,
        DateTimeOffset updatedUtc)
    {
        if (vaultName is null) throw new ArgumentNullException(nameof(vaultName));

        var name = vaultName.Trim();
        if (name.Length == 0) throw new ArgumentException("Vault name cannot be empty.", nameof(vaultName));

        if (schemaVersion <= 0) throw new ArgumentOutOfRangeException(nameof(schemaVersion), "Schema version must be > 0.");

        return new VaultMetadata(name, schemaVersion, createdUtc, updatedUtc);
    }
}
