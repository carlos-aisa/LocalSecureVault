
using System.ComponentModel.DataAnnotations;
using Vault.Domain;

namespace Vault.App;
public sealed class EntryEditorModel
{
    [Required]
    public string Name { get; set; } = "";

    public string? Username { get; set; }

    [Required]
    public string Password { get; set; } = "";

    public string? Url { get; set; }

    public string? Notes { get; set; }

    // UI helper
    public string TagsText { get; set; } = "";

    public static EntryEditorModel FromEntry(VaultEntry? e)
    {
        if (e is null) return new EntryEditorModel();

        return new EntryEditorModel
        {
            Name = e.Name,
            Username = e.Username,
            Password = e.Password,
            Url = e.Url,
            Notes = e.Notes,
            TagsText = string.Join(Environment.NewLine, e.Tags)
        };
    }

    public VaultEntry ToNewEntry(DateTimeOffset? nowUtc = null)
    {
        var tags = ParseTags();
        return VaultEntry.CreateNew(
            name: Name.Trim(),
            password: Password,
            username: Username?.Trim(),
            url: Url?.Trim(),
            notes: Notes?.Trim(),
            tags: tags,
            nowUtc: nowUtc);
    }

    public EntryUpdateData ToUpdateData()
    {
        return new EntryUpdateData(
            Name.Trim(),
            Password,
            Username?.Trim(),
            Url?.Trim(),
            Notes?.Trim(),
            ParseTags());
    }

    private List<string> ParseTags()
    {
        return TagsText
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public sealed record EntryUpdateData(
        string Name,
        string Password,
        string? Username,
        string? Url,
        string? Notes,
        IReadOnlyList<string> Tags);
}
