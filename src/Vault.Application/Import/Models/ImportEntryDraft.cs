using Vault.Domain;

namespace Vault.Application.Import.Models;

public sealed record ImportEntryDraft(
    string Name,
    string? Identifier,     // maps to Username
    string? Password,
    string? Notes,
    IReadOnlyList<string> Tags
)
{
    public bool IsIncomplete => string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Password);

    // Dedupe key: Name + Identifier (and optionally URL in future)
    public string DedupeKey =>
        $"{Name.Trim().ToLowerInvariant()}|{(Identifier ?? "").Trim().ToLowerInvariant()}";
}
