using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vault.Application.Abstractions;
using Vault.Application.Models;
using Vault.Application.Services;
using Xunit;

namespace Vault.Application.Tests.Services
{
    public sealed class VaultSaveServiceTests
    {
        [Fact]
        public async Task TrySaveAsync_WhenPathIsEmpty_ReturnsInvalidPath()
        {
            var store = new FakeStore((_, __, ___) => Task.CompletedTask);
            var crypto = new FakeCrypto((_, __, ___) => DummyVaultFile());

            var svc = new VaultSaveService(store, crypto);

            var res = await svc.TrySaveAsync(
                path: "",
                document: DummyDocument(),
                sessionKey: new byte[32],
                currentHeader: DummyHeader(),
                ct: CancellationToken.None);

            Assert.False(res.IsSuccess);
            Assert.Equal(VaultErrorCode.InvalidPath, res.Error!.Code);
        }

        [Fact]
        public async Task TrySaveAsync_WhenStoreThrowsUnauthorizedAccess_ReturnsAccessDenied()
        {
            var store = new FakeStore((_, __, ___) => throw new UnauthorizedAccessException());
            var crypto = new FakeCrypto((_, __, ___) => DummyVaultFile());

            var svc = new VaultSaveService(store, crypto);

            var res = await svc.TrySaveAsync(
                path: "c:\\vault.vlt",
                document: DummyDocument(),
                sessionKey: new byte[32],
                currentHeader: DummyHeader(),
                ct: CancellationToken.None);

            Assert.False(res.IsSuccess);
            Assert.Equal(VaultErrorCode.AccessDenied, res.Error!.Code);
        }

        [Fact]
        public async Task TrySaveAsync_WhenStoreThrowsIOException_ReturnsIoError()
        {
            var store = new FakeStore((_, __, ___) => throw new IOException("disk error"));
            var crypto = new FakeCrypto((_, __, ___) => DummyVaultFile());

            var svc = new VaultSaveService(store, crypto);

            var res = await svc.TrySaveAsync(
                path: "c:\\vault.vlt",
                document: DummyDocument(),
                sessionKey: new byte[32],
                currentHeader: DummyHeader(),
                ct: CancellationToken.None);

            Assert.False(res.IsSuccess);
            Assert.Equal(VaultErrorCode.IoError, res.Error!.Code);
        }

        [Fact]
        public async Task TrySaveAsync_WhenUnexpectedException_ReturnsUnknown()
        {
            var store = new FakeStore((_, __, ___) => throw new Exception("boom"));
            var crypto = new FakeCrypto((_, __, ___) => DummyVaultFile());

            var svc = new VaultSaveService(store, crypto);

            var res = await svc.TrySaveAsync(
                path: "c:\\vault.vlt",
                document: DummyDocument(),
                sessionKey: new byte[32],
                currentHeader: DummyHeader(),
                ct: CancellationToken.None);

            Assert.False(res.IsSuccess);
            Assert.Equal(VaultErrorCode.Unknown, res.Error!.Code);
        }
        private sealed class FakeStore : IVaultStore
        {
            private readonly Func<string, VaultFile, CancellationToken, Task> _write;

            public FakeStore(Func<string, VaultFile, CancellationToken, Task> write) => _write = write;

            public Task<VaultFile> ReadAsync(string path, CancellationToken ct = default)
                => throw new NotImplementedException("Not needed for save tests.");

            public Task WriteAtomicAsync(string path, VaultFile file, CancellationToken ct = default)
                => _write(path, file, ct);
        }

        private sealed class FakeCrypto : IVaultCryptoService
        {
            private readonly Func<VaultDocument, VaultFileHeader, byte[], VaultFile> _seal;

            public FakeCrypto(Func<VaultDocument, VaultFileHeader, byte[], VaultFile> seal) => _seal = seal;

            public VaultUnlockResult UnlockVault(VaultFile file, ReadOnlySpan<char> masterPassword)
                => throw new NotImplementedException("Not needed for save tests.");

            public VaultFile SealForSave(VaultDocument document,VaultFileHeader currentHeader,
                                        ReadOnlySpan<byte> sessionKey,DateTimeOffset? nowUtc = null)
                => _seal(document, currentHeader, sessionKey.ToArray());

            public VaultCreateResult CreateVault(VaultDocument document,ReadOnlySpan<char> masterPassword,
                                 KdfProfile profile,DateTimeOffset? nowUtc = null)
                => throw new NotImplementedException("Not needed for save tests.");
        }
        private static VaultDocument DummyDocument()
            => VaultDocument.CreateNew("test-vault");

        private static VaultFileHeader DummyHeader()
            => new VaultFileHeader(
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

        private static VaultFile DummyVaultFile()
            => new VaultFile(
                Header: DummyHeader(),
                Ciphertext: Array.Empty<byte>(),
                Tag: Array.Empty<byte>()
            );
    }
}
