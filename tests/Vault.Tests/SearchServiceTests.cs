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
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();

        uc.AddEntry(doc, "GitHub", "pw");
        uc.AddEntry(doc, "Google", "pw");

        var svc = new SearchService();
        var result = svc.Search(doc, new SearchQuery(" "));

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Search_FindsByFields()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();

        uc.AddEntry(doc, "GitHub", "pw", username: "carlos", url: "https://github.com", notes: "work", tags: new[] { "dev" });
        uc.AddEntry(doc, "Bank", "pw", username: "me", url: "https://bank.local", notes: "money", tags: new[] { "finance" });

        var svc = new SearchService();

        Assert.Single(svc.Search(doc, new SearchQuery("git")));
        Assert.Single(svc.Search(doc, new SearchQuery("CARLOS")));
        Assert.Single(svc.Search(doc, new SearchQuery("bank.local")));
        Assert.Single(svc.Search(doc, new SearchQuery("money")));
        Assert.Single(svc.Search(doc, new SearchQuery("dev")));
    }
}
