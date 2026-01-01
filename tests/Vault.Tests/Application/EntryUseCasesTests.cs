using System;
using System.Collections.Generic;
using Vault.Application.Abstractions;
using Vault.Application.Models;
using Vault.Application.UseCases;
using Vault.Domain;
using Xunit;

namespace Vault.Tests;

public class EntryUseCasesTests
{
    [Fact]
    public void Add_WithUniqueEntry_Succeeds()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();
        var entry = VaultEntry.CreateNew("GitHub", "pw123");

        var result = uc.Add(doc, entry);

        Assert.True(result.IsSuccess);
        Assert.Equal(entry.Id, result.Value);
        Assert.Single(doc.Entries);
    }

    [Fact]
    public void Add_WithDuplicateNameAndUsername_Fails()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();
        
        var entry1 = VaultEntry.CreateNew("GitHub", "pw1", username: "carlos");
        var entry2 = VaultEntry.CreateNew("GitHub", "pw2", username: "carlos");
        
        uc.Add(doc, entry1);
        var result = uc.Add(doc, entry2);

        Assert.False(result.IsSuccess);
        Assert.Equal(VaultErrorCode.InvalidFormat, result.Error!.Code);
        Assert.Contains("already exists", result.Error.UserMessage);
        Assert.Single(doc.Entries);
    }

    [Fact]
    public void Add_WithSameNameButDifferentUsername_Succeeds()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();
        
        var entry1 = VaultEntry.CreateNew("GitHub", "pw1", username: "carlos");
        var entry2 = VaultEntry.CreateNew("GitHub", "pw2", username: "john");
        
        uc.Add(doc, entry1);
        var result = uc.Add(doc, entry2);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, doc.Entries.Count);
    }

    [Fact]
    public void Add_DuplicateDetectionIsCaseInsensitive()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();
        
        var entry1 = VaultEntry.CreateNew("GitHub", "pw1", username: "Carlos");
        var entry2 = VaultEntry.CreateNew("GITHUB", "pw2", username: "CARLOS");
        
        uc.Add(doc, entry1);
        var result = uc.Add(doc, entry2);

        Assert.False(result.IsSuccess);
        Assert.Single(doc.Entries);
    }

    [Fact]
    public void Add_WithNullDocument_ThrowsArgumentNullException()
    {
        var uc = new EntryUseCases();
        var entry = VaultEntry.CreateNew("GitHub", "pw");

        Assert.Throws<ArgumentNullException>(() => uc.Add(null!, entry));
    }

    [Fact]
    public void Add_WithNullEntry_ThrowsArgumentNullException()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();

        Assert.Throws<ArgumentNullException>(() => uc.Add(doc, null!));
    }

    [Fact]
    public void Update_WithValidChanges_Succeeds()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();
        var entry = VaultEntry.CreateNew("GitHub", "oldpw", username: "carlos");
        uc.Add(doc, entry);

        var result = uc.Update(
            doc,
            entry.Id,
            "GitHub Updated",
            "newpw",
            "newuser",
            "https://github.com",
            "Updated notes",
            new[] { "dev", "work" });

        Assert.True(result.IsSuccess);
        Assert.Equal("GitHub Updated", entry.Name);
        Assert.Equal("newpw", entry.Password);
        Assert.Equal("newuser", entry.Username);
    }

    [Fact]
    public void Update_WithNonExistentId_Fails()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();

        var result = uc.Update(
            doc,
            Guid.NewGuid(),
            "Name",
            "pw",
            null, null, null,
            Array.Empty<string>());

        Assert.False(result.IsSuccess);
        Assert.Equal(VaultErrorCode.InvalidFormat, result.Error!.Code);
        Assert.Contains("Entry not found", result.Error.UserMessage);
    }

    [Fact]
    public void Update_ChangingToExistingNameAndUsername_Fails()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();
        
        var entry1 = VaultEntry.CreateNew("GitHub", "pw1", username: "carlos");
        var entry2 = VaultEntry.CreateNew("GitLab", "pw2", username: "john");
        uc.Add(doc, entry1);
        uc.Add(doc, entry2);

        var result = uc.Update(
            doc,
            entry2.Id,
            "GitHub",
            "pw2",
            "carlos",
            null, null,
            Array.Empty<string>());

        Assert.False(result.IsSuccess);
        Assert.Equal(VaultErrorCode.InvalidFormat, result.Error!.Code);
        Assert.Contains("Another entry", result.Error.UserMessage);
        Assert.Equal("GitLab", entry2.Name); // Should not have changed
    }

    [Fact]
    public void Update_KeepingSameNameAndUsername_Succeeds()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();
        
        var entry = VaultEntry.CreateNew("GitHub", "oldpw", username: "carlos");
        uc.Add(doc, entry);

        var result = uc.Update(
            doc,
            entry.Id,
            "GitHub",
            "newpw",
            "carlos",
            "https://github.com",
            null,
            Array.Empty<string>());

        Assert.True(result.IsSuccess);
        Assert.Equal("newpw", entry.Password);
    }

    [Fact]
    public void Update_DuplicateCheckIsCaseInsensitive()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();
        
        var entry1 = VaultEntry.CreateNew("GitHub", "pw1", username: "carlos");
        var entry2 = VaultEntry.CreateNew("GitLab", "pw2", username: "john");
        uc.Add(doc, entry1);
        uc.Add(doc, entry2);

        var result = uc.Update(
            doc,
            entry2.Id,
            "GITHUB",
            "pw2",
            "CARLOS",
            null, null,
            Array.Empty<string>());

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Delete_WithExistingEntry_Succeeds()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();
        var entry = VaultEntry.CreateNew("GitHub", "pw");
        uc.Add(doc, entry);

        var result = uc.Delete(doc, entry.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(doc.Entries);
    }

    [Fact]
    public void Delete_WithNonExistentId_Fails()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();

        var result = uc.Delete(doc, Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(VaultErrorCode.InvalidFormat, result.Error!.Code);
        Assert.Contains("Entry not found", result.Error.UserMessage);
    }

    [Fact]
    public void Delete_WithNullDocument_ThrowsArgumentNullException()
    {
        var uc = new EntryUseCases();

        Assert.Throws<ArgumentNullException>(() => uc.Delete(null!, Guid.NewGuid()));
    }

    [Fact]
    public void Add_WithBothNullUsernames_DetectsAsDuplicate()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();
        
        var entry1 = VaultEntry.CreateNew("Service", "pw1", username: null);
        var entry2 = VaultEntry.CreateNew("Service", "pw2", username: null);
        
        uc.Add(doc, entry1);
        var result = uc.Add(doc, entry2);

        Assert.False(result.IsSuccess);
        Assert.Single(doc.Entries);
    }

    [Fact]
    public void Add_WithEmptyUsernameAndNullUsername_DetectsAsDuplicate()
    {
        var doc = VaultDocument.CreateNew("Vault");
        var uc = new EntryUseCases();
        
        var entry1 = VaultEntry.CreateNew("Service", "pw1", username: "");
        var entry2 = VaultEntry.CreateNew("Service", "pw2", username: null);
        
        uc.Add(doc, entry1);
        var result = uc.Add(doc, entry2);

        Assert.False(result.IsSuccess);
        Assert.Single(doc.Entries);
    }
}
