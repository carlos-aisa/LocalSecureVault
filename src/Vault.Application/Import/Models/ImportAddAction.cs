
using Vault.Domain;

namespace Vault.Application.Import.Models;

public sealed class ImportAddAction
{
    public VaultEntry Entry { get; init; } = default!;
}
