using Vault.Application.Models;
using Vault.Domain;

namespace Vault.Application.UseCases;

public sealed class EntryCommands
{
    public VaultEntry AddEntry(
        VaultState state,
        string name,
        string password,
        string? username = null,
        string? url = null,
        string? notes = null,
        IEnumerable<string>? tags = null,
        DateTimeOffset? nowUtc = null)
    {
        ArgumentNullException.ThrowIfNull(state);

        var entry = VaultEntry.CreateNew(
            name: name,
            password: password,
            username: username,
            url: url,
            notes: notes,
            tags: tags,
            nowUtc: nowUtc);

        state.Add(entry);
        return entry;
    }

    public void UpdateEntry(
        VaultState state,
        Guid id,
        string name,
        string password,
        string? username = null,
        string? url = null,
        string? notes = null,
        IEnumerable<string>? tags = null,
        DateTimeOffset? nowUtc = null)
    {
        ArgumentNullException.ThrowIfNull(state);

        var entry = state.GetById(id);

        entry.Update(
            name: name,
            password: password,
            username: username,
            url: url,
            notes: notes,
            tags: tags,
            nowUtc: nowUtc);
    }

    public bool DeleteEntry(VaultState state, Guid id)
    {
        ArgumentNullException.ThrowIfNull(state);
        return state.Remove(id);
    }
}
