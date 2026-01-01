using System;
using System.Collections.Generic;
using Vault.Application.Models;
using Vault.Application.UseCases;
using Vault.Domain;
using Xunit;

namespace Vault.Tests;

public class EntryUseCasesEdgeCasesTests
{
    [Fact]
    public void AddEntry_WithNullDocument_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => 
            TestHelpers.AddEntry(null!, "GitHub", "pw"));
    }

    [Fact]
    public void AddEntry_ReturnsCreatedEntry()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var entry = TestHelpers.AddEntry(doc, "GitHub", "pw", username: "carlos");

        Assert.NotNull(entry);
        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal("GitHub", entry.Name);
        Assert.Equal("pw", entry.Password);
        Assert.Equal("carlos", entry.Username);
    }

    [Fact]
    public void AddEntry_WithAllOptionalFields_CreatesFullEntry()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var now = new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero);

        var entry = TestHelpers.AddEntry(
            doc, 
            "GitHub", 
            "pw123",
            username: "carlos",
            url: "https://github.com",
            notes: "Work account",
            tags: new[] { "dev", "work" },
            nowUtc: now);

        Assert.Equal("GitHub", entry.Name);
        Assert.Equal("pw123", entry.Password);
        Assert.Equal("carlos", entry.Username);
        Assert.Equal("https://github.com", entry.Url);
        Assert.Equal("Work account", entry.Notes);
        Assert.Equal(2, entry.Tags.Count);
        Assert.Equal(now, entry.CreatedUtc);
    }

    [Fact]
    public void UpdateEntry_WithNullDocument_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => 
            TestHelpers.UpdateEntry(null!, Guid.NewGuid(), "GitHub", "pw"));
    }

    [Fact]
    public void UpdateEntry_WithNonExistentId_ThrowsKeyNotFoundException()
    {
        var doc = VaultDocument.CreateNew("Vault");

        Assert.Throws<KeyNotFoundException>(() =>
            TestHelpers.UpdateEntry(doc, Guid.NewGuid(), "GitHub", "pw"));
    }

    [Fact]
    public void UpdateEntry_ModifiesExistingEntry()
    {
        var t1 = new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero);
        var t2 = t1.AddMinutes(10);

        var doc = VaultDocument.CreateNew("Vault", nowUtc: t1);
        var entry = TestHelpers.AddEntry(doc, "GitHub", "oldpw", nowUtc: t1);
        var originalId = entry.Id;
        var originalCreated = entry.CreatedUtc;

        TestHelpers.UpdateEntry(
            doc, 
            entry.Id, 
            "GitHub Updated", 
            "newpw",
            username: "newuser",
            nowUtc: t2);

        Assert.Equal(originalId, entry.Id);
        Assert.Equal("GitHub Updated", entry.Name);
        Assert.Equal("newpw", entry.Password);
        Assert.Equal("newuser", entry.Username);
        Assert.Equal(originalCreated, entry.CreatedUtc);
        Assert.Equal(t2, entry.UpdatedUtc);
        Assert.Equal(t2, doc.Meta.UpdatedUtc);
    }

    [Fact]
    public void DeleteEntry_WithNullDocument_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => 
            TestHelpers.DeleteEntry(null!, Guid.NewGuid()));
    }

    [Fact]
    public void DeleteEntry_WithNonExistentId_ReturnsFalse()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var result = TestHelpers.DeleteEntry(doc, Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public void DeleteEntry_RemovesEntryAndReturnsTrue()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var entry = TestHelpers.AddEntry(doc, "GitHub", "pw");
        Assert.Single(doc.Entries);

        var result = TestHelpers.DeleteEntry(doc, entry.Id);

        Assert.True(result);
        Assert.Empty(doc.Entries);
    }

    [Fact]
    public void DeleteEntry_TouchesDocumentTimestamp()
    {
        var t1 = new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero);
        var t2 = t1.AddMinutes(5);

        var doc = VaultDocument.CreateNew("Vault", nowUtc: t1);
        var entry = TestHelpers.AddEntry(doc, "GitHub", "pw", nowUtc: t1);

        TestHelpers.DeleteEntry(doc, entry.Id, nowUtc: t2);

        Assert.Equal(t2, doc.Meta.UpdatedUtc);
    }

    [Fact]
    public void AddEntry_MultipleEntries_AllAddedSuccessfully()
    {
        var doc = VaultDocument.CreateNew("Vault");

        var e1 = TestHelpers.AddEntry(doc, "GitHub", "pw1");
        var e2 = TestHelpers.AddEntry(doc, "GitLab", "pw2");
        var e3 = TestHelpers.AddEntry(doc, "Bitbucket", "pw3");

        Assert.Equal(3, doc.Entries.Count);
        Assert.Contains(doc.Entries, e => e.Id == e1.Id);
        Assert.Contains(doc.Entries, e => e.Id == e2.Id);
        Assert.Contains(doc.Entries, e => e.Id == e3.Id);
    }

    [Fact]
    public void UpdateEntry_CanClearOptionalFields()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var entry = TestHelpers.AddEntry(
            doc, 
            "GitHub", 
            "pw",
            username: "carlos",
            url: "https://github.com",
            notes: "Work",
            tags: new[] { "dev" });

        TestHelpers.UpdateEntry(
            doc,
            entry.Id,
            "GitHub",
            "pw",
            username: null,
            url: null,
            notes: null,
            tags: null);

        Assert.Null(entry.Username);
        Assert.Null(entry.Url);
        Assert.Null(entry.Notes);
        Assert.Empty(entry.Tags);
    }
}
