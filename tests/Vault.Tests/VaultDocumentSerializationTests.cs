using Vault.Application.Models;
using Vault.Application.UseCases;
using Vault.Storage.Serialization;
using Xunit;

namespace Vault.Tests;

public class VaultDocumentSerializationTests
{
    [Fact]
    public void SerializeDeserialize_Roundtrip_PreservesData()
    {
        var t1 = new DateTimeOffset(2025, 12, 15, 10, 0, 0, TimeSpan.Zero);
        var t2 = t1.AddMinutes(1);

        var doc = VaultDocument.CreateNew("Personal", nowUtc: t1);
        var uc = new EntryUseCases();

        var e1 = uc.AddEntry(doc, "GitHub", "pw1", username: "carlos", url: "https://github.com", notes: "work", tags: new[] { "Dev", " " }, nowUtc: t2);
        var e2 = uc.AddEntry(doc, "Bank", "pw2", username: "me", url: "https://bank.local", notes: "money", tags: new[] { "Finance" }, nowUtc: t2);

        var serializer = new VaultDocumentSerializer();

        var bytes = serializer.SerializeToUtf8(doc);
        var doc2 = serializer.DeserializeFromUtf8(bytes);

        Assert.Equal(doc.Meta.VaultName, doc2.Meta.VaultName);
        Assert.Equal(doc.Meta.SchemaVersion, doc2.Meta.SchemaVersion);
        Assert.Equal(doc.Meta.CreatedUtc, doc2.Meta.CreatedUtc);
        Assert.Equal(doc.Meta.UpdatedUtc, doc2.Meta.UpdatedUtc);

        Assert.Equal(2, doc2.Entries.Count);

        Assert.Equal(e1.Id, doc2.Entries[0].Id);
        Assert.Equal("GitHub", doc2.Entries[0].Name);
        Assert.Equal("pw1", doc2.Entries[0].Password);
        Assert.Equal("carlos", doc2.Entries[0].Username);
        Assert.Equal("https://github.com", doc2.Entries[0].Url);

        Assert.Equal(e2.Id, doc2.Entries[1].Id);
        Assert.Equal("Bank", doc2.Entries[1].Name);
        Assert.Equal("pw2", doc2.Entries[1].Password);
    }
}
