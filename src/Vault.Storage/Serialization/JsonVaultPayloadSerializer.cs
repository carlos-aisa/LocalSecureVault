using System.Text.Json;
using Vault.Application.Abstractions;
using Vault.Application.Models;

namespace Vault.Storage.Serialization;

public sealed class JsonVaultPayloadSerializer : IVaultPayloadSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public byte[] SerializeToUtf8(VaultDocument document)
    {
        var dto = VaultDocumentMapper.ToDto(document);
        return JsonSerializer.SerializeToUtf8Bytes(dto, Options);
    }

    public VaultDocument DeserializeFromUtf8(byte[] utf8Payload)
    {
        var dto = JsonSerializer.Deserialize<VaultDocumentDto>(utf8Payload, Options)
                  ?? throw new InvalidOperationException("Invalid vault JSON.");

        return VaultDocumentMapper.RehydrateDocument(dto);
    }
}
