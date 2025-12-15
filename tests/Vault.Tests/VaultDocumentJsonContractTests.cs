using System.Text;
using Vault.Application.Models;
using Vault.Application.UseCases;
using Vault.Storage.Serialization;
using Xunit;

namespace Vault.Tests;

public class VaultDocumentJsonContractTests
{
    [Fact]
    public void SerializeToString_ContainsExpectedRootFields()
    {
        var t1 = new DateTimeOffset(2025, 12, 15, 10, 0, 0, TimeSpan.Zero);

        var doc = VaultDocument.CreateNew("Personal", nowUtc: t1);
        var uc = new EntryCommands();
        uc.AddEntry(doc, "GitHub", "pw", username: "carlos", url: "https://github.com", tags: new[] { "dev" }, nowUtc: t1);

        var serializer = new VaultDocumentSerializer();
        var json = serializer.SerializeToString(doc);

        // Root objects
        Assert.Contains("\"meta\"", json);
        Assert.Contains("\"entries\"", json);

        // Expected metadata keys
        Assert.Contains("\"vaultName\"", json);
        Assert.Contains("\"schemaVersion\"", json);
        Assert.Contains("\"createdUtc\"", json);
        Assert.Contains("\"updatedUtc\"", json);

        // Expected entry keys (at least one entry)
        Assert.Contains("\"id\"", json);
        Assert.Contains("\"name\"", json);
        Assert.Contains("\"password\"", json);
        Assert.Contains("\"username\"", json);
        Assert.Contains("\"url\"", json);
        Assert.Contains("\"tags\"", json);
    }

    [Fact]
    public void DeserializeFromUtf8_InvalidJson_Throws()
    {
        var serializer = new VaultDocumentSerializer();

        // Not valid JSON
        var bad = Encoding.UTF8.GetBytes("{ this is not json }");

        Assert.ThrowsAny<Exception>(() => serializer.DeserializeFromUtf8(bad));
    }
}
