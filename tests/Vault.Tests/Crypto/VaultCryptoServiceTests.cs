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
        var unlocked = service.UnlockVault(file.File, password);

        Assert.Equal(doc.Meta.VaultName, unlocked.Document.Meta.VaultName);
        Assert.Equal(doc.Entries.Count, unlocked.Document.Entries.Count);
    }

    [Fact]
    public void UnlockVault_WrongPassword_Fails()
    {
        var crypto = new CryptoProvider();
        var serializer = new JsonVaultPayloadSerializer();
        var service = new VaultCryptoService(crypto, serializer);

        var now = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var doc = VaultDocument.CreateNew("test", now);
        var file = service.CreateVault(doc, "correct-password".AsSpan(), KdfProfile.Interactive, now);

        Assert.ThrowsAny<CryptographicException>(() =>
            service.UnlockVault(file.File, "wrong".AsSpan()));
    }

    [Fact]
    public void UnlockVault_CorrectPassword_ReturnsDocumentAndSessionKey()
    {
        var now = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var doc = VaultDocument.CreateNew("test", now);

        var cryptoProvider = new CryptoProvider();
        var serializer = new JsonVaultPayloadSerializer(); // ajusta si difiere
        var svc = new VaultCryptoService(cryptoProvider, serializer);

        var create = svc.CreateVault(doc, "correct-password".AsSpan(), KdfProfile.Interactive, now);

        var unlock = svc.UnlockVault(create.File, "correct-password");

        Assert.NotNull(unlock.Document);
        Assert.NotNull(unlock.SessionKey);
        Assert.True(unlock.SessionKey.Length > 0);

        Assert.Equal(doc.Meta.VaultName, unlock.Document.Meta.VaultName);
    }
}
