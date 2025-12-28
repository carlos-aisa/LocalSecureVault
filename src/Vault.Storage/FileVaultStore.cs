using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vault.Application.Abstractions;
using System.Text;

namespace Vault.Storage;

public sealed class FileVaultStore : IVaultStore
{
    public async Task<VaultFile> ReadAsync(string path, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Invalid path.", nameof(path));

        using var fs = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        if (fs.Length < VaultFormatConstants.HeaderSizeV1 + VaultFormatConstants.TagSize)
            throw new InvalidDataException("File too small to be a vault.");

        // Read header
        var headerBytes = new byte[VaultFormatConstants.HeaderSizeV1];
        await ReadExactAsync(fs, headerBytes, ct);

        var header = DeserializeHeader(headerBytes);

        // Remaining = ciphertext + tag
        var remaining = fs.Length - VaultFormatConstants.HeaderSizeV1;
        if (remaining <= VaultFormatConstants.TagSize)
            throw new InvalidDataException("Invalid vault file.");

        var cipherLen = remaining - VaultFormatConstants.TagSize;

        var ciphertext = new byte[cipherLen];
        await ReadExactAsync(fs, ciphertext, ct);

        var tag = new byte[VaultFormatConstants.TagSize];
        await ReadExactAsync(fs, tag, ct);

        return new VaultFile(header, ciphertext, tag);
    }

    public async Task WriteAtomicAsync(string path, VaultFile file, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Invalid path.", nameof(path));
        ArgumentNullException.ThrowIfNull(file);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var tempPath = path + ".tmp";

        using (var fs = new FileStream(
            tempPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true))
        {
            var headerBytes = SerializeHeader(file.Header);
            await fs.WriteAsync(headerBytes, ct);
            await fs.WriteAsync(file.Ciphertext, ct);
            await fs.WriteAsync(file.Tag, ct);
            await fs.FlushAsync(ct);
        }

        // Atomic replace (Windows)
        if (File.Exists(path))
            File.Replace(tempPath, path, null);
        else
            File.Move(tempPath, path);
    }

    // -----------------------
    // Helpers
    // -----------------------

    private static async Task ReadExactAsync(Stream s, byte[] buffer, CancellationToken ct)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await s.ReadAsync(buffer.AsMemory(offset), ct);
            if (read == 0)
                throw new EndOfStreamException();
            offset += read;
        }
    }

    private static byte[] SerializeHeader(VaultFileHeader h)
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

    private static VaultFileHeader DeserializeHeader(byte[] buffer)
    {
        var offset = 0;

        var magic = Encoding.ASCII.GetString(buffer, offset, 4);
        offset += 4;

        var version = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(offset, 2));
        offset += 2;

        var flags = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(offset, 2));
        offset += 2;

        var kdfId = buffer[offset++];
        var payloadEncoding = buffer[offset++];

        var schemaVersion = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(offset, 2));
        offset += 2;

        var mem = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(offset, 4));
        offset += 4;

        var iter = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(offset, 4));
        offset += 4;

        var par = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(offset, 2));
        offset += 2;

        var salt = buffer.AsSpan(offset, VaultFormatConstants.SaltSize).ToArray();
        offset += VaultFormatConstants.SaltSize;

        var nonce = buffer.AsSpan(offset, VaultFormatConstants.NonceSize).ToArray();
        offset += VaultFormatConstants.NonceSize;

        var created = BinaryPrimitives.ReadInt64LittleEndian(buffer.AsSpan(offset, 8));
        offset += 8;

        var updated = BinaryPrimitives.ReadInt64LittleEndian(buffer.AsSpan(offset, 8));
        offset += 8;

        var reserved = buffer.AsSpan(offset, VaultFormatConstants.ReservedSize).ToArray();

        return new VaultFileHeader(
            magic,
            version,
            flags,
            kdfId,
            payloadEncoding,
            schemaVersion,
            mem,
            iter,
            par,
            salt,
            nonce,
            created,
            updated,
            reserved);
    }
}
