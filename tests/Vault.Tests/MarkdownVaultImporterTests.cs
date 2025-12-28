using System;
using System.Linq;
using Vault.Application.Import;
using Vault.Application.Import.Markdown;
using Vault.Application.Models;
using Vault.Domain;
using Xunit;

namespace Vault.Tests.Import;

public sealed class MarkdownVaultImporterTests
{
    private readonly MarkdownVaultImporter _importer = new();

    [Fact]
    public void Parse_ValidTable_WithHeadings_AssignsTagsAndFields()
    {
        var md = """
        # Carlos
        ## DGT

        | Nombre | Usuario | Contraseña | Info extra |
        | --- | --- | --- | --- |
        | cuenta | xxxxx | pass123 | note |
        """;

        var result = _importer.Parse(md);

        Assert.NotNull(result);
        Assert.Single(result.Entries);
        Assert.Empty(result.Issues);

        var e = result.Entries[0];
        Assert.Equal("cuenta", e.Name);
        Assert.Equal("xxxxx", e.Identifier);
        Assert.Equal("pass123", e.Password);
        Assert.Equal("note", e.Notes);

        Assert.Equal(2, e.Tags.Count);
        Assert.Equal("Carlos", e.Tags[0]);
        Assert.Equal("DGT", e.Tags[1]);
    }

    [Fact]
    public void Parse_ColumnsOutOfOrder_IsMappedCorrectly()
    {
        var md = """
        # Root

        | Contraseña | Nombre | Info extra | Usuario |
        | --- | --- | --- | --- |
        | p1 | N1 | E1 | U1 |
        """;

        var result = _importer.Parse(md);

        Assert.Single(result.Entries);
        var e = result.Entries[0];
        Assert.Equal("N1", e.Name);
        Assert.Equal("U1", e.Identifier);
        Assert.Equal("p1", e.Password);
        Assert.Equal("E1", e.Notes);
        Assert.Single(e.Tags); // solo h1
        Assert.Equal("Root", e.Tags[0]);
    }

    [Fact]
    public void Parse_TableMissingRequiredColumns_IsIgnoredAndWarned()
    {
        var md = """
        # Root

        | Nombre | Usuario | Info extra |
        | --- | --- | --- |
        | N1 | U1 | E1 |
        """;

        var result = _importer.Parse(md);

        Assert.Empty(result.Entries);
        Assert.Single(result.Issues);

        var w = result.Issues[0];
        Assert.Equal(Vault.Application.Import.Models.ImportSeverity.Warning, w.Severity);
        Assert.Contains("Tabla ignorada", w.Message);
    }

    [Fact]
    public void BuildApplyPlan_VaultEmpty_AddsAll()
    {
        var now = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var vault = VaultDocument.CreateNew("test", now);

        var md = """
        # Carlos

        | Nombre | Usuario | Contraseña | Info extra |
        | --- | --- | --- | --- |
        | DGT | cuenta | p1 | e1 |
        | ING | VALIDACION MOVIL | p2 | e2 |
        """;

        var parsed = _importer.Parse(md);

        var plan = ImportPlanner.BuildApplyPlan(vault, parsed, now);

        Assert.Equal(2, plan.AddActions.Count);
        Assert.Empty(plan.SkippedDuplicates);
    }

    [Fact]
    public void BuildApplyPlan_SkipsDuplicates_ExistingVaultEntry()
    {
        var now = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var vault = VaultDocument.CreateNew("test", now);

        // Entrada existente
        vault.AddEntry(VaultEntry.CreateNew(
            name: "DGT",
            password: "old",
            username: "cuenta",
            notes: "old-notes",
            tags: new[] { "Carlos" },
            nowUtc: now), now);

        var md = """
        # Carlos

        | Nombre | Usuario | Contraseña | Info extra |
        | --- | --- | --- | --- |
        | DGT | cuenta | newpass | newnotes |
        """;

        var parsed = _importer.Parse(md);
        var plan = ImportPlanner.BuildApplyPlan(vault, parsed, now);

        Assert.Empty(plan.AddActions);
        Assert.Single(plan.SkippedDuplicates);
    }

    [Fact]
    public void BuildApplyPlan_SkipsDuplicates_WithinSameImport()
    {
        var now = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var vault = VaultDocument.CreateNew("test", now);

        var md = """
        # Carlos

        | Nombre | Usuario | Contraseña |
        | --- | --- | --- |
        | DGT | cuenta | p1 |
        | DGT | cuenta | p2 |
        """;

        var parsed = _importer.Parse(md);
        var plan = ImportPlanner.BuildApplyPlan(vault, parsed, now);

        Assert.Single(plan.AddActions);
        Assert.Single(plan.SkippedDuplicates);
    }

    [Fact]
    public void Apply_AddsEntriesAndTouchesVaultUpdatedUtc()
    {
        var t0 = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var t1 = t0.AddMinutes(5);

        var vault = VaultDocument.CreateNew("test", t0);

        var md = """
        # Carlos

        | Nombre | Usuario | Contraseña | Info extra |
        | --- | --- | --- | --- |
        | DGT | cuenta | p1 | e1 |
        | ING | VALIDACION MOVIL | p2 | e2 |
        """;

        var parsed = _importer.Parse(md);
        var plan = ImportPlanner.BuildApplyPlan(vault, parsed, t1);

        var res = _importer.Apply(vault, plan, t1);

        Assert.Equal(2, res.Added);
        Assert.Equal(0, res.Skipped);

        Assert.Equal(2, vault.Entries.Count);

        // Touch() debe haber actualizado meta.UpdatedUtc (si tu VaultMetadata lo expone así)
        Assert.True(vault.Meta.UpdatedUtc >= t1);
    }
}
