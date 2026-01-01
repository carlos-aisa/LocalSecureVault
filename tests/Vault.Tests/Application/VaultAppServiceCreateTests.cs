using System;
using System.Threading;
using System.Threading.Tasks;
using Vault.Application.Abstractions;
using Vault.Application.Models;
using Vault.Application.Import.Markdown;
using Vault.Application.Services;
using Vault.Application.UseCases;
using Xunit;

namespace Vault.Tests;

public class VaultAppServiceCreateTests
{
    [Fact]
    public void CreateInMemory_WithValidData_ReturnsCreatedVault()
    {
        var crypto = new FakeCryptoService();
        var svc = new VaultAppService(
            new FakeVaultStore(),
            crypto,
            new VaultSaveService(new FakeVaultStore(), crypto),
            new VaultImportService(new MarkdownVaultImporter()),
            new EntryUseCases()
        );

        var result = svc.CreateInMemory("TestVault", "password".AsMemory(), KdfProfile.Interactive);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("TestVault", result.Value!.Document.Meta.VaultName);
        Assert.NotEmpty(result.Value.SessionKey);
    }

    [Fact]
    public void CreateInMemory_WithEmptyVaultName_ReturnsError()
    {
        var crypto = new FakeCryptoService();
        var svc = new VaultAppService(
            new FakeVaultStore(),
            crypto,
            new VaultSaveService(new FakeVaultStore(), crypto),
            new VaultImportService(new MarkdownVaultImporter()),
            new EntryUseCases()
        );

        var result = svc.CreateInMemory("", "password".AsMemory(), KdfProfile.Interactive);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(VaultErrorCode.InvalidFormat, result.Error!.Code);
        Assert.Contains("Vault name is required", result.Error.UserMessage);
    }

    [Fact]
    public void CreateInMemory_WithWhitespaceVaultName_ReturnsError()
    {
        var crypto = new FakeCryptoService();
        var svc = new VaultAppService(
            new FakeVaultStore(),
            crypto,
            new VaultSaveService(new FakeVaultStore(), crypto),
            new VaultImportService(new MarkdownVaultImporter()),
            new EntryUseCases()
        );

        var result = svc.CreateInMemory("   ", "password".AsMemory(), KdfProfile.Interactive);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(VaultErrorCode.InvalidFormat, result.Error!.Code);
        Assert.Contains("Vault name is required", result.Error.UserMessage);
    }

    [Fact]
    public void CreateInMemory_WithEmptyPassword_ReturnsError()
    {
        var crypto = new FakeCryptoService();
        var svc = new VaultAppService(
            new FakeVaultStore(),
            crypto,
            new VaultSaveService(new FakeVaultStore(), crypto),
            new VaultImportService(new MarkdownVaultImporter()),
            new EntryUseCases()
        );

        var result = svc.CreateInMemory("TestVault", ReadOnlyMemory<char>.Empty, KdfProfile.Interactive);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(VaultErrorCode.InvalidFormat, result.Error!.Code);
        Assert.Contains("Master password is required", result.Error.UserMessage);
    }

    [Fact]
    public void CreateInMemory_WithDifferentKdfProfiles_CreatesVault()
    {
        var crypto = new FakeCryptoService();
        var svc = new VaultAppService(
            new FakeVaultStore(),
            crypto,
            new VaultSaveService(new FakeVaultStore(), crypto),
            new VaultImportService(new MarkdownVaultImporter()),
            new EntryUseCases()
        );

        var interactiveResult = svc.CreateInMemory("TestVault1", "password".AsMemory(), KdfProfile.Interactive);
        var strongResult = svc.CreateInMemory("TestVault2", "password".AsMemory(), KdfProfile.Strong);

        Assert.True(interactiveResult.IsSuccess);
        Assert.True(strongResult.IsSuccess);
        Assert.NotNull(interactiveResult.Value);
        Assert.NotNull(strongResult.Value);
    }

    [Fact]
    public void CreateInMemory_WhenCryptoThrowsException_ReturnsUnknownError()
    {
        var crypto = new ThrowingCryptoService();
        var svc = new VaultAppService(
            new FakeVaultStore(),
            crypto,
            new VaultSaveService(new FakeVaultStore(), crypto),
            new VaultImportService(new MarkdownVaultImporter()),
            new EntryUseCases()
        );

        var result = svc.CreateInMemory("TestVault", "password".AsMemory(), KdfProfile.Interactive);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(VaultErrorCode.Unknown, result.Error!.Code);
        Assert.Contains("Unexpected error", result.Error.UserMessage);
    }
    private class FakeCryptoService : IVaultCryptoService
    {
        public VaultCreateResult CreateVault(VaultDocument doc, ReadOnlySpan<char> masterPassword, KdfProfile profile, DateTimeOffset? nowUtc = null)
        {
            var now = nowUtc ?? DateTimeOffset.UtcNow;
            var header = new VaultFileHeader(
                Magic: "VLT1",
                Version: 1,
                Flags: 0,
                KdfId: 1,
                PayloadEncoding: 1,
                SchemaVersion: 1,
                Argon2MemoryKiB: 64 * 1024,
                Argon2Iterations: 3,
                Argon2Parallelism: 1,
                Salt: new byte[16],
                Nonce: new byte[12],
                CreatedUtcTicks: now.UtcTicks,
                UpdatedUtcTicks: now.UtcTicks,
                Reserved: new byte[16]
            );
            var file = new VaultFile(header, new byte[64], new byte[16]);
            return new VaultCreateResult(file, new byte[32]);
        }

        public VaultUnlockResult UnlockVault(VaultFile file, ReadOnlySpan<char> masterPassword)
            => throw new NotImplementedException();

        public VaultFile SealForSave(VaultDocument document, VaultFileHeader currentHeader, ReadOnlySpan<byte> sessionKey, DateTimeOffset? nowUtc = null)
            => throw new NotImplementedException();
    }

    private class ThrowingCryptoService : IVaultCryptoService
    {
        public VaultCreateResult CreateVault(VaultDocument doc, ReadOnlySpan<char> masterPassword, KdfProfile profile, DateTimeOffset? nowUtc = null)
            => throw new InvalidOperationException("Crypto error");

        public VaultUnlockResult UnlockVault(VaultFile file, ReadOnlySpan<char> masterPassword)
            => throw new NotImplementedException();

        public VaultFile SealForSave(VaultDocument document, VaultFileHeader currentHeader, ReadOnlySpan<byte> sessionKey, DateTimeOffset? nowUtc = null)
            => throw new NotImplementedException();
    }

    private class FakeVaultStore : IVaultStore
    {
        public Task<VaultFile> ReadAsync(string path, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task WriteAtomicAsync(string path, VaultFile file, CancellationToken ct = default)
            => throw new NotImplementedException();
    }
}

