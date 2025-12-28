namespace Vault.Application.Import.Models;

public sealed record ImportResult(
    IReadOnlyList<ImportEntryDraft> Entries,
    IReadOnlyList<ImportIssue> Issues
)
{
    public int TotalEntries => Entries.Count;
    public int IncompleteEntries => Entries.Count(e => e.IsIncomplete);

    public IReadOnlyDictionary<string, int> CountByTagPath()
    {
        // "Category" or "Category/Subcategory"
        var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in Entries)
        {
            var key = string.Join("/", e.Tags);
            if (!dict.TryAdd(key, 1)) dict[key]++;
        }
        return dict;
    }
}
