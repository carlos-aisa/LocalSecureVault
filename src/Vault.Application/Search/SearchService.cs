using Vault.Application.Models;
using Vault.Domain;

namespace Vault.Application.Search;

public sealed class SearchService
{
    public IReadOnlyList<VaultEntry> Search(VaultState state, SearchQuery query)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(query);

        if (query.IsEmpty) return state.Entries;

        var q = query.Normalized;

        // App policy: case-insensitive contains, across key fields + tags
        return state.Entries
            .Where(e => Matches(e, q))
            .ToList();
    }

    private static bool Matches(VaultEntry e, string q)
    {
        var cmp = StringComparison.OrdinalIgnoreCase;

        if (e.Name.Contains(q, cmp)) return true;
        if ((e.Username?.Contains(q, cmp) ?? false)) return true;
        if ((e.Url?.Contains(q, cmp) ?? false)) return true;
        if ((e.Notes?.Contains(q, cmp) ?? false)) return true;

        // Tags: we decide to search case-insensitive on trimmed tags.
        if (e.Tags.Any(t => t.Contains(q, cmp))) return true;

        return false;
    }
}
