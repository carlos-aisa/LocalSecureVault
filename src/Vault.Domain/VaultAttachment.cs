namespace Vault.Domain;

public sealed class VaultAttachment
{
    public const int MaximumCountPerEntry = 5;
    public const int MaximumSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> SupportedMediaTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "application/pdf"
    };

    private readonly byte[] _content;

    public Guid Id { get; }
    public string FileName { get; }
    public string MediaType { get; }
    public ReadOnlyMemory<byte> Content => _content;
    public DateTimeOffset CreatedUtc { get; }
    public bool IsImage => MediaType.StartsWith("image/", StringComparison.Ordinal);

    private VaultAttachment(Guid id, string fileName, string mediaType, byte[] content, DateTimeOffset createdUtc)
    {
        Id = id;
        FileName = fileName;
        MediaType = mediaType;
        _content = content;
        CreatedUtc = createdUtc;
    }

    public static VaultAttachment CreateNew(string fileName, string mediaType, ReadOnlySpan<byte> content, DateTimeOffset? nowUtc = null)
        => Create(Guid.NewGuid(), fileName, mediaType, content, nowUtc ?? DateTimeOffset.UtcNow);

    public static VaultAttachment Rehydrate(Guid id, string fileName, string mediaType, ReadOnlySpan<byte> content, DateTimeOffset createdUtc)
        => Create(id == Guid.Empty ? throw new ArgumentException("Id cannot be empty.", nameof(id)) : id, fileName, mediaType, content, createdUtc);

    private static VaultAttachment Create(Guid id, string fileName, string mediaType, ReadOnlySpan<byte> content, DateTimeOffset createdUtc)
    {
        var normalizedFileName = RequireNonEmpty(fileName, nameof(fileName));
        var normalizedMediaType = RequireNonEmpty(mediaType, nameof(mediaType)).ToLowerInvariant();
        if (!SupportedMediaTypes.Contains(normalizedMediaType))
            throw new ArgumentException("Unsupported attachment media type.", nameof(mediaType));
        if (content.Length == 0)
            throw new ArgumentException("Attachment content cannot be empty.", nameof(content));
        if (content.Length > MaximumSizeBytes)
            throw new ArgumentException("Attachment exceeds the maximum size of 5 MiB.", nameof(content));

        return new VaultAttachment(id, normalizedFileName, normalizedMediaType, content.ToArray(), createdUtc);
    }

    private static string RequireNonEmpty(string value, string paramName)
    {
        if (value is null) throw new ArgumentNullException(paramName);
        var trimmed = value.Trim();
        return trimmed.Length == 0 ? throw new ArgumentException("Value cannot be empty.", paramName) : trimmed;
    }
}
