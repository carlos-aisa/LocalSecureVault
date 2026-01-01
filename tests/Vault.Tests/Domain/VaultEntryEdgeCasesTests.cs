using System;
using System.Linq;
using Vault.Domain;
using Xunit;

namespace Vault.Tests;

public class VaultEntryEdgeCasesTests
{
    [Fact]
    public void CreateNew_WithNullOptionalFields_SetsThemToNull()
    {
        var entry = VaultEntry.CreateNew("GitHub", "pw", username: null, url: null, notes: null, tags: null);

        Assert.Equal("GitHub", entry.Name);
        Assert.Equal("pw", entry.Password);
        Assert.Null(entry.Username);
        Assert.Null(entry.Url);
        Assert.Null(entry.Notes);
        Assert.Empty(entry.Tags);
    }

    [Fact]
    public void CreateNew_WithEmptyOptionalFields_NormalizesToNull()
    {
        var entry = VaultEntry.CreateNew("GitHub", "pw", username: "", url: "  ", notes: "   ");

        Assert.Null(entry.Username);
        Assert.Null(entry.Url);
        Assert.Null(entry.Notes);
    }

    [Fact]
    public void CreateNew_WithEmptyTags_FiltersThemOut()
    {
        var entry = VaultEntry.CreateNew("GitHub", "pw", tags: new[] { "work", "", "  ", "dev", null! });

        Assert.Equal(2, entry.Tags.Count);
        Assert.Contains("work", entry.Tags);
        Assert.Contains("dev", entry.Tags);
    }

    [Fact]
    public void CreateNew_GeneratesUniqueIds()
    {
        var entry1 = VaultEntry.CreateNew("Entry1", "pw");
        var entry2 = VaultEntry.CreateNew("Entry2", "pw");
        var entry3 = VaultEntry.CreateNew("Entry3", "pw");

        Assert.NotEqual(entry1.Id, entry2.Id);
        Assert.NotEqual(entry2.Id, entry3.Id);
        Assert.NotEqual(entry1.Id, entry3.Id);
        Assert.NotEqual(Guid.Empty, entry1.Id);
    }

    [Fact]
    public void Update_WithNullFields_ClearsExistingValues()
    {
        var entry = VaultEntry.CreateNew(
            "GitHub", 
            "pw", 
            username: "carlos", 
            url: "https://github.com", 
            notes: "work");

        entry.Update(
            name: "GitHub Updated",
            password: "newpw",
            username: null,
            url: null,
            notes: null);

        Assert.Equal("GitHub Updated", entry.Name);
        Assert.Equal("newpw", entry.Password);
        Assert.Null(entry.Username);
        Assert.Null(entry.Url);
        Assert.Null(entry.Notes);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_RequiresName(string? badName)
    {
        var entry = VaultEntry.CreateNew("GitHub", "pw");

        Assert.ThrowsAny<Exception>(() => entry.Update(name: badName!, password: "pw"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_RequiresPassword(string? badPassword)
    {
        var entry = VaultEntry.CreateNew("GitHub", "pw");

        Assert.ThrowsAny<Exception>(() => entry.Update(name: "GitHub", password: badPassword!));
    }

    [Fact]
    public void Update_PreservesId()
    {
        var entry = VaultEntry.CreateNew("GitHub", "pw");
        var originalId = entry.Id;

        entry.Update("New Name", "newpw");

        Assert.Equal(originalId, entry.Id);
    }

    [Fact]
    public void Update_PreservesCreatedUtc()
    {
        var t1 = new DateTimeOffset(2025, 12, 14, 0, 0, 0, TimeSpan.Zero);
        var t2 = t1.AddDays(1);

        var entry = VaultEntry.CreateNew("GitHub", "pw", nowUtc: t1);

        entry.Update("New Name", "newpw", nowUtc: t2);

        Assert.Equal(t1, entry.CreatedUtc);
        Assert.Equal(t2, entry.UpdatedUtc);
    }

    [Fact]
    public void Update_ReplacesAllTags()
    {
        var entry = VaultEntry.CreateNew("GitHub", "pw", tags: new[] { "old1", "old2", "old3" });

        entry.Update("GitHub", "pw", tags: new[] { "new1" });

        Assert.Single(entry.Tags);
        Assert.Equal("new1", entry.Tags.First());
    }

    [Fact]
    public void Update_WithNoTags_ClearsExistingTags()
    {
        var entry = VaultEntry.CreateNew("GitHub", "pw", tags: new[] { "work", "dev" });

        entry.Update("GitHub", "pw", tags: null);

        Assert.Empty(entry.Tags);
    }

    [Fact]
    public void Tags_IsReadOnlyCollection()
    {
        var entry = VaultEntry.CreateNew("GitHub", "pw", tags: new[] { "work" });
        Assert.IsAssignableFrom<IReadOnlyCollection<string>>(entry.Tags);
    }

    [Fact]
    public void CreateNew_WithVeryLongValues_StoresThemCorrectly()
    {
        var longName = new string('A', 1000);
        var longPassword = new string('B', 1000);
        var longUsername = new string('C', 1000);
        var longUrl = new string('D', 1000);
        var longNotes = new string('E', 5000);

        var entry = VaultEntry.CreateNew(
            name: longName,
            password: longPassword,
            username: longUsername,
            url: longUrl,
            notes: longNotes);

        Assert.Equal(1000, entry.Name.Length);
        Assert.Equal(1000, entry.Password.Length);
        Assert.Equal(1000, entry.Username!.Length);
        Assert.Equal(1000, entry.Url!.Length);
        Assert.Equal(5000, entry.Notes!.Length);
    }

    [Fact]
    public void CreateNew_WithSpecialCharacters_PreservesThem()
    {
        var entry = VaultEntry.CreateNew(
            name: "Test@#$%^&*()",
            password: "Pá$$wörd!@#",
            username: "user+alias@domain.com",
            url: "https://example.com/path?query=value&other=123",
            notes: "Notes with émojis 🔒🔑 and línés\nof\ntext");

        Assert.Equal("Test@#$%^&*()", entry.Name);
        Assert.Equal("Pá$$wörd!@#", entry.Password);
        Assert.Equal("user+alias@domain.com", entry.Username);
        Assert.Equal("https://example.com/path?query=value&other=123", entry.Url);
        Assert.Contains("🔒🔑", entry.Notes);
    }
}
