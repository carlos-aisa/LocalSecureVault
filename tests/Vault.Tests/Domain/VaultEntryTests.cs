using Vault.Domain;
using Xunit;

namespace Vault.Tests;

public class VaultEntryTests
{
    [Fact]
    public void CreateNew_TrimsAndNormalizesFields()
    {
        var now = new DateTimeOffset(2025, 12, 14, 0, 0, 0, TimeSpan.Zero);

        var entry = VaultEntry.CreateNew(
            name: "  GitHub  ",
            password: "  secret  ",
            username: "  carlos  ",
            url: "  https://github.com  ",
            notes: "  note  ",
            tags: new[] { " Dev ", "personal", "DEV", " " },
            nowUtc: now);

        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal("GitHub", entry.Name);
        Assert.Equal("secret", entry.Password);
        Assert.Equal("carlos", entry.Username);
        Assert.Equal("https://github.com", entry.Url);
        Assert.Equal("note", entry.Notes);

        // Domain doesn't deduplicate/lowercase/order tags; it only trims and removes empties.
        Assert.Equal(new[] { "Dev", "personal", "DEV" }, entry.Tags);

        Assert.Equal(now, entry.CreatedUtc);
        Assert.Equal(now, entry.UpdatedUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateNew_RequiresName(string? badName)
    {
        Assert.ThrowsAny<Exception>(() =>
            VaultEntry.CreateNew(name: badName!, password: "x"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateNew_RequiresPassword(string? badPassword)
    {
        Assert.ThrowsAny<Exception>(() =>
            VaultEntry.CreateNew(name: "ok", password: badPassword!));
    }

    [Fact]
    public void Update_ChangesUpdatedUtcAndReplacesTags()
    {
        var t1 = new DateTimeOffset(2025, 12, 14, 0, 0, 0, TimeSpan.Zero);
        var t2 = t1.AddMinutes(10);

        var entry = VaultEntry.CreateNew("A", "P", tags: new[] { " one ", "two" }, nowUtc: t1);

        entry.Update(
            name: "B",
            password: "Q",
            tags: new[] { " Two ", "three", "  " },
            nowUtc: t2);

        Assert.Equal("B", entry.Name);
        Assert.Equal("Q", entry.Password);
        Assert.Equal(new[] { "Two", "three" }, entry.Tags);

        Assert.Equal(t1, entry.CreatedUtc);
        Assert.Equal(t2, entry.UpdatedUtc);
    }
}
