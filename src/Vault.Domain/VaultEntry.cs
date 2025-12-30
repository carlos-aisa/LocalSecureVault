namespace Vault.Domain;

public sealed class VaultEntry
{
    public Guid Id { get; }
    public string Name { get; private set; }
    public string? Username { get; private set; }
    public string Password { get; private set; }
    public string? Url { get; private set; }
    public string? Notes { get; private set; }
    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();
    public DateTimeOffset CreatedUtc { get; }
    public DateTimeOffset UpdatedUtc { get; private set; }

    private readonly List<string> _tags;

    internal VaultEntry(
        Guid id,
        string name,
        string password,
        string? username,
        string? url,
        string? notes,
        IEnumerable<string>? tags,
        DateTimeOffset createdUtc,
        DateTimeOffset updatedUtc)
    {
        Id = id;
        Name = name;
        Password = password;
        Username = username;
        Url = url;
        Notes = notes;
        _tags = NormalizeTags(tags);
        CreatedUtc = createdUtc;
        UpdatedUtc = updatedUtc;
    }

    public static VaultEntry CreateNew(
        string name,
        string password,
        string? username = null,
        string? url = null,
        string? notes = null,
        IEnumerable<string>? tags = null,
        DateTimeOffset? nowUtc = null)
    {
        var now = nowUtc ?? DateTimeOffset.UtcNow;

        return new VaultEntry(
            id: Guid.NewGuid(),
            name: RequireNonEmpty(name, nameof(name)),
            password: RequireNonEmpty(password, nameof(password)),
            username: NormalizeOptional(username),
            url: NormalizeOptional(url),
            notes: NormalizeOptional(notes),
            tags: tags,
            createdUtc: now,
            updatedUtc: now);
    }

    public void Update(
        string name,
        string password,
        string? username = null,
        string? url = null,
        string? notes = null,
        IEnumerable<string>? tags = null,
        DateTimeOffset? nowUtc = null)
    {
        var now = nowUtc ?? DateTimeOffset.UtcNow;

        Name = RequireNonEmpty(name, nameof(name));
        Password = RequireNonEmpty(password, nameof(password));
        Username = NormalizeOptional(username);
        Url = NormalizeOptional(url);
        Notes = NormalizeOptional(notes);

        _tags.Clear();
        _tags.AddRange(NormalizeTags(tags));

        UpdatedUtc = now;
    }

    public static VaultEntry Rehydrate(
        Guid id,
        string name,
        string password,
        string? username,
        string? url,
        string? notes,
        IEnumerable<string>? tags,
        DateTimeOffset createdUtc,
        DateTimeOffset updatedUtc)
    {
        // Reuse the same minimal invariants
        return new VaultEntry(
            id: id == Guid.Empty ? throw new ArgumentException("Id cannot be empty.", nameof(id)) : id,
            name: RequireNonEmpty(name, nameof(name)),
            password: RequireNonEmpty(password, nameof(password)),
            username: NormalizeOptional(username),
            url: NormalizeOptional(url),
            notes: NormalizeOptional(notes),
            tags: tags,
            createdUtc: createdUtc,
            updatedUtc: updatedUtc);
    }

    private static string RequireNonEmpty(string value, string paramName)
    {
        if (value is null) throw new ArgumentNullException(paramName);
        var trimmed = value.Trim();
        if (trimmed.Length == 0) throw new ArgumentException("Value cannot be empty.", paramName);
        return trimmed;
    }

    private static string? NormalizeOptional(string? value)
    {
        if (value is null) return null;
        var trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    private static List<string> NormalizeTags(IEnumerable<string>? tags)
    {
        if (tags is null) return new List<string>();

        // Domain rule (minimal): keep tags non-empty after trimming.
        // Policy rules like lowercase, sorting, deduplication belong to Application.
        return tags
            .Where(t => t is not null)
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .ToList();
    }
}
