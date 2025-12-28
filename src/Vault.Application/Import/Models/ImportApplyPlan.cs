namespace Vault.Application.Import.Models;

public sealed class ImportApplyPlan
{
    public IReadOnlyList<ImportAddAction> AddActions { get; init; } = [];
    public IReadOnlyList<ImportEntryDraft> SkippedDuplicates { get; init; } = [];
}
