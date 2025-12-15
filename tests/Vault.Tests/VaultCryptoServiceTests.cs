using Vault.Application.Abstractions;
using Vault.Application.Models;
using Vault.Application.Services;
using Vault.Crypto;
using Vault.Storage.Serialization;
using Xunit;
using System.Security.Cryptography;

namespace Vault.Tests;

public class VaultCryptoServiceTests
{
    [Fact]
    public void CreateAndUnlockVault_Roundtrip_Works()
    {
        var crypto = new CryptoProvider();
        var serializer = new JsonVaultPayloadSerializer();
        var service = new VaultCryptoService(crypto, serializer);

        var doc = VaultDocument.CreateNew("Personal");
        var password = "super-secret".AsSpan();

        var file = service.CreateVault(doc, password, KdfProfile.Interactive);
        var unlocked = service.UnlockVault(file, password);

        Assert.Equal(doc.Meta.VaultName, unlocked.Meta.VaultName);
        Assert.Equal(doc.Entries.Count, unlocked.Entries.Count);
    }

    [Fact]
    public void UnlockVault_WrongPassword_Fails()
    {
        var crypto = new CryptoProvider();
        var serializer = new JsonVaultPayloadSerializer();
        var service = new VaultCryptoService(crypto, serializer);

        var doc = VaultDocument.CreateNew("Vault");
        var file = service.CreateVault(doc, "correct".AsSpan(), KdfProfile.Interactive);

        Assert.ThrowsAny<CryptographicException>(() =>
            service.UnlockVault(file, "wrong".AsSpan()));
    }
}
