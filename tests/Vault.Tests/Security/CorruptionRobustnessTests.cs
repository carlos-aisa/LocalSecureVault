using System;
using System.IO;
using System.Security.Cryptography;
using Vault.Application.Abstractions;
using Vault.Application.Models;
using Vault.Application.Services;
using Vault.Crypto;
using Vault.Storage;
using Vault.Storage.Serialization;
using Xunit;

namespace Vault.Tests;

/// <summary>
/// Robustness tests - handling corrupted data safely without crashes
/// </summary>
public class CorruptionRobustnessTests
{
    [Fact]
    public async Task UnlockVault_RandomlyCorruptedFile_FailsSafely()
    {
        var store = new FileVaultStore();
        var cryptoProvider = new CryptoProvider();
        var serializer = new JsonVaultPayloadSerializer();
        var cryptoService = new VaultCryptoService(cryptoProvider, serializer);
        
        var temp = Path.GetTempFileName();

        try
        {
            // Create a valid vault
            var doc = VaultDocument.CreateNew("TestVault");
            var password = "test-password";
            var created = cryptoService.CreateVault(doc, password.AsSpan(), KdfProfile.Interactive);
            
            await store.WriteAtomicAsync(temp, created.File);

            // Read the file and corrupt random bytes
            var originalBytes = await File.ReadAllBytesAsync(temp);
            var corruptedBytes = new byte[originalBytes.Length];
            originalBytes.CopyTo(corruptedBytes, 0);

            // Corrupt 5 random bytes in the file
            var random = new Random(42); // Fixed seed for reproducibility
            for (int i = 0; i < 5; i++)
            {
                int pos = random.Next(0, corruptedBytes.Length);
                corruptedBytes[pos] ^= 0xFF;
            }

            await File.WriteAllBytesAsync(temp, corruptedBytes);

            // Attempt to read and unlock should fail gracefully (no crash)
            var exception = await Record.ExceptionAsync(async () =>
            {
                var file = await store.ReadAsync(temp);
                cryptoService.UnlockVault(file, password.AsSpan());
            });

            // Should throw an exception, but not crash
            Assert.NotNull(exception);
            // Should be either InvalidOperationException or CryptographicException
            Assert.True(
                exception is InvalidOperationException || 
                exception is CryptographicException ||
                exception is ArgumentException,
                $"Expected InvalidOperationException or CryptographicException, got {exception.GetType().Name}");
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public async Task UnlockVault_CorruptedCiphertext_FailsSafely()
    {
        var store = new FileVaultStore();
        var cryptoProvider = new CryptoProvider();
        var serializer = new JsonVaultPayloadSerializer();
        var cryptoService = new VaultCryptoService(cryptoProvider, serializer);
        
        var temp = Path.GetTempFileName();

        try
        {
            // Create a valid vault
            var doc = VaultDocument.CreateNew("TestVault");
            var password = "test-password";
            var created = cryptoService.CreateVault(doc, password.AsSpan(), KdfProfile.Interactive);
            
            await store.WriteAtomicAsync(temp, created.File);

            // Read and corrupt just the ciphertext portion
            var original = await store.ReadAsync(temp);
            var corruptedCiphertext = new byte[original.Ciphertext.Length];
            original.Ciphertext.CopyTo(corruptedCiphertext, 0);
            
            // Flip bits in ciphertext
            if (corruptedCiphertext.Length > 0)
            {
                corruptedCiphertext[corruptedCiphertext.Length / 2] ^= 0xFF;
            }

            var corruptedFile = new VaultFile(original.Header, corruptedCiphertext, original.Tag);
            await store.WriteAtomicAsync(temp, corruptedFile);

            // Should fail with CryptographicException
            var file = await store.ReadAsync(temp);
            Assert.ThrowsAny<CryptographicException>(() =>
                cryptoService.UnlockVault(file, password.AsSpan()));
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public async Task UnlockVault_CorruptedJsonPayload_FailsSafely()
    {
        var store = new FileVaultStore();
        var cryptoProvider = new CryptoProvider();
        var temp = Path.GetTempFileName();

        try
        {
            // Create a vault file with valid encryption but invalid JSON inside
            var password = "test-password";
            var salt = RandomNumberGenerator.GetBytes(VaultFormatConstants.SaltSize);
            var kdfParams = new KdfParams(64 * 1024, 2, 2, 32);
            var key = cryptoProvider.DeriveKey(password.AsSpan(), salt, kdfParams);

            // Create invalid JSON payload
            var invalidJson = System.Text.Encoding.UTF8.GetBytes("{ this is not valid json }");
            
            var header = new VaultFileHeader(
                Magic: VaultFormatConstants.Magic,
                Version: VaultFormatConstants.Version,
                Flags: 0,
                KdfId: VaultFormatConstants.KdfIdArgon2id,
                PayloadEncoding: VaultFormatConstants.PayloadJsonUtf8,
                SchemaVersion: 1,
                Argon2MemoryKiB: kdfParams.MemoryKiB,
                Argon2Iterations: kdfParams.Iterations,
                Argon2Parallelism: kdfParams.Parallelism,
                Salt: salt,
                Nonce: RandomNumberGenerator.GetBytes(VaultFormatConstants.NonceSize),
                CreatedUtcTicks: DateTimeOffset.UtcNow.Ticks,
                UpdatedUtcTicks: DateTimeOffset.UtcNow.Ticks,
                Reserved: new byte[VaultFormatConstants.ReservedSize]
            );

            // Encrypt the invalid JSON
            var aad = new byte[VaultFormatConstants.HeaderSizeV1];
            var blob = cryptoProvider.Encrypt(invalidJson, aad, key);

            var file = new VaultFile(header, blob.Ciphertext, blob.Tag);
            await store.WriteAtomicAsync(temp, file);

            // Unlock should fail gracefully when trying to parse JSON
            var readFile = await store.ReadAsync(temp);
            
            var serializer = new JsonVaultPayloadSerializer();
            var cryptoService = new VaultCryptoService(cryptoProvider, serializer);

            var exception = Record.Exception(() =>
                cryptoService.UnlockVault(readFile, password.AsSpan()));

            Assert.NotNull(exception);
            // Should handle JSON parse failure gracefully
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public async Task ReadAsync_TruncatedFile_ThrowsInvalidDataException()
    {
        var store = new FileVaultStore();
        var temp = Path.GetTempFileName();

        try
        {
            // Create a valid vault
            var cryptoProvider = new CryptoProvider();
            var serializer = new JsonVaultPayloadSerializer();
            var cryptoService = new VaultCryptoService(cryptoProvider, serializer);
            
            var doc = VaultDocument.CreateNew("TestVault");
            var created = cryptoService.CreateVault(doc, "password".AsSpan(), KdfProfile.Interactive);
            
            await store.WriteAtomicAsync(temp, created.File);

            // Truncate the file to less than header + tag size
            var bytes = await File.ReadAllBytesAsync(temp);
            var truncated = bytes[..30]; // Less than header size (82 bytes)
            await File.WriteAllBytesAsync(temp, truncated);

            // Should fail with InvalidDataException
            await Assert.ThrowsAsync<InvalidDataException>(() => store.ReadAsync(temp));
        }
        finally
        {
            File.Delete(temp);
        }
    }
}
