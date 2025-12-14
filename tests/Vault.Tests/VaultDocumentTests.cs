using Vault.Application.Models;
using Vault.Domain;
using Xunit;

namespace Vault.Tests;

public class VaultDocumentTests
{
    [Fact]
    public void CreateNew_CreatesEmptyDocumentWithMetadata()
    {
        var now = new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero);

        var doc = VaultDocument.CreateNew("  Personal Vault  ", nowUtc: now);

        Assert.Equal("Personal Vault", doc.Meta.VaultName);
        Assert.Equal(1, doc.Meta.SchemaVersion);
        Assert.Equal(now, doc.Meta.CreatedUtc);
        Assert.Equal(now, doc.Meta.UpdatedUtc);
        Assert.Empty(doc.Entries);
    }

    [Fact]
    public void AddEntry_TouchesUpdatedUtc()
    {
        var t1 = new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero);
        var t2 = t1.AddMinutes(5);

        var doc = VaultDocument.CreateNew("Vault", nowUtc: t1);
        var entry = VaultEntry.CreateNew("GitHub", "pw", nowUtc: t1);

        doc.AddEntry(entry, nowUtc: t2);

        Assert.Single(doc.Entries);
        Assert.Equal(t2, doc.Meta.UpdatedUtc);
    }

    [Fact]
    public void RemoveEntry_TouchesUpdatedUtc_WhenRemoved()
    {
        var t1 = new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero);
        var t2 = t1.AddMinutes(5);

        var doc = VaultDocument.CreateNew("Vault", nowUtc: t1);
        var entry = VaultEntry.CreateNew("GitHub", "pw", nowUtc: t1);
        doc.AddEntry(entry, nowUtc: t1);

        var removed = doc.RemoveEntry(entry.Id, nowUtc: t2);

        Assert.True(removed);
        Assert.Empty(doc.Entries);
        Assert.Equal(t2, doc.Meta.UpdatedUtc);
    }

    [Fact]
    public void RemoveEntry_DoesNotTouchUpdatedUtc_WhenNotFound()
    {
        var t1 = new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero);

        var doc = VaultDocument.CreateNew("Vault", nowUtc: t1);
        var removed = doc.RemoveEntry(Guid.NewGuid(), nowUtc: t1.AddMinutes(1));

        Assert.False(removed);
        Assert.Equal(t1, doc.Meta.UpdatedUtc);
    }
}
