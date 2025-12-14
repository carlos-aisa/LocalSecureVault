using Vault.Application.Models;
using Vault.Application.UseCases;
using Xunit;

namespace Vault.Tests;

public class ApplicationEntryCommandsTests
{
    [Fact]
    public void AddEntry_AddsToState()
    {
        var state = new VaultState();
        var uc = new EntryCommands();

        var entry = uc.AddEntry(state, "GitHub", "pw", username: "carlos");

        Assert.Single(state.Entries);
        Assert.Equal(entry.Id, state.Entries[0].Id);
        Assert.Equal("GitHub", entry.Name);
    }

    [Fact]
    public void UpdateEntry_UpdatesExistingEntry()
    {
        var state = new VaultState();
        var uc = new EntryCommands();

        var entry = uc.AddEntry(state, "A", "P");
        uc.UpdateEntry(state, entry.Id, "B", "Q", tags: new[] { "Two", " " });

        var updated = state.Entries.Single(e => e.Id == entry.Id);
        Assert.Equal("B", updated.Name);
        Assert.Equal("Q", updated.Password);
        Assert.Equal(new[] { "Two" }, updated.Tags);
    }

    [Fact]
    public void DeleteEntry_RemovesEntry()
    {
        var state = new VaultState();
        var uc = new EntryCommands();

        var entry = uc.AddEntry(state, "A", "P");
        var removed = uc.DeleteEntry(state, entry.Id);

        Assert.True(removed);
        Assert.Empty(state.Entries);
    }
}
