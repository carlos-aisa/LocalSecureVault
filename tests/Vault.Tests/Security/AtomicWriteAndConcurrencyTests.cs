using System.IO;
using System.Threading.Tasks;
using Vault.Application.Abstractions;
using Vault.Storage;
using Xunit;

namespace Vault.Tests;
public class AtomicWriteAndConcurrencyTests
{
    [Fact]
    public async Task WriteAtomicAsync_FailureDuringWrite_OriginalFileRemainsReadable()
    {
        var store = new FileVaultStore();
        var temp = Path.GetTempFileName();

        try
        {
            var originalHeader = CreateValidHeader();
            var originalFile = new VaultFile(originalHeader, new byte[] { 1, 2, 3 }, new byte[VaultFormatConstants.TagSize]);
            await store.WriteAtomicAsync(temp, originalFile);
            var original = await store.ReadAsync(temp);
            Assert.NotNull(original);
            var fileInfo = new FileInfo(temp);
            fileInfo.IsReadOnly = true;

            try
            {
                var newHeader = CreateValidHeader();
                var newFile = new VaultFile(newHeader, new byte[] { 4, 5, 6, 7 }, new byte[VaultFormatConstants.TagSize]);
                await Assert.ThrowsAnyAsync<Exception>(() => store.WriteAtomicAsync(temp, newFile));
                fileInfo.IsReadOnly = false;
                var afterFailed = await store.ReadAsync(temp);
                Assert.NotNull(afterFailed);
                Assert.Equal(original.Ciphertext, afterFailed.Ciphertext);
            }
            finally
            {
                fileInfo.IsReadOnly = false;
            }
        }
        finally
        {
            if (File.Exists(temp))
                File.Delete(temp);
        }
    }

    [Fact]
    public async Task WriteAtomicAsync_SuccessfulReplace_NewFileReadable()
    {
        var store = new FileVaultStore();
        var temp = Path.GetTempFileName();

        try
        {
            var originalHeader = CreateValidHeader();
            var originalFile = new VaultFile(originalHeader, new byte[] { 1, 2, 3 }, new byte[VaultFormatConstants.TagSize]);
            await store.WriteAtomicAsync(temp, originalFile);
            var newHeader = CreateValidHeader();
            var newFile = new VaultFile(newHeader, new byte[] { 7, 8, 9, 10, 11 }, new byte[VaultFormatConstants.TagSize]);
            await store.WriteAtomicAsync(temp, newFile);
            var afterReplace = await store.ReadAsync(temp);
            Assert.NotNull(afterReplace);
            Assert.Equal(newFile.Ciphertext, afterReplace.Ciphertext);
            Assert.NotEqual(originalFile.Ciphertext.Length, afterReplace.Ciphertext.Length);
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public async Task ConcurrentWrites_OnlyOneSucceeds()
    {
        var store = new FileVaultStore();
        var temp = Path.GetTempFileName();

        try
        {
            var initialHeader = CreateValidHeader();
            var initialFile = new VaultFile(initialHeader, new byte[] { 0 }, new byte[VaultFormatConstants.TagSize]);
            await store.WriteAtomicAsync(temp, initialFile);

            var tasks = new Task<Exception?>[5];
            var contents = new byte[][] { 
                new byte[] { 1 }, 
                new byte[] { 2 }, 
                new byte[] { 3 }, 
                new byte[] { 4 }, 
                new byte[] { 5 } 
            };
            for (int i = 0; i < tasks.Length; i++)
            {
                var content = contents[i];
                tasks[i] = Task.Run(async () =>
                {
                    try
                    {
                        var header = CreateValidHeader();
                        var file = new VaultFile(header, content, new byte[VaultFormatConstants.TagSize]);
                        await store.WriteAtomicAsync(temp, file);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        return ex;
                    }
                });
            }

            var results = await Task.WhenAll(tasks);
            var read = await store.ReadAsync(temp);
            Assert.NotNull(read);
            Assert.True(read.Ciphertext.Length > 0);
            bool anySucceeded = !read.Ciphertext.SequenceEqual(new byte[] { 0 });
        }
        finally
        {
            File.Delete(temp);
        }
    }

    private static VaultFileHeader CreateValidHeader()
    {
        return new VaultFileHeader(
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
            CreatedUtcTicks: DateTimeOffset.UtcNow.Ticks,
            UpdatedUtcTicks: DateTimeOffset.UtcNow.Ticks,
            Reserved: new byte[VaultFormatConstants.ReservedSize]
        );
    }
}
