using Vault.Application.Import.Models;
using Vault.Application.Models;
using Vault.Domain;

namespace Vault.Application.Import;

public static class ImportPlanner
{
    public static ImportApplyPlan BuildApplyPlan(
        VaultDocument vault,
        ImportResult parsed,
        DateTimeOffset? nowUtc = null)
    {
        ArgumentNullException.ThrowIfNull(vault);
        ArgumentNullException.ThrowIfNull(parsed);

        var now = nowUtc ?? DateTimeOffset.UtcNow;

        // Índice de entradas existentes (para dedupe O(1))
        var existingKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var e in vault.Entries)
        {
            existingKeys.Add(ComputeKey(
                e.Name,
                e.Username,
                e.Tags
            ));
        }

        var addActions = new List<ImportAddAction>();
        var skipped = new List<ImportEntryDraft>();

        foreach (var d in parsed.Entries)
        {
            // Defensa final (aunque el parser ya filtre)
            if (string.IsNullOrWhiteSpace(d.Name) ||
                string.IsNullOrWhiteSpace(d.Password))
            {
                continue;
            }

            var key = ComputeKey(d.Name, d.Identifier, d.Tags);

            // HashSet.Add returns false if already existed
            if (!existingKeys.Add(key))
            {
                skipped.Add(d);
                continue;
            }

            var entry = VaultEntry.CreateNew(
                name: d.Name,
                password: d.Password!,
                username: d.Identifier,
                url: null,
                notes: d.Notes,
                tags: d.Tags,
                nowUtc: now
            );

            addActions.Add(new ImportAddAction(){Entry = entry});
        }

        return new ImportApplyPlan(){
            AddActions = addActions,
            SkippedDuplicates = skipped
        };
    }

    private static string ComputeKey(
        string name,
        string? username,
        IReadOnlyCollection<string> tags)
    {
        static string N(string? s) =>
            string.IsNullOrWhiteSpace(s)
                ? ""
                : s.Trim().ToLowerInvariant();

        var tagPart = tags is null || tags.Count == 0
            ? ""
            : string.Join("|",
                tags
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(N)
                    .Order(StringComparer.Ordinal));

        return $"{N(name)}::{N(username)}::{tagPart}";
    }
}
