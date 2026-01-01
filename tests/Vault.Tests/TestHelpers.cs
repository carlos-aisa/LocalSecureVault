using System;
using System.Collections.Generic;
using Vault.Application.Models;
using Vault.Domain;

namespace Vault.Tests;

public static class TestHelpers
{
    public static VaultEntry AddEntry(
        VaultDocument document,
        string name,
        string password,
        string? username = null,
        string? url = null,
        string? notes = null,
        IEnumerable<string>? tags = null,
        DateTimeOffset? nowUtc = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        var entry = VaultEntry.CreateNew(
            name: name,
            password: password,
            username: username,
            url: url,
            notes: notes,
            tags: tags,
            nowUtc: nowUtc);

        document.AddEntry(entry, nowUtc);
        return entry;
    }

    public static void UpdateEntry(
        VaultDocument document,
        Guid entryId,
        string name,
        string password,
        string? username = null,
        string? url = null,
        string? notes = null,
        IEnumerable<string>? tags = null,
        DateTimeOffset? nowUtc = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        var entry = document.GetEntry(entryId);

        entry.Update(
            name: name,
            password: password,
            username: username,
            url: url,
            notes: notes,
            tags: tags,
            nowUtc: nowUtc);

        document.Touch(nowUtc);
    }

    public static bool DeleteEntry(
        VaultDocument document,
        Guid entryId,
        DateTimeOffset? nowUtc = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        return document.RemoveEntry(entryId, nowUtc);
    }
}
