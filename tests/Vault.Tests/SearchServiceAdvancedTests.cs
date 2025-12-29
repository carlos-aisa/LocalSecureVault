using System;
using System.Linq;
using Vault.Application.Models;
using Vault.Application.Search;
using Vault.Application.UseCases;
using Xunit;

namespace Vault.Tests;

public class SearchServiceAdvancedTests
{
    [Fact]
    public void Search_PartialMatch()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();

        uc.AddEntry(doc, "GitHub Enterprise", "pw");
        uc.AddEntry(doc, "GitLab Premium", "pw");

        var svc = new SearchService();

        var result = svc.Search(doc, new SearchQuery("git"));
        Assert.Equal(2, result.Count);

        result = svc.Search(doc, new SearchQuery("hub"));
        Assert.Single(result);
        Assert.Equal("GitHub Enterprise", result[0].Name);
    }

    [Fact]
    public void Search_ByTags()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();

        uc.AddEntry(doc, "Account1", "pw", tags: new[] { "work", "dev" });
        uc.AddEntry(doc, "Account2", "pw", tags: new[] { "personal", "dev" });
        uc.AddEntry(doc, "Account3", "pw", tags: new[] { "work", "finance" });

        var svc = new SearchService();

        Assert.Equal(2, svc.Search(doc, new SearchQuery("dev")).Count);
        Assert.Equal(2, svc.Search(doc, new SearchQuery("work")).Count);
        Assert.Single(svc.Search(doc, new SearchQuery("finance")));
        Assert.Single(svc.Search(doc, new SearchQuery("personal")));
    }

    [Fact]
    public void Search_NoMatches_ReturnsEmpty()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();

        uc.AddEntry(doc, "GitHub", "pw", username: "carlos");

        var svc = new SearchService();

        var result = svc.Search(doc, new SearchQuery("nonexistent"));
        Assert.Empty(result);
    }

    [Fact]
    public void Search_SpecialCharacters_StillWorks()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();

        uc.AddEntry(doc, "C++ Editor", "pw", url: "https://example.com/path?query=value");

        var svc = new SearchService();

        Assert.Single(svc.Search(doc, new SearchQuery("C++")));
        Assert.Single(svc.Search(doc, new SearchQuery("example.com")));
        Assert.Single(svc.Search(doc, new SearchQuery("query=value")));
    }

    [Fact]
    public void Search_WhitespaceInQuery_IsTrimmed()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();

        uc.AddEntry(doc, "GitHub", "pw");
        uc.AddEntry(doc, "GitLab", "pw");

        var svc = new SearchService();

        // Leading/trailing whitespace
        Assert.Single(svc.Search(doc, new SearchQuery("  GitHub  ")));
        
        // Query with just whitespace should return all
        Assert.Equal(2, svc.Search(doc, new SearchQuery("   ")).Count);
    }
}
