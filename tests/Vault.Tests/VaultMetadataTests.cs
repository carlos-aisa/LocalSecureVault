using Vault.Application.Models;

namespace Vault.Tests;

public class VaultMetadataTests
{
    [Fact]
    public void CreateNew_WithValidData_ReturnsMetadata()
    {
        var now = DateTimeOffset.UtcNow;
        var metadata = VaultMetadata.CreateNew("TestVault", 1, now, now);

        Assert.Equal("TestVault", metadata.VaultName);
        Assert.Equal(1, metadata.SchemaVersion);
        Assert.Equal(now, metadata.CreatedUtc);
        Assert.Equal(now, metadata.UpdatedUtc);
    }

    [Fact]
    public void CreateNew_TrimsWhitespace_ReturnsMetadataWithTrimmedName()
    {
        var now = DateTimeOffset.UtcNow;
        var metadata = VaultMetadata.CreateNew("  TestVault  ", 1, now, now);

        Assert.Equal("TestVault", metadata.VaultName);
    }

    [Fact]
    public void CreateNew_WithNullVaultName_ThrowsArgumentNullException()
    {
        var now = DateTimeOffset.UtcNow;
        
        var ex = Assert.Throws<ArgumentNullException>(() =>
            VaultMetadata.CreateNew(null!, 1, now, now));
        
        Assert.Equal("vaultName", ex.ParamName);
    }

    [Fact]
    public void CreateNew_WithEmptyVaultName_ThrowsArgumentException()
    {
        var now = DateTimeOffset.UtcNow;
        
        var ex = Assert.Throws<ArgumentException>(() =>
            VaultMetadata.CreateNew("", 1, now, now));
        
        Assert.Equal("vaultName", ex.ParamName);
        Assert.Contains("cannot be empty", ex.Message);
    }

    [Fact]
    public void CreateNew_WithWhitespaceOnlyVaultName_ThrowsArgumentException()
    {
        var now = DateTimeOffset.UtcNow;
        
        var ex = Assert.Throws<ArgumentException>(() =>
            VaultMetadata.CreateNew("   ", 1, now, now));
        
        Assert.Equal("vaultName", ex.ParamName);
        Assert.Contains("cannot be empty", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void CreateNew_WithInvalidSchemaVersion_ThrowsArgumentOutOfRangeException(int version)
    {
        var now = DateTimeOffset.UtcNow;
        
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            VaultMetadata.CreateNew("TestVault", version, now, now));
        
        Assert.Equal("schemaVersion", ex.ParamName);
        Assert.Contains("must be > 0", ex.Message);
    }

    [Fact]
    public void CreateNew_WithValidSchemaVersions_ReturnsMetadata()
    {
        var now = DateTimeOffset.UtcNow;
        
        var meta1 = VaultMetadata.CreateNew("TestVault", 1, now, now);
        var meta2 = VaultMetadata.CreateNew("TestVault", 2, now, now);
        var meta100 = VaultMetadata.CreateNew("TestVault", 100, now, now);

        Assert.Equal(1, meta1.SchemaVersion);
        Assert.Equal(2, meta2.SchemaVersion);
        Assert.Equal(100, meta100.SchemaVersion);
    }

    [Fact]
    public void CreateNew_WithDifferentTimestamps_ReturnsMetadataWithCorrectTimestamps()
    {
        var created = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var updated = new DateTimeOffset(2024, 6, 15, 12, 30, 0, TimeSpan.Zero);

        var metadata = VaultMetadata.CreateNew("TestVault", 1, created, updated);

        Assert.Equal(created, metadata.CreatedUtc);
        Assert.Equal(updated, metadata.UpdatedUtc);
    }
}
