namespace Vault.Storage.Serialization;

internal sealed class VaultDocumentDto
{
    public VaultMetadataDto Meta { get; set; } = new();
    public List<VaultEntryDto> Entries { get; set; } = new();
}

internal sealed class VaultMetadataDto
{
    public string VaultName { get; set; } = "";
    public int SchemaVersion { get; set; } = 1;
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}

internal sealed class VaultEntryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Username { get; set; }
    public string Password { get; set; } = "";
    public string? Url { get; set; }
    public string? Notes { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}
