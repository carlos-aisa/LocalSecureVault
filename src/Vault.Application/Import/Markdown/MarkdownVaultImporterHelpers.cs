using System.Diagnostics;
using System.Text;
namespace Vault.Application.Import.Markdown;

public static class MarkdownVaultImporterHelpers
{
    public static string[] SplitLines(string text) =>
        text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

    public static bool TryParseHeading(string line, out int level, out string text)
    {
        level = 0;
        text = "";
        if (line is null) return false;

        var trimmed = line.Trim();
        if (trimmed.StartsWith("## "))
        {
            level = 2;
            text = trimmed.Substring(3).Trim();
            return text.Length > 0;
        }
        if (trimmed.StartsWith("# "))
        {
            level = 1;
            text = trimmed.Substring(2).Trim();
            return text.Length > 0;
        }
        return false;
    }

    public static bool LooksLikeTableHeader(string line) =>
        LooksLikeTableRow(line) && 
        line.ToUpper().Contains("NOMBRE", StringComparison.OrdinalIgnoreCase);

    public static bool LooksLikeTableRow(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return false;
        var t = line.Trim();
        return t.Contains('|') && !t.StartsWith("```");
    }

    public static List<string> ParseTableRow(string line)
    {
        var raw = line.Trim();
        var cells = raw.Trim('|').Split('|').Select(c => c.Trim()).ToList();
        return cells;
    }

    public static string? GetCell(List<string> row, int index)
    {
        if (index < 0) return null;
        if (index >= row.Count) return null;
        var v = row[index].Trim();
        return v.Length == 0 ? null : v;
    }

    public static IReadOnlyList<string> BuildTags(string? h1, string? h2)
    {
        var tags = new List<string>();
        if (!string.IsNullOrWhiteSpace(h1)) tags.Add(h1!.Trim());
        if (!string.IsNullOrWhiteSpace(h2)) tags.Add(h2!.Trim());
        return tags;
    }

    public static ColumnMap BuildColumnMap(List<string> headerCols)
    {
        int? name = null, identifier = null, password = null, notes = null;

        for (int i = 0; i < headerCols.Count; i++)
        {
            var key = NormalizeHeader(headerCols[i]);

            if (key is "nombre") name = i;
            else if (key is "usuario" or "identificador") identifier = i;
            else if (key is "contrasena" or "contraseña") password = i;
            else if (key is "infoextra" or "info" or "notas" or "nota") notes = i;
        }

        return new ColumnMap(
            NameIndex: name ?? -1,
            IdentifierIndex: identifier,
            PasswordIndex: password ?? -1,
            NotesIndex: notes
        );
    }

    public static string NormalizeHeader(string s)
    {
        // lower, trim, remove spaces, remove accents
        var t = s.Trim().ToLowerInvariant();
        t = t.Replace(" ", "");
        t = RemoveDiacritics(t);
        return t;
    }

    public static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
public sealed record ColumnMap(int NameIndex, int? IdentifierIndex, int PasswordIndex, int? NotesIndex)
{
    public bool HasName => NameIndex >= 0;
    public bool HasPassword => PasswordIndex >= 0;
}

