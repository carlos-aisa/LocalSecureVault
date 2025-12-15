using System.Text;
using System.Text.Json;
using Vault.Application.Models;

namespace Vault.Storage.Serialization;

public sealed class VaultDocumentSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public byte[] SerializeToUtf8(VaultDocument doc)
    {
        ArgumentNullException.ThrowIfNull(doc);

        var dto = VaultDocumentMapper.ToDto(doc);
        return JsonSerializer.SerializeToUtf8Bytes(dto, Options);
    }

    public VaultDocument DeserializeFromUtf8(byte[] utf8Json)
    {
        ArgumentNullException.ThrowIfNull(utf8Json);

        var dto = JsonSerializer.Deserialize<VaultDocumentDto>(utf8Json, Options)
                  ?? throw new InvalidOperationException("Invalid vault JSON.");

        return VaultDocumentMapper.RehydrateDocument(dto);
    }

    public string SerializeToString(VaultDocument doc) =>
        Encoding.UTF8.GetString(SerializeToUtf8(doc));
}
