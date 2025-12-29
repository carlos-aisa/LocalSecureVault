using Vault.Application.Models;

namespace Vault.Application.Abstractions;
public enum VaultErrorCode
{
    InvalidFormat,
    InvalidPath,
    UnsupportedOrCorrupted,
    AccessDenied,
    FileNotFound,
    IoError,
    Unknown
}

public sealed record VaultError(VaultErrorCode Code, string UserMessage, string? Detail = null);

public sealed record VaultResult<T>(T? Value, VaultError? Error)
{
    public bool IsSuccess => Error is null;
    public static VaultResult<T> Ok(T value) => new(value, null);
    public static VaultResult<T> Fail(VaultError error) => new(default, error);
}

public sealed record UnlockedVault(string Path, VaultDocument Document, byte[] SessionKey, VaultFileHeader Header);