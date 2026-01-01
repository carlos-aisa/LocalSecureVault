using System.IO;
using Vault.Application.Abstractions;
using Vault.Storage;
using Xunit;

namespace Vault.Tests;

public class FileFormatValidationTests
{
    [Fact]
    public async Task ReadAsync_WrongMagic_ReadsButShouldBeRejectedByApplication()
    {
        var store = new FileVaultStore();
        var temp = Path.GetTempFileName();

        try
        {
            var header = new VaultFileHeader(
                Magic: "BAD!",
                Version: VaultFormatConstants.Version,
                Flags: 0,
                KdfId: VaultFormatConstants.KdfIdArgon2id,
                PayloadEncoding: VaultFormatConstants.PayloadJsonUtf8,
                SchemaVersion: 1,
                Argon2MemoryKiB: 65536,
                Argon2Iterations: 2,
                Argon2Parallelism: 2,
                Salt: new byte[VaultFormatConstants.SaltSize],
                Nonce: new byte[VaultFormatConstants.NonceSize],
                CreatedUtcTicks: 1,
                UpdatedUtcTicks: 1,
                Reserved: new byte[VaultFormatConstants.ReservedSize]
            );

            var file = new VaultFile(header, new byte[] { 1, 2, 3 }, new byte[VaultFormatConstants.TagSize]);
            await store.WriteAtomicAsync(temp, file);

            var read = await store.ReadAsync(temp);
            
            Assert.NotEqual(VaultFormatConstants.Magic, read.Header.Magic);
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public async Task ReadAsync_UnsupportedVersion_ReadsButShouldBeRejectedByApplication()
    {
        var store = new FileVaultStore();
        var temp = Path.GetTempFileName();

        try
        {
            var header = new VaultFileHeader(
                Magic: VaultFormatConstants.Magic,
                Version: 999,
                Flags: 0,
                KdfId: VaultFormatConstants.KdfIdArgon2id,
                PayloadEncoding: VaultFormatConstants.PayloadJsonUtf8,
                SchemaVersion: 1,
                Argon2MemoryKiB: 65536,
                Argon2Iterations: 2,
                Argon2Parallelism: 2,
                Salt: new byte[VaultFormatConstants.SaltSize],
                Nonce: new byte[VaultFormatConstants.NonceSize],
                CreatedUtcTicks: 1,
                UpdatedUtcTicks: 1,
                Reserved: new byte[VaultFormatConstants.ReservedSize]
            );

            var file = new VaultFile(header, new byte[] { 1, 2, 3 }, new byte[VaultFormatConstants.TagSize]);
            await store.WriteAtomicAsync(temp, file);

            var read = await store.ReadAsync(temp);
            
            Assert.NotEqual(VaultFormatConstants.Version, read.Header.Version);
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public async Task ReadAsync_FileShorterThanHeader_ThrowsException()
    {
        var temp = Path.GetTempFileName();

        try
        {
            await File.WriteAllBytesAsync(temp, new byte[50]); // Only 50 bytes, need 82 + tag

            var store = new FileVaultStore();
            await Assert.ThrowsAnyAsync<Exception>(() => store.ReadAsync(temp));
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public async Task ReadAsync_ValidFile_Succeeds()
    {
        var store = new FileVaultStore();
        var temp = Path.GetTempFileName();

        try
        {
            var header = new VaultFileHeader(
                Magic: VaultFormatConstants.Magic,
                Version: VaultFormatConstants.Version,
                Flags: 0,
                KdfId: VaultFormatConstants.KdfIdArgon2id,
                PayloadEncoding: VaultFormatConstants.PayloadJsonUtf8,
                SchemaVersion: 1,
                Argon2MemoryKiB: 65536,
                Argon2Iterations: 2,
                Argon2Parallelism: 2,
                Salt: new byte[VaultFormatConstants.SaltSize],
                Nonce: new byte[VaultFormatConstants.NonceSize],
                CreatedUtcTicks: 1,
                UpdatedUtcTicks: 1,
                Reserved: new byte[VaultFormatConstants.ReservedSize]
            );

            var file = new VaultFile(header, new byte[] { 1, 2, 3 }, new byte[VaultFormatConstants.TagSize]);
            await store.WriteAtomicAsync(temp, file);
            var read = await store.ReadAsync(temp);
            
            Assert.NotNull(read);
            Assert.Equal(VaultFormatConstants.Magic, read.Header.Magic);
            Assert.Equal(VaultFormatConstants.Version, read.Header.Version);
        }
        finally
        {
            File.Delete(temp);
        }
    }
}
