
namespace Vault.Application.Import.Models;

public sealed class ImportIssue
{
    public ImportSeverity Severity { get; init; }
    public string Code { get; init; } = "";
    public string Message { get; init; } = "";
    public int? LineNumber { get; init; }
}
