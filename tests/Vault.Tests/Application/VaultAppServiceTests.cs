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

public sealed class VaultAppServiceOpenTests
{
    [Fact]
    public async Task OpenAsync_WhenPathIsEmpty_ReturnsInvalidPath()
    {
        var svc = CreateSut(
            storeRead: _ => Task.FromResult<VaultFile>(null!),
            cryptoUnlock: _ => throw new Exception("should not be called"));

        var res = await svc.OpenAsync("", "pw".AsMemory());

        Assert.False(res.IsSuccess);
        Assert.Equal(VaultErrorCode.InvalidPath, res.Error!.Code);
    }

    [Fact]
    public async Task OpenAsync_WhenStoreThrowsFileNotFound_ReturnsFileNotFound()
    {
        var svc = CreateSut(
            storeRead: _ => throw new FileNotFoundException(),
            cryptoUnlock: _ => throw new Exception("should not be called"));

        var res = await svc.OpenAsync("c:\\missing.vlt", "pw".AsMemory());

        Assert.False(res.IsSuccess);
        Assert.Equal(VaultErrorCode.FileNotFound, res.Error!.Code);
    }

    [Fact]
    public async Task OpenAsync_WhenCryptoThrowsInvalidOperation_ReturnsInvalidFormat()
    {
        var svc = CreateSut(
            storeRead: _ => Task.FromResult<VaultFile>(DummyVaultFile()),
            cryptoUnlock: _ => throw new InvalidOperationException());

        var res = await svc.OpenAsync("c:\\vault.vlt", "pw".AsMemory());

        Assert.False(res.IsSuccess);
        Assert.Equal(VaultErrorCode.InvalidFormat, res.Error!.Code);
    }

    [Fact]
    public async Task OpenAsync_WhenCryptoThrowsCryptographicException_ReturnsUnsupportedOrCorrupted()
    {
        var svc = CreateSut(
            storeRead: _ => Task.FromResult<VaultFile>(DummyVaultFile()),
            cryptoUnlock: _ => throw new CryptographicException());

        var res = await svc.OpenAsync("c:\\vault.vlt", "pw".AsMemory());

        Assert.False(res.IsSuccess);
        Assert.Equal(VaultErrorCode.UnsupportedOrCorrupted, res.Error!.Code);
    }
    private static VaultAppService CreateSut(
        Func<string, Task<VaultFile>> storeRead,
        Func<VaultFile, VaultUnlockResult> cryptoUnlock)
    {
        var store = new FakeStore(storeRead);
        var crypto = new FakeCrypto(cryptoUnlock);
        var saver = new FakeSaver();
        var import = new FakeImportService();
        var entries = new EntryUseCases();

        return new VaultAppService(store, crypto, saver, import, entries);
    }

    private sealed class FakeStore : IVaultStore
    {
        private readonly Func<string, Task<VaultFile>> _read;
        public FakeStore(Func<string, Task<VaultFile>> read) => _read = read;

        public Task<VaultFile> ReadAsync(string path, CancellationToken ct = default) => _read(path);

        public Task WriteAtomicAsync(string path, VaultFile file, CancellationToken ct = default)
            => throw new NotImplementedException();
    }

    private sealed class FakeCrypto : IVaultCryptoService
    {
        private readonly Func<VaultFile, VaultUnlockResult> _unlock;
        public FakeCrypto(Func<VaultFile, VaultUnlockResult> unlock) => _unlock = unlock;

        public VaultUnlockResult UnlockVault(VaultFile file, ReadOnlySpan<char> masterPassword) => _unlock(file);

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

    private sealed class FakeImportService : VaultImportService
    {
        public FakeImportService() : base(new MarkdownVaultImporter()) { }
    }

    private static VaultFile DummyVaultFile()
    {
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
            CreatedUtcTicks: DateTimeOffset.UtcNow.UtcTicks,
            UpdatedUtcTicks: DateTimeOffset.UtcNow.UtcTicks,
            Reserved: new byte[16]
        );

        return new VaultFile(header, Array.Empty<byte>(), Array.Empty<byte>());
    }
}
