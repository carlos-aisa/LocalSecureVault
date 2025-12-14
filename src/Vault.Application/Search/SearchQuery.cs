namespace Vault.Application.Search;

public sealed record SearchQuery(string Text)
{
    public string Normalized => (Text ?? string.Empty).Trim();

    public bool IsEmpty => string.IsNullOrWhiteSpace(Normalized);
}
