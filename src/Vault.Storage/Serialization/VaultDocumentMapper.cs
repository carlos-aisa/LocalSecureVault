using Vault.Application.Models;
using Vault.Domain;

namespace Vault.Storage.Serialization;

internal static class VaultDocumentMapper
{
    public static VaultDocumentDto ToDto(VaultDocument doc)
    {
        ArgumentNullException.ThrowIfNull(doc);

        return new VaultDocumentDto
        {
            Meta = new VaultMetadataDto
            {
                VaultName = doc.Meta.VaultName,
                SchemaVersion = doc.Meta.SchemaVersion,
                CreatedUtc = doc.Meta.CreatedUtc,
                UpdatedUtc = doc.Meta.UpdatedUtc
            },
            Entries = doc.Entries.Select(ToDto).ToList()
        };
    }

    private static VaultEntryDto ToDto(VaultEntry e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Username = e.Username,
        Password = e.Password,
        Url = e.Url,
        Notes = e.Notes,
        Tags = e.Tags.ToList(),
        CreatedUtc = e.CreatedUtc,
        UpdatedUtc = e.UpdatedUtc
    };

    public static VaultDocument RehydrateDocument(VaultDocumentDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(dto.Meta);
        ArgumentNullException.ThrowIfNull(dto.Entries);

        var meta = VaultMetadata.CreateNew(
            vaultName: dto.Meta.VaultName,
            schemaVersion: dto.Meta.SchemaVersion,
            createdUtc: dto.Meta.CreatedUtc,
            updatedUtc: dto.Meta.UpdatedUtc);

        var entries = dto.Entries.Select(RehydrateEntry).ToList();

        return VaultDocument.Rehydrate(meta, entries);
    }


    // Rehydrate entry with fixed ID/timestamps.
    private static VaultEntry RehydrateEntry(VaultEntryDto e)
    {
        return VaultEntry.Rehydrate(
            id: e.Id,
            name: e.Name,
            password: e.Password,
            username: e.Username,
            url: e.Url,
            notes: e.Notes,
            tags: e.Tags,
            createdUtc: e.CreatedUtc,
            updatedUtc: e.UpdatedUtc);
    }
}
