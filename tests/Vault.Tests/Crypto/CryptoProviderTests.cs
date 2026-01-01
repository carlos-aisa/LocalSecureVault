using System.Security.Cryptography;
using Vault.Application.Abstractions;
using Vault.Crypto;
using Xunit;

namespace Vault.Tests;

public class CryptoProviderTests
{
    [Fact]
    public void DeriveKey_SameInputs_SameKey()
    {
        var crypto = new CryptoProvider();
        var salt = RandomNumberGenerator.GetBytes(VaultFormatConstants.SaltSize);
        var p = new KdfParams(256 * 1024, 3, 2, 32);

        var k1 = crypto.DeriveKey("password".AsSpan(), salt, p);
        var k2 = crypto.DeriveKey("password".AsSpan(), salt, p);

        Assert.Equal(k1, k2);
        Assert.Equal(32, k1.Length);
    }

    [Fact]
    public void DeriveKey_DifferentSalt_DifferentKey()
    {
        var crypto = new CryptoProvider();
        var salt1 = RandomNumberGenerator.GetBytes(VaultFormatConstants.SaltSize);
        var salt2 = RandomNumberGenerator.GetBytes(VaultFormatConstants.SaltSize);
        var p = new KdfParams(256 * 1024, 3, 2, 32);

        var k1 = crypto.DeriveKey("password".AsSpan(), salt1, p);
        var k2 = crypto.DeriveKey("password".AsSpan(), salt2, p);

        Assert.NotEqual(k1, k2);
    }

    [Fact]
    public void EncryptDecrypt_Roundtrip_Works()
    {
        var crypto = new CryptoProvider();
        var key = RandomNumberGenerator.GetBytes(32);

        var aad = new byte[] { 1, 2, 3, 4, 5 };
        var plaintext = new byte[] { 10, 20, 30, 40, 50 };

        var blob = crypto.Encrypt(plaintext, aad, key);
        var decrypted = crypto.Decrypt(blob, aad, key);

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void Decrypt_WrongKey_Fails()
    {
        var crypto = new CryptoProvider();
        var key = RandomNumberGenerator.GetBytes(32);
        var wrongKey = RandomNumberGenerator.GetBytes(32);

        var aad = new byte[] { 9, 9, 9 };
        var plaintext = new byte[] { 1, 2, 3 };

        var blob = crypto.Encrypt(plaintext, aad, key);

        Assert.ThrowsAny<CryptographicException>(() => crypto.Decrypt(blob, aad, wrongKey));
    }

    [Fact]
    public void Decrypt_TamperedCiphertext_Fails()
    {
        var crypto = new CryptoProvider();
        var key = RandomNumberGenerator.GetBytes(32);
        var aad = new byte[] { 7, 7, 7 };
        var plaintext = new byte[] { 1, 2, 3, 4, 5 };

        var blob = crypto.Encrypt(plaintext, aad, key);

        blob.Ciphertext[0] ^= 0xFF; // tamper

        Assert.ThrowsAny<CryptographicException>(() => crypto.Decrypt(blob, aad, key));
    }

    [Fact]
    public void Decrypt_TamperedAad_Fails()
    {
        var crypto = new CryptoProvider();
        var key = RandomNumberGenerator.GetBytes(32);
        var aad = new byte[] { 1, 2, 3 };
        var plaintext = new byte[] { 8, 8, 8 };

        var blob = crypto.Encrypt(plaintext, aad, key);

        var tamperedAad = new byte[] { 1, 2, 4 };

        Assert.ThrowsAny<CryptographicException>(() => crypto.Decrypt(blob, tamperedAad, key));
    }

    [Fact]
    public void Encrypt_GeneratesNewNonceEachTime()
    {
        var crypto = new CryptoProvider();
        var key = RandomNumberGenerator.GetBytes(32);
        var aad = new byte[] { 1, 2, 3 };
        var plaintext = new byte[] { 9, 9, 9 };

        var b1 = crypto.Encrypt(plaintext, aad, key);
        var b2 = crypto.Encrypt(plaintext, aad, key);

        Assert.NotEqual(b1.Nonce, b2.Nonce);
    }

    [Fact]
    public void Decrypt_TamperedTag_Fails()
    {
        var crypto = new CryptoProvider();
        var key = RandomNumberGenerator.GetBytes(32);
        var aad = new byte[] { 5, 5, 5 };
        var plaintext = new byte[] { 1, 2, 3, 4, 5 };

        var blob = crypto.Encrypt(plaintext, aad, key);
        blob.Tag[0] ^= 0xFF;

        Assert.ThrowsAny<CryptographicException>(() => crypto.Decrypt(blob, aad, key));
    }
}
