namespace Vault.Application.Import.Models;

public sealed class ApplyResult
{
    public int Added { get; init; }
    public int Skipped { get; init; }
}
