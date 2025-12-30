using System.IO;
using Vault.Application.Abstractions;
using Vault.Storage;
using Xunit;

namespace Vault.Tests;

public class FileVaultStoreTests
{
    [Fact]
    public async Task WriteAndRead_Roundtrip_Works()
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

            var ciphertext = new byte[] { 1, 2, 3, 4, 5 };
            var tag = new byte[VaultFormatConstants.TagSize];

            var file = new VaultFile(header, ciphertext, tag);

            await store.WriteAtomicAsync(temp, file);
            var read = await store.ReadAsync(temp);

            Assert.Equal(file.Ciphertext, read.Ciphertext);
            Assert.Equal(file.Tag, read.Tag);
            Assert.Equal(file.Header.Magic, read.Header.Magic);
            Assert.Equal(file.Header.SchemaVersion, read.Header.SchemaVersion);
        }
        finally
        {
            File.Delete(temp);
        }
    }
}
