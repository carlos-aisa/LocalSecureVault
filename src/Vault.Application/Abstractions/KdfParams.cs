namespace Vault.Application.Abstractions;

public sealed record KdfParams(
    uint MemoryKiB,
    uint Iterations,
    ushort Parallelism,
    int KeyLengthBytes = 32
);
