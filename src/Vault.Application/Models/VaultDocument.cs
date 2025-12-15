using Vault.Domain;

namespace Vault.Application.Models;

public sealed class VaultDocument
{
    public VaultMetadata Meta { get; private set; }
    public IReadOnlyList<VaultEntry> Entries => _entries;

    private readonly List<VaultEntry> _entries;

    private VaultDocument(VaultMetadata meta, List<VaultEntry> entries)
    {
        Meta = meta ?? throw new ArgumentNullException(nameof(meta));
        _entries = entries ?? throw new ArgumentNullException(nameof(entries));
    }

    public static VaultDocument CreateNew(string vaultName, DateTimeOffset? nowUtc = null)
    {
        var now = nowUtc ?? DateTimeOffset.UtcNow;

        var meta = VaultMetadata.CreateNew(
            vaultName: vaultName,
            schemaVersion: 1,
            createdUtc: now,
            updatedUtc: now);

        return new VaultDocument(meta, new List<VaultEntry>());
    }

    internal static VaultDocument Rehydrate(VaultMetadata meta, IEnumerable<VaultEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(meta);
        ArgumentNullException.ThrowIfNull(entries);
        return new VaultDocument(meta, entries.ToList());
    }


    public void Touch(DateTimeOffset? nowUtc = null)
    {
        var now = nowUtc ?? DateTimeOffset.UtcNow;
        Meta = Meta with { UpdatedUtc = now };
    }

    public void AddEntry(VaultEntry entry, DateTimeOffset? nowUtc = null)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _entries.Add(entry);
        Touch(nowUtc);
    }

    public bool RemoveEntry(Guid id, DateTimeOffset? nowUtc = null)
    {
        var idx = _entries.FindIndex(e => e.Id == id);
        if (idx < 0) return false;

        _entries.RemoveAt(idx);
        Touch(nowUtc);
        return true;
    }

    public VaultEntry GetEntry(Guid id) =>
        _entries.SingleOrDefault(e => e.Id == id)
        ?? throw new KeyNotFoundException($"Entry not found: {id}");
}
