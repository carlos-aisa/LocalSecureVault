using Vault.Domain;

namespace Vault.Application.Models;

public sealed class VaultState
{
    private readonly List<VaultEntry> _entries = new();

    public IReadOnlyList<VaultEntry> Entries => _entries;

    public void Add(VaultEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _entries.Add(entry);
    }

    public VaultEntry GetById(Guid id)
    {
        var entry = _entries.SingleOrDefault(e => e.Id == id);
        if (entry is null) throw new KeyNotFoundException($"Entry not found: {id}");
        return entry;
    }

    public bool Remove(Guid id)
    {
        var idx = _entries.FindIndex(e => e.Id == id);
        if (idx < 0) return false;
        _entries.RemoveAt(idx);
        return true;
    }
}
