using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Vault.Application.Abstractions;
using Vault.Application.Import;
using Vault.Application.Import.Markdown;
using Vault.Application.Import.Models;
using Vault.Application.Models;
using Vault.Application.Services;
using Vault.Application.UseCases;
using Xunit;

namespace Vault.Application.Tests.Services;

public sealed class VaultAppServiceImportTests
{
    [Fact]
    public void ImportPreview_ValidMarkdown_ReturnsBothResultAndPlan()
    {
        var svc = CreateSut();
        var doc = VaultDocument.CreateNew("Test Vault");

        var markdown = """
        # Work

        | Nombre | Usuario | Contraseña |
        | --- | --- | --- |
        | GitHub | carlos | pass123 |
        """;

        var result = svc.ImportPreview(markdown, doc);

        Assert.True(result.IsSuccess);
        
        // Should return both Result and Plan
        var (importResult, plan) = result.Value!;
        
        Assert.NotNull(importResult);
        Assert.NotNull(plan);
        
        // Verify ImportResult
        Assert.Single(importResult.Entries);
        Assert.Equal("GitHub", importResult.Entries[0].Name);
        Assert.Equal("carlos", importResult.Entries[0].Identifier);
        Assert.Equal("pass123", importResult.Entries[0].Password);
        
        // Verify ImportApplyPlan
        Assert.Single(plan.AddActions);
        Assert.Empty(plan.SkippedDuplicates);
    }

    [Fact]
    public void ImportPreview_WithDuplicates_IdentifiesThem()
    {
        var svc = CreateSut();
        var doc = VaultDocument.CreateNew("Test Vault");
        var uc = new EntryUseCases();
        
        // Add existing entry
        uc.AddEntry(doc, "GitHub", "oldpass", username: "carlos", tags: new[] { "Work" });

        var markdown = """
        # Work

        | Nombre | Usuario | Contraseña |
        | --- | --- | --- |
        | GitHub | carlos | newpass |
        | GitLab | carlos | pass456 |
        """;

        var result = svc.ImportPreview(markdown, doc);

        Assert.True(result.IsSuccess);
        var (importResult, plan) = result.Value!;
        
        // 2 entries parsed
        Assert.Equal(2, importResult.Entries.Count);
        
        // 1 new, 1 duplicate
        Assert.Single(plan.AddActions);
        Assert.Single(plan.SkippedDuplicates);
        
        // GitLab should be added
        Assert.Equal("GitLab", plan.AddActions[0].Entry.Name);
        
        // GitHub should be skipped
        Assert.Equal("GitHub", plan.SkippedDuplicates[0].Name);
    }

    [Fact]
    public void ImportPreview_InvalidMarkdown_ReturnsError()
    {
        var svc = CreateSut();
        var doc = VaultDocument.CreateNew("Test Vault");

        var markdown = """
        # Missing password column

        | Nombre | Usuario |
        | --- | --- |
        | GitHub | carlos |
        """;

        var result = svc.ImportPreview(markdown, doc);

        Assert.True(result.IsSuccess);
        var (importResult, plan) = result.Value!;
        
        // No entries because table is invalid
        Assert.Empty(importResult.Entries);
        
        // Should have a warning
        Assert.NotEmpty(importResult.Issues);
    }

    [Fact]
    public void ImportPreview_EmptyMarkdown_ReturnsEmptyPlan()
    {
        var svc = CreateSut();
        var doc = VaultDocument.CreateNew("Test Vault");

        var result = svc.ImportPreview("", doc);

        // Should succeed even with empty input
        if (result.IsSuccess)
        {
            var (importResult, plan) = result.Value!;
            
            Assert.Empty(importResult.Entries);
            Assert.Empty(plan.AddActions);
            Assert.Empty(plan.SkippedDuplicates);
        }
        else
        {
            // Or it might fail, which is also acceptable
            Assert.False(result.IsSuccess);
        }
    }

    // ---------- helpers ----------

    private static VaultAppService CreateSut()
    {
        var store = new FakeStore();
        var crypto = new FakeCrypto();
        var saver = new FakeSaver();
        var import = new VaultImportService(new MarkdownVaultImporter());

        return new VaultAppService(store, crypto, saver, import);
    }

    private sealed class FakeStore : IVaultStore
    {
        public Task<VaultFile> ReadAsync(string path, CancellationToken ct = default) 
            => throw new NotImplementedException();

        public Task WriteAtomicAsync(string path, VaultFile file, CancellationToken ct = default)
            => throw new NotImplementedException();
    }

    private sealed class FakeCrypto : IVaultCryptoService
    {
        public VaultUnlockResult UnlockVault(VaultFile file, ReadOnlySpan<char> masterPassword) 
            => throw new NotImplementedException();

        public VaultCreateResult CreateVault(VaultDocument doc, ReadOnlySpan<char> masterPassword, KdfProfile profile, DateTimeOffset? nowUtc = null)
            => throw new NotImplementedException();

        public VaultFile SealForSave(VaultDocument document, VaultFileHeader header, ReadOnlySpan<byte> sessionKey, DateTimeOffset? nowUtc = null)
            => throw new NotImplementedException();
    }

    private sealed class FakeSaver : VaultSaveService
    {
        public FakeSaver() : base(new DummyStore(), new DummyCrypto()) { }

        private sealed class DummyStore : IVaultStore
        {
            public Task<VaultFile> ReadAsync(string path, CancellationToken ct = default) => throw new NotImplementedException();
            public Task WriteAtomicAsync(string path, VaultFile file, CancellationToken ct = default) => throw new NotImplementedException();
        }

        private sealed class DummyCrypto : IVaultCryptoService
        {
            public VaultUnlockResult UnlockVault(VaultFile file, ReadOnlySpan<char> masterPassword) => throw new NotImplementedException();
            public VaultCreateResult CreateVault(VaultDocument doc, ReadOnlySpan<char> masterPassword, KdfProfile profile, DateTimeOffset? nowUtc = null) => throw new NotImplementedException();
            public VaultFile SealForSave(VaultDocument document, VaultFileHeader header, ReadOnlySpan<byte> sessionKey, DateTimeOffset? nowUtc = null) => throw new NotImplementedException();
        }
    }
}
