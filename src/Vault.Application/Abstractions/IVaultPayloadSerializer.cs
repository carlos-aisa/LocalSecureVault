using Vault.Application.Models;

namespace Vault.Application.Abstractions;

public interface IVaultPayloadSerializer
{
    byte[] SerializeToUtf8(VaultDocument document);
    VaultDocument DeserializeFromUtf8(byte[] utf8Payload);
}
