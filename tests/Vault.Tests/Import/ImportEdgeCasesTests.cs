using System;
using System.Linq;
using Vault.Application.Import;
using Vault.Application.Import.Markdown;
using Vault.Application.Models;
using Vault.Domain;
using Xunit;

namespace Vault.Tests.Import;

public sealed class ImportEdgeCasesTests
{
    private readonly MarkdownVaultImporter _importer = new();

    [Fact]
    public void Parse_EmptyMarkdown_ReturnsNoEntriesNoIssues()
    {
        var result = _importer.Parse("");

        Assert.NotNull(result);
        Assert.Empty(result.Entries);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Parse_TextWithoutTables_ReturnsNoEntries()
    {
        var md = """
        # Carlos
        Esto es texto suelto.
        - lista
        - sin tablas
        """;

        var result = _importer.Parse(md);

        Assert.Empty(result.Entries);

        // If your parser adds warnings for “no tables”, leave this.
        // If it does NOT add warnings in this case, change to Assert.Empty(...)
        // Assert.NotEmpty(result.Warnings);
    }

    [Fact]
    public void BuildApplyPlan_SameNameAndUser_DifferentTags_AreNotDuplicates()
    {
        var now = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var vault = VaultDocument.CreateNew("test", now);

        // Import 1: Tag Carlos
        var md1 = """
        # Carlos
        | Nombre | Usuario | Contraseña |
        | --- | --- | --- |
        | DGT | cuenta | p1 |
        """;

        // Import 2: Tag Empresa
        var md2 = """
        # Empresa
        | Nombre | Usuario | Contraseña |
        | --- | --- | --- |
        | DGT | cuenta | p2 |
        """;

        var r1 = _importer.Parse(md1);
        var r2 = _importer.Parse(md2);

        var p1 = ImportPlanner.BuildApplyPlan(vault, r1, now);
        _importer.Apply(vault, p1, now);

        var p2 = ImportPlanner.BuildApplyPlan(vault, r2, now);

        Assert.Single(p2.AddActions);
        Assert.Empty(p2.SkippedDuplicates);
    }

    [Fact]
    public void BuildApplyPlan_NormalizesFields_ForDuplicateDetection()
    {
        var now = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var vault = VaultDocument.CreateNew("test", now);

        // Entry existing “normal”
        vault.AddEntry(VaultEntry.CreateNew(
            name: "DGT",
            password: "old",
            username: "cuenta",
            url: null,
            notes: null,
            tags: new[] { "Carlos" },
            nowUtc: now), now);

        // Import with spaces/capital letters
        var md = """
        #  CARLOS  
        | Nombre | Usuario | Contraseña |
        | --- | --- | --- |
        |  dgt   |  CUENTA  |  newpass  |
        """;

        var parsed = _importer.Parse(md);
        var plan = ImportPlanner.BuildApplyPlan(vault, parsed, now);

        Assert.Empty(plan.AddActions);
        Assert.Single(plan.SkippedDuplicates);
    }

    [Fact]
    public void Apply_WithNoAddActions_DoesNotModifyVault()
    {
        var t0 = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var t1 = t0.AddMinutes(10);

        var vault = VaultDocument.CreateNew("test", t0);

        // We simulate an empty plan (all duplicates or nothing to add)
        var emptyPlan = new Vault.Application.Import.Models.ImportApplyPlan(){
            AddActions = Array.Empty<Vault.Application.Import.Models.ImportAddAction>(),
            SkippedDuplicates = Array.Empty<Vault.Application.Import.Models.ImportEntryDraft>()};

        var beforeCount = vault.Entries.Count;
        var beforeUpdated = vault.Meta.UpdatedUtc;

        var res = _importer.Apply(vault, emptyPlan, t1);

        Assert.Equal(0, res.Added);
        Assert.Equal(0, res.Skipped);
        Assert.Equal(beforeCount, vault.Entries.Count);

        // If Apply/Touch does NOT touch UpdatedUtc when there are no changes, this should remain the same
        Assert.Equal(beforeUpdated, vault.Meta.UpdatedUtc);
    }
}