using System.IO;
using Vault.Application.Abstractions;
using Vault.Storage;
using Xunit;

namespace Vault.Tests;

/// <summary>
/// Tests for file format validation - magic bytes, version checks, and header integrity
/// </summary>
public class FileFormatValidationTests
{
    [Fact]
    public async Task ReadAsync_WrongMagic_ReadsButShouldBeRejectedByApplication()
    {
        var store = new FileVaultStore();
        var temp = Path.GetTempFileName();

        try
        {
            // Create a file with wrong magic bytes
            var header = new VaultFileHeader(
                Magic: "BAD!",  // Wrong magic
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

            // Read the file - FileVaultStore doesn't validate, just deserializes
            var read = await store.ReadAsync(temp);
            
            // Application layer should reject wrong magic
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
            // Create a file with unsupported version
            var header = new VaultFileHeader(
                Magic: VaultFormatConstants.Magic,
                Version: 999,  // Unsupported version
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

            // Read the file - FileVaultStore doesn't validate version
            var read = await store.ReadAsync(temp);
            
            // Application layer should reject unsupported version
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
            // Write a file that's shorter than the required header size
            await File.WriteAllBytesAsync(temp, new byte[50]); // Only 50 bytes, need 82 + tag

            var store = new FileVaultStore();
            
            // Should throw because file is too short
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
            // Create a valid file
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

            // Should succeed
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
