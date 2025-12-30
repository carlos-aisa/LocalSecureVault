using Vault.Application.Abstractions;

namespace Vault.Tests;

public class VaultResultTests
{
    [Fact]
    public void Ok_CreatesSuccessResult()
    {
        var result = VaultResult<int>.Ok(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Ok_WithNullValue_CreatesSuccessResult()
    {
        var result = VaultResult<string?>.Ok(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Fail_CreatesErrorResult()
    {
        var error = new VaultError(VaultErrorCode.InvalidFormat, "Test error");
        var result = VaultResult<int>.Fail(error);

        Assert.False(result.IsSuccess);
        Assert.Equal(0, result.Value);
        Assert.NotNull(result.Error);
        Assert.Equal(VaultErrorCode.InvalidFormat, result.Error!.Code);
        Assert.Equal("Test error", result.Error.UserMessage);
    }

    [Fact]
    public void Fail_WithDetailedError_CreatesErrorResultWithDetail()
    {
        var error = new VaultError(VaultErrorCode.IoError, "IO failed", "File not writable");
        var result = VaultResult<string>.Fail(error);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Equal("IO failed", result.Error!.UserMessage);
        Assert.Equal("File not writable", result.Error.Detail);
    }

    [Theory]
    [InlineData(VaultErrorCode.InvalidFormat)]
    [InlineData(VaultErrorCode.InvalidPath)]
    [InlineData(VaultErrorCode.UnsupportedOrCorrupted)]
    [InlineData(VaultErrorCode.AccessDenied)]
    [InlineData(VaultErrorCode.FileNotFound)]
    [InlineData(VaultErrorCode.IoError)]
    [InlineData(VaultErrorCode.Unknown)]
    public void Fail_WithDifferentErrorCodes_CreatesCorrectErrorResult(VaultErrorCode code)
    {
        var error = new VaultError(code, "Test message");
        var result = VaultResult<int>.Fail(error);

        Assert.False(result.IsSuccess);
        Assert.Equal(code, result.Error!.Code);
    }

    [Fact]
    public void IsSuccess_WhenOk_ReturnsTrue()
    {
        var result = VaultResult<string>.Ok("success");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void IsSuccess_WhenFail_ReturnsFalse()
    {
        var error = new VaultError(VaultErrorCode.Unknown, "Error");
        var result = VaultResult<string>.Fail(error);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void VaultError_WithAllProperties_StoresCorrectly()
    {
        var error = new VaultError(
            VaultErrorCode.AccessDenied,
            "Access was denied",
            "Insufficient permissions"
        );

        Assert.Equal(VaultErrorCode.AccessDenied, error.Code);
        Assert.Equal("Access was denied", error.UserMessage);
        Assert.Equal("Insufficient permissions", error.Detail);
    }

    [Fact]
    public void VaultError_WithoutDetail_HasNullDetail()
    {
        var error = new VaultError(VaultErrorCode.FileNotFound, "File not found");

        Assert.Equal(VaultErrorCode.FileNotFound, error.Code);
        Assert.Equal("File not found", error.UserMessage);
        Assert.Null(error.Detail);
    }
}
