using Vault.Application.Models;
using Vault.Domain;
using Vault.Application.Abstractions;
using Vault.Application.Services;

namespace Vault.Application.UseCases;

public sealed class EntryUseCases
{
    public VaultResult<Guid> Add(
        VaultDocument doc,
        VaultEntry entry,
        DateTimeOffset? nowUtc = null)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(entry);

        if (ExistsDuplicate(doc, entry.Name, entry.Username))
            return VaultResult<Guid>.Fail(new(
                VaultErrorCode.InvalidFormat,
                "An entry with the same name and username already exists."));

        doc.AddEntry(entry, nowUtc);
        return VaultResult<Guid>.Ok(entry.Id);
    }

    public VaultResult<Unit> Update(
        VaultDocument doc,
        Guid id,
        string name,
        string password,
        string? username,
        string? url,
        string? notes,
        IReadOnlyList<string> tags,
        DateTimeOffset? nowUtc = null)
    {
        VaultEntry existing;
        try { existing = doc.GetEntry(id); }
        catch (KeyNotFoundException)
        {
            return VaultResult<Unit>.Fail(new(VaultErrorCode.InvalidFormat, "Entry not found."));
        }

        // validar duplicados si cambia (name, username)
        if (!SameKey(existing, name, username) && ExistsDuplicate(doc, name, username))
            return VaultResult<Unit>.Fail(new(VaultErrorCode.InvalidFormat, "Another entry with the same name and username already exists."));

        existing.Update(
            name: name,
            password: password,
            username: username,
            url: url,
            notes: notes,
            tags: tags,
            nowUtc: nowUtc);

        doc.Touch(nowUtc);
        return VaultResult<Unit>.Ok(Unit.Value);
    }

    private static bool SameKey(VaultEntry e, string name, string? username)
        => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase)
        && string.Equals(e.Username ?? "", username ?? "", StringComparison.OrdinalIgnoreCase);


    public VaultResult<Unit> Delete(
        VaultDocument doc,
        Guid id,
        DateTimeOffset? nowUtc = null)
    {
        ArgumentNullException.ThrowIfNull(doc);

        var removed = doc.RemoveEntry(id, nowUtc);
        if (!removed)
        {
            return VaultResult<Unit>.Fail(new(
                VaultErrorCode.InvalidFormat,
                "Entry not found."));
        }

        return VaultResult<Unit>.Ok(Unit.Value);
    }

    // ---------------- helpers ----------------

    private static bool ExistsDuplicate(
        VaultDocument doc,
        string name,
        string? username)
        => doc.Entries.Any(e =>
            string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(e.Username ?? "", username ?? "", StringComparison.OrdinalIgnoreCase));
}
