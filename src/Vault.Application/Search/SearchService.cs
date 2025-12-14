using Vault.Application.Models;
using Vault.Domain;

namespace Vault.Application.Search;

public sealed class SearchService
{
    public IReadOnlyList<VaultEntry> Search(VaultDocument document, SearchQuery query)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(query);

        if (query.IsEmpty)
            return document.Entries;

        var q = query.Normalized;
        var cmp = StringComparison.OrdinalIgnoreCase;

        return document.Entries
            .Where(e =>
                e.Name.Contains(q, cmp) ||
                (e.Username?.Contains(q, cmp) ?? false) ||
                (e.Url?.Contains(q, cmp) ?? false) ||
                (e.Notes?.Contains(q, cmp) ?? false) ||
                e.Tags.Any(t => t.Contains(q, cmp)))
            .ToList();
    }
}
