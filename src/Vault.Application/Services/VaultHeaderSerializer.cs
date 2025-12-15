using System.Buffers.Binary;
using System.Text;
using Vault.Application.Abstractions;

namespace Vault.Application.Services;

public static class VaultHeaderSerializer
{
    public static byte[] SerializeHeader(VaultFileHeader h)
    {
        var buffer = new byte[VaultFormatConstants.HeaderSizeV1];
        var offset = 0;

        Encoding.ASCII.GetBytes(h.Magic, buffer.AsSpan(offset, 4));
        offset += 4;

        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset, 2), h.Version);
        offset += 2;

        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset, 2), h.Flags);
        offset += 2;

        buffer[offset++] = h.KdfId;
        buffer[offset++] = h.PayloadEncoding;

        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset, 2), h.SchemaVersion);
        offset += 2;

        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset, 4), h.Argon2MemoryKiB);
        offset += 4;

        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset, 4), h.Argon2Iterations);
        offset += 4;

        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset, 2), h.Argon2Parallelism);
        offset += 2;

        h.Salt.CopyTo(buffer.AsSpan(offset, VaultFormatConstants.SaltSize));
        offset += VaultFormatConstants.SaltSize;

        h.Nonce.CopyTo(buffer.AsSpan(offset, VaultFormatConstants.NonceSize));
        offset += VaultFormatConstants.NonceSize;

        BinaryPrimitives.WriteInt64LittleEndian(buffer.AsSpan(offset, 8), h.CreatedUtcTicks);
        offset += 8;

        BinaryPrimitives.WriteInt64LittleEndian(buffer.AsSpan(offset, 8), h.UpdatedUtcTicks);
        offset += 8;

        h.Reserved.CopyTo(buffer.AsSpan(offset, VaultFormatConstants.ReservedSize));

        return buffer;
    }
}
