using System;
using System.Collections.Generic;
using Vault.Application.Models;
using Vault.Domain;

namespace Vault.Tests;

/// <summary>
/// Helper methods for tests that simplify creating and manipulating vault entries.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates and adds a new entry to the document with the specified fields.
    /// </summary>
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

    /// <summary>
    /// Updates an existing entry in the document.
    /// </summary>
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

    /// <summary>
    /// Deletes an entry from the document.
    /// </summary>
    public static bool DeleteEntry(
        VaultDocument document,
        Guid entryId,
        DateTimeOffset? nowUtc = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        return document.RemoveEntry(entryId, nowUtc);
    }
}
