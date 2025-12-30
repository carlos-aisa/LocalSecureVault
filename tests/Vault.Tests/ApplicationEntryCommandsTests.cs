using Vault.Application.Models;
using Vault.Application.UseCases;
using Xunit;

namespace Vault.Tests;

public class ApplicationEntryCommandsTests
{
    [Fact]
    public void AddEntry_AddsEntryAndTouchesDocument()
    {
        var t1 = new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero);
        var t2 = t1.AddMinutes(1);

        var doc = VaultDocument.CreateNew("Vault", nowUtc: t1);
        // Using TestHelpers

        var entry = TestHelpers.AddEntry(doc, "GitHub", "pw", nowUtc: t2);

        Assert.Single(doc.Entries);
        Assert.Equal(entry.Id, doc.Entries[0].Id);
        Assert.Equal(t2, doc.Meta.UpdatedUtc);
    }

    [Fact]
    public void UpdateEntry_UpdatesEntryAndTouchesDocument()
    {
        var t1 = new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero);
        var t2 = t1.AddMinutes(5);

        var doc = VaultDocument.CreateNew("Vault", nowUtc: t1);
        // Using TestHelpers
        var entry = TestHelpers.AddEntry(doc, "A", "P", nowUtc: t1);

        TestHelpers.UpdateEntry(doc, entry.Id, "B", "Q", nowUtc: t2);

        Assert.Equal("B", entry.Name);
        Assert.Equal("Q", entry.Password);
        Assert.Equal(t2, doc.Meta.UpdatedUtc);
    }

    [Fact]
    public void DeleteEntry_RemovesEntryAndTouchesDocument()
    {
        var t1 = new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero);
        var t2 = t1.AddMinutes(3);

        var doc = VaultDocument.CreateNew("Vault", nowUtc: t1);
        // Using TestHelpers
        var entry = TestHelpers.AddEntry(doc, "A", "P", nowUtc: t1);

        var removed = TestHelpers.DeleteEntry(doc, entry.Id, nowUtc: t2);

        Assert.True(removed);
        Assert.Empty(doc.Entries);
        Assert.Equal(t2, doc.Meta.UpdatedUtc);
    }
}
