using Vault.Application.Models;
using Vault.Application.Search;
using Vault.Application.UseCases;
using Xunit;

namespace Vault.Tests;

public class SearchServiceTests
{
    [Fact]
    public void Search_EmptyQuery_ReturnsAll()
    {
        var state = new VaultState();
        var uc = new EntryCommands();
        uc.AddEntry(state, "GitHub", "pw");
        uc.AddEntry(state, "Google", "pw");

        var svc = new SearchService();
        var result = svc.Search(state, new SearchQuery(" "));

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Search_FindsByNameUsernameUrlNotesAndTags()
    {
        var state = new VaultState();
        var uc = new EntryCommands();

        uc.AddEntry(state, "GitHub", "pw", username: "carlos", url: "https://github.com", notes: "work", tags: new[] { "dev" });
        uc.AddEntry(state, "Bank", "pw", username: "me", url: "https://bank.local", notes: "money", tags: new[] { "finance" });

        var svc = new SearchService();

        Assert.Single(svc.Search(state, new SearchQuery("git")));
        Assert.Single(svc.Search(state, new SearchQuery("CARLOS")));
        Assert.Single(svc.Search(state, new SearchQuery("bank.local")));
        Assert.Single(svc.Search(state, new SearchQuery("money")));
        Assert.Single(svc.Search(state, new SearchQuery("dev")));
    }
}
