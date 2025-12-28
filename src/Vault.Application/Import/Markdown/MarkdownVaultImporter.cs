
using System.Text.RegularExpressions;
using System.Xml.Schema;
using Vault.Application.Import.Models;
using Vault.Application.Models;

namespace Vault.Application.Import.Markdown;

public sealed class MarkdownVaultImporter
{
    private static readonly Regex SeparatorRowRegex = new(@"^\s*\|?\s*[-:]+\s*(\|\s*[-:]+\s*)+\|?\s*$",
        RegexOptions.Compiled);

    public ImportResult Parse(string markdown)
    {
        if (markdown is null) throw new ArgumentNullException(nameof(markdown));

        var lines = MarkdownVaultImporterHelpers.SplitLines(markdown);
        var warnings = new List<ImportIssue>();
        var entries = new List<ImportEntryDraft>();

        // Current tags from headings
        var h1 = (string?)null;
        var h2 = (string?)null;

        int i = 0;
        while (i < lines.Length)
        {
            var line = lines[i];

            // headings
            if (MarkdownVaultImporterHelpers.TryParseHeading(line, out var level, out var text))
            {
                if (level == 1) { h1 = text; h2 = null; }
                else if (level == 2) { h2 = text; }
                i++;
                continue;
            }

            // table start? (header row with pipes, next row is separator)
            if (MarkdownVaultImporterHelpers.LooksLikeTableHeader(line) && i + 1 < lines.Length && SeparatorRowRegex.IsMatch(lines[i + 1]))
            {
                var headerLineNo = i + 1; // 1-based
                var headerCols = MarkdownVaultImporterHelpers.ParseTableRow(line);
                var colMap = MarkdownVaultImporterHelpers.BuildColumnMap(headerCols);

                if (!colMap.HasName || !colMap.HasPassword)
                {
                    warnings.Add(new ImportIssue(){Severity = ImportSeverity.Warning, 
                                                   LineNumber = headerLineNo,
                                                   Message =$"Tabla ignorada: no se encontraron columnas obligatorias 'Nombre' y/o 'Contraseña'. Columnas: {string.Join(", ", headerCols)}"});
                    // skip table block
                    i += 2;
                    while (i < lines.Length && MarkdownVaultImporterHelpers.LooksLikeTableRow(lines[i])) i++;
                    continue;
                }

                // consume separator
                i += 2;

                // parse rows until not a table row
                while (i < lines.Length && MarkdownVaultImporterHelpers.LooksLikeTableRow(lines[i]))
                {
                    var rowLineNo = i + 1;
                    var row = MarkdownVaultImporterHelpers.ParseTableRow(lines[i]);

                    // Normalize row length: allow missing trailing cells
                    var name = MarkdownVaultImporterHelpers.GetCell(row, colMap.NameIndex);
                    var identifier = colMap.IdentifierIndex is null ? null : MarkdownVaultImporterHelpers.GetCell(row, colMap.IdentifierIndex.Value);
                    var password = MarkdownVaultImporterHelpers.GetCell(row, colMap.PasswordIndex);
                    var notes = colMap.NotesIndex is null ? null : MarkdownVaultImporterHelpers.GetCell(row, colMap.NotesIndex.Value);

                    var tags = MarkdownVaultImporterHelpers.BuildTags(h1, h2);

                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
                    {
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            warnings.Add(new ImportIssue(){ Severity = ImportSeverity.Warning, 
                                                            LineNumber = rowLineNo, 
                                                            Message = "Fila con 'Nombre' vacío. Se importará como incompleta."});
                        }

                        // Allow password empty but warn
                        if (string.IsNullOrWhiteSpace(password))
                        {
                            warnings.Add(new ImportIssue(){ Severity = ImportSeverity.Warning, 
                                                            LineNumber = rowLineNo, 
                                                            Message = $"Fila '{name}': 'Contraseña' vacía. Se importará como incompleta."});
                        }
                        i++;
                        continue;
                    }

                    entries.Add(new ImportEntryDraft(
                        Name: name ?? "",
                        Identifier: string.IsNullOrWhiteSpace(identifier) ? null : identifier,
                        Password: string.IsNullOrWhiteSpace(password) ? null : password,
                        Notes: string.IsNullOrWhiteSpace(notes) ? null : notes,
                        Tags: tags
                    ));

                    i++;
                }

                continue;
            }

            i++;
        }

        return new ImportResult(entries, warnings);
    }

    public ApplyResult Apply(
        VaultDocument vault,
        ImportApplyPlan plan,
        DateTimeOffset? nowUtc = null)
    {
        ArgumentNullException.ThrowIfNull(vault);
        ArgumentNullException.ThrowIfNull(plan);

        foreach (var action in plan.AddActions)
        {
            vault.AddEntry(action.Entry, nowUtc);
        }

        return new ApplyResult()
        {
            Added = plan.AddActions.Count,
            Skipped = plan.SkippedDuplicates.Count
        };
    }
}
