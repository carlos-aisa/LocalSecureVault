using System;
using System.Collections.Generic;
using Vault.Application.Models;
using Vault.Domain;
using Xunit;

namespace Vault.Tests;

public class VaultDocumentEdgeCasesTests
{
    [Fact]
    public void GetEntry_WithValidId_ReturnsEntry()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var entry = VaultEntry.CreateNew("GitHub", "pw");
        doc.AddEntry(entry);

        var found = doc.GetEntry(entry.Id);

        Assert.NotNull(found);
        Assert.Equal(entry.Id, found.Id);
        Assert.Equal("GitHub", found.Name);
    }

    [Fact]
    public void GetEntry_WithInvalidId_ThrowsKeyNotFoundException()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var entry = VaultEntry.CreateNew("GitHub", "pw");
        doc.AddEntry(entry);

        Assert.Throws<KeyNotFoundException>(() => doc.GetEntry(Guid.NewGuid()));
    }

    [Fact]
    public void GetEntry_OnEmptyDocument_ThrowsKeyNotFoundException()
    {
        var doc = VaultDocument.CreateNew("Vault");

        Assert.Throws<KeyNotFoundException>(() => doc.GetEntry(Guid.NewGuid()));
    }

    [Fact]
    public void AddEntry_MultipleEntries_MaintainsOrder()
    {
        var doc = VaultDocument.CreateNew("Vault");
        
        var e1 = VaultEntry.CreateNew("First", "pw1");
        var e2 = VaultEntry.CreateNew("Second", "pw2");
        var e3 = VaultEntry.CreateNew("Third", "pw3");
        
        doc.AddEntry(e1);
        doc.AddEntry(e2);
        doc.AddEntry(e3);

        Assert.Equal(3, doc.Entries.Count);
        Assert.Equal("First", doc.Entries[0].Name);
        Assert.Equal("Second", doc.Entries[1].Name);
        Assert.Equal("Third", doc.Entries[2].Name);
    }

    [Fact]
    public void AddEntry_NullEntry_ThrowsArgumentNullException()
    {
        var doc = VaultDocument.CreateNew("Vault");

        Assert.Throws<ArgumentNullException>(() => doc.AddEntry(null!));
    }

    [Fact]
    public void RemoveEntry_FromMiddle_RemovesCorrectEntry()
    {
        var doc = VaultDocument.CreateNew("Vault");
        
        var e1 = VaultEntry.CreateNew("First", "pw1");
        var e2 = VaultEntry.CreateNew("Second", "pw2");
        var e3 = VaultEntry.CreateNew("Third", "pw3");
        
        doc.AddEntry(e1);
        doc.AddEntry(e2);
        doc.AddEntry(e3);

        var removed = doc.RemoveEntry(e2.Id);

        Assert.True(removed);
        Assert.Equal(2, doc.Entries.Count);
        Assert.Equal("First", doc.Entries[0].Name);
        Assert.Equal("Third", doc.Entries[1].Name);
    }

    [Fact]
    public void Touch_UpdatesTimestamp()
    {
        var t1 = new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero);
        var t2 = t1.AddMinutes(10);

        var doc = VaultDocument.CreateNew("Vault", nowUtc: t1);
        
        Assert.Equal(t1, doc.Meta.UpdatedUtc);
        
        doc.Touch(nowUtc: t2);
        
        Assert.Equal(t2, doc.Meta.UpdatedUtc);
        Assert.Equal(t1, doc.Meta.CreatedUtc); // CreatedUtc should not change
    }

    [Fact]
    public void CreateNew_WithWhitespace_TrimsVaultName()
    {
        var doc = VaultDocument.CreateNew("  Personal Vault  ");

        Assert.Equal("Personal Vault", doc.Meta.VaultName);
    }

    [Fact]
    public void Entries_IsReadOnly_CannotModifyDirectly()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var entry = VaultEntry.CreateNew("GitHub", "pw");
        doc.AddEntry(entry);

        // Should be IReadOnlyList, not a mutable list
        Assert.IsAssignableFrom<IReadOnlyList<VaultEntry>>(doc.Entries);
    }
}
