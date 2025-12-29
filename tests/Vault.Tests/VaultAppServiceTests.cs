using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Vault.Application.Abstractions;
using Vault.Application.Services;
using Vault.Application.Models;
using Xunit;

namespace Vault.Application.Tests.Services
{
    public sealed class VaultAppServiceTests
    {
        [Fact]
        public async Task OpenAsync_WhenPathIsEmpty_ReturnsInvalidFormat()
        {
            var store = new FakeStore(_ => Task.FromResult<VaultFile>(null!));
            var crypto = new FakeCrypto(_ => throw new Exception("Should not be called"));

            var svc = new VaultAppService(store, crypto);

            var res = await svc.OpenAsync("", "pw".AsMemory());

            Assert.False(res.IsSuccess);
            Assert.NotNull(res.Error);
            Assert.Equal(VaultErrorCode.InvalidFormat, res.Error!.Code);
        }

        [Fact]
        public async Task OpenAsync_WhenStoreThrowsFileNotFound_ReturnsFileNotFound()
        {
            var store = new FakeStore(_ => throw new FileNotFoundException("nope"));
            var crypto = new FakeCrypto(_ => throw new Exception("Should not be called"));

            var svc = new VaultAppService(store, crypto);

            var res = await svc.OpenAsync("c:\\missing.vault", "pw".AsMemory());

            Assert.False(res.IsSuccess);
            Assert.Equal(VaultErrorCode.FileNotFound, res.Error!.Code);
        }

        [Fact]
        public async Task OpenAsync_WhenStoreThrowsUnauthorized_ReturnsAccessDenied()
        {
            var store = new FakeStore(_ => throw new UnauthorizedAccessException("denied"));
            var crypto = new FakeCrypto(_ => throw new Exception("Should not be called"));

            var svc = new VaultAppService(store, crypto);

            var res = await svc.OpenAsync("c:\\vault.vault", "pw".AsMemory());

            Assert.False(res.IsSuccess);
            Assert.Equal(VaultErrorCode.AccessDenied, res.Error!.Code);
        }

        [Fact]
        public async Task OpenAsync_WhenStoreThrowsIOException_ReturnsIoError()
        {
            var store = new FakeStore(_ => throw new IOException("io"));
            var crypto = new FakeCrypto(_ => throw new Exception("Should not be called"));

            var svc = new VaultAppService(store, crypto);

            var res = await svc.OpenAsync("c:\\vault.vault", "pw".AsMemory());

            Assert.False(res.IsSuccess);
            Assert.Equal(VaultErrorCode.IoError, res.Error!.Code);
        }

        [Fact]
        public async Task OpenAsync_WhenCryptoThrowsInvalidOperation_ReturnsInvalidFormat()
        {
            // store devuelve algo (aunque sea null!) porque el fake crypto no lo usa
            var store = new FakeStore(_ => Task.FromResult<VaultFile>(null!));
            var crypto = new FakeCrypto(_ => throw new InvalidOperationException("Invalid vault file."));

            var svc = new VaultAppService(store, crypto);

            var res = await svc.OpenAsync("c:\\vault.vault", "pw".AsMemory());

            Assert.False(res.IsSuccess);
            Assert.Equal(VaultErrorCode.InvalidFormat, res.Error!.Code);
        }

        [Fact]
        public async Task OpenAsync_WhenCryptoThrowsCryptographicException_ReturnsUnsupportedOrCorrupted()
        {
            var store = new FakeStore(_ => Task.FromResult<VaultFile>(null!));
            var crypto = new FakeCrypto(_ => throw new CryptographicException("tag mismatch"));

            var svc = new VaultAppService(store, crypto);

            var res = await svc.OpenAsync("c:\\vault.vault", "wrong".AsMemory());

            Assert.False(res.IsSuccess);
            Assert.Equal(VaultErrorCode.UnsupportedOrCorrupted, res.Error!.Code);
        }

        [Fact]
        public async Task OpenAsync_WhenUnexpectedException_ReturnsUnknown()
        {
            var store = new FakeStore(_ => throw new Exception("boom"));
            var crypto = new FakeCrypto(_ => throw new Exception("Should not be called"));

            var svc = new VaultAppService(store, crypto);

            var res = await svc.OpenAsync("c:\\vault.vault", "pw".AsMemory());

            Assert.False(res.IsSuccess);
            Assert.Equal(VaultErrorCode.Unknown, res.Error!.Code);
        }

        // ----- Fakes -----

        private sealed class FakeStore : IVaultStore
        {
            private readonly Func<string, Task<VaultFile>> _read;

            public FakeStore(Func<string, Task<VaultFile>> read) => _read = read;

            public Task<VaultFile> ReadAsync(string path, CancellationToken ct = default) => _read(path);

            public Task WriteAtomicAsync(string path, VaultFile file, CancellationToken ct = default)
                => throw new NotImplementedException("Not used in these tests.");
        }

        private sealed class FakeCrypto : IVaultCryptoService
        {
            private readonly Func<VaultFile, VaultUnlockResult> _unlock;

            public FakeCrypto(Func<VaultFile, VaultUnlockResult> unlock) => _unlock = unlock;

            public VaultUnlockResult UnlockVault(VaultFile file, ReadOnlySpan<char> masterPassword)
                => _unlock(file);

            public VaultCreateResult CreateVault(VaultDocument document,ReadOnlySpan<char> masterPassword,
                                                KdfProfile profile,DateTimeOffset? nowUtc = null)
                => throw new NotImplementedException("Not used in these tests.");
            public VaultFile SealForSave(VaultDocument document,VaultFileHeader currentHeader,
                                        ReadOnlySpan<byte> sessionKey,DateTimeOffset? nowUtc = null)
                => throw new NotImplementedException("Not used in these tests.");
        }
    }
}
