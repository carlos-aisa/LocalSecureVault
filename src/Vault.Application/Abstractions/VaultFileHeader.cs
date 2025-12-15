namespace Vault.Application.Abstractions;

public sealed record VaultFileHeader(
    string Magic,            // "VLT1"
    ushort Version,          // 1
    ushort Flags,            // bit flags
    byte KdfId,              // 1 = Argon2id
    byte PayloadEncoding,    // 1 = JSON UTF-8
    ushort SchemaVersion,    // 1
    uint Argon2MemoryKiB,
    uint Argon2Iterations,
    ushort Argon2Parallelism,
    byte[] Salt,             // 16 bytes
    byte[] Nonce,            // 12 bytes
    long CreatedUtcTicks,
    long UpdatedUtcTicks,
    byte[] Reserved          // 16 bytes
);
