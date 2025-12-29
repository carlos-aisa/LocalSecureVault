using Vault.Application.Import.Markdown;
using Vault.Application.Import.Models;
using Vault.Application.Models;
using Vault.Application.Abstractions;

namespace Vault.Application.Services;

public class VaultImportService
{
    private readonly MarkdownVaultImporter _importer;

    public VaultImportService(MarkdownVaultImporter importer)
    {
        _importer = importer;
    }

    public VaultResult<ImportResult> Preview(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return VaultResult<ImportResult>.Fail(
                new(VaultErrorCode.InvalidFormat, "Markdown content is empty."));

        try
        {
            var result = _importer.Parse(markdown);
            return VaultResult<ImportResult>.Ok(result);
        }
        catch (Exception)
        {
            return VaultResult<ImportResult>.Fail(
                new(VaultErrorCode.Unknown, "Failed to parse markdown."));
        }
    }

    public VaultResult<ApplyResult> Apply(
        VaultDocument document,
        ImportApplyPlan plan,
        DateTimeOffset? nowUtc = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(plan);

        try
        {
            var res = _importer.Apply(document, plan, nowUtc);
            return VaultResult<ApplyResult>.Ok(res);
        }
        catch (Exception)
        {
            return VaultResult<ApplyResult>.Fail(
                new(VaultErrorCode.Unknown, "Failed to apply import plan."));
        }
    }
}
