using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Vault.Application.Abstractions;
using Vault.Application.Import;
using Vault.Application.Import.Markdown;
using Vault.Application.Models;
using Vault.Application.Services;
using Vault.Application.UseCases;
using Vault.Domain;
using Xunit;

namespace Vault.Application.Tests.Services;

public sealed class VaultAppServiceEntryTests
{
    [Fact]
    public void AddEntry_WithValidEntry_Succeeds()
    {
        var svc = CreateSut();
        var doc = VaultDocument.CreateNew("Vault");
        var entry = VaultEntry.CreateNew("GitHub", "pw123", username: "carlos");

        var result = svc.AddEntry(doc, entry);

        Assert.True(result.IsSuccess);
        Assert.Equal(entry.Id, result.Value);
        Assert.Single(doc.Entries);
    }

    [Fact]
    public void AddEntry_WithDuplicate_Fails()
    {
        var svc = CreateSut();
        var doc = VaultDocument.CreateNew("Vault");
        var entry1 = VaultEntry.CreateNew("GitHub", "pw1", username: "carlos");
        var entry2 = VaultEntry.CreateNew("GitHub", "pw2", username: "carlos");

        svc.AddEntry(doc, entry1);
        var result = svc.AddEntry(doc, entry2);

        Assert.False(result.IsSuccess);
        Assert.Equal(VaultErrorCode.InvalidFormat, result.Error!.Code);
        Assert.Single(doc.Entries);
    }

    [Fact]
    public void UpdateEntry_WithValidChanges_Succeeds()
    {
        var svc = CreateSut();
        var doc = VaultDocument.CreateNew("Vault");
        var entry = VaultEntry.CreateNew("GitHub", "oldpw", username: "carlos");
        svc.AddEntry(doc, entry);

        var result = svc.UpdateEntry(
            doc,
            entry.Id,
            "GitHub Updated",
            "newpw",
            "newuser",
            "https://github.com",
            "notes",
            new[] { "dev" }.ToList());

        Assert.True(result.IsSuccess);
        Assert.Equal("GitHub Updated", entry.Name);
        Assert.Equal("newpw", entry.Password);
    }

    [Fact]
    public void UpdateEntry_WithNonExistentId_Fails()
    {
        var svc = CreateSut();
        var doc = VaultDocument.CreateNew("Vault");

        var result = svc.UpdateEntry(
            doc,
            Guid.NewGuid(),
            "Name",
            "pw",
            null, null, null,
            new List<string>());

        Assert.False(result.IsSuccess);
        Assert.Equal(VaultErrorCode.InvalidFormat, result.Error!.Code);
    }

    [Fact]
    public void UpdateEntry_CreatingDuplicate_Fails()
    {
        var svc = CreateSut();
        var doc = VaultDocument.CreateNew("Vault");
        var entry1 = VaultEntry.CreateNew("GitHub", "pw1", username: "carlos");
        var entry2 = VaultEntry.CreateNew("GitLab", "pw2", username: "john");
        svc.AddEntry(doc, entry1);
        svc.AddEntry(doc, entry2);

        var result = svc.UpdateEntry(
            doc,
            entry2.Id,
            "GitHub",
            "pw2",
            "carlos",
            null, null,
            new List<string>());

        Assert.False(result.IsSuccess);
        Assert.Equal(VaultErrorCode.InvalidFormat, result.Error!.Code);
    }

    [Fact]
    public void DeleteEntry_WithExistingEntry_Succeeds()
    {
        var svc = CreateSut();
        var doc = VaultDocument.CreateNew("Vault");
        var entry = VaultEntry.CreateNew("GitHub", "pw");
        svc.AddEntry(doc, entry);

        var result = svc.DeleteEntry(doc, entry.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(doc.Entries);
    }

    [Fact]
    public void DeleteEntry_WithNonExistentId_Fails()
    {
        var svc = CreateSut();
        var doc = VaultDocument.CreateNew("Vault");

        var result = svc.DeleteEntry(doc, Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(VaultErrorCode.InvalidFormat, result.Error!.Code);
    }

    [Fact]
    public async Task WriteNewVaultAsync_WithEmptyPath_Fails()
    {
        var svc = CreateSut();
        var file = new VaultFile(
            new VaultFileHeader(
                "VLT1", 1, 0, 1, 1, 1, 64 * 1024, 3, 1,
                new byte[16], new byte[12],
                DateTimeOffset.UtcNow.UtcTicks,
                DateTimeOffset.UtcNow.UtcTicks,
                new byte[16]),
            Array.Empty<byte>(),
            Array.Empty<byte>());

        var result = await svc.WriteNewVaultAsync("", file);

        Assert.False(result.IsSuccess);
        Assert.Equal(VaultErrorCode.InvalidPath, result.Error!.Code);
    }

    // ---------- helpers ----------

    private static VaultAppService CreateSut()
    {
        var store = new FakeStore();
        var crypto = new FakeCrypto();
        var saver = new FakeSaver();
        var import = new VaultImportService(new MarkdownVaultImporter());
        var entries = new EntryUseCases();

        return new VaultAppService(store, crypto, saver, import, entries);
    }

    private sealed class FakeStore : IVaultStore
    {
        public Task<VaultFile> ReadAsync(string path, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task WriteAtomicAsync(string path, VaultFile file, CancellationToken ct = default)
            => Task.CompletedTask;
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
