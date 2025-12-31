using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vault.Application.Abstractions;
using System.Text;

#if ANDROID
using Android.Content;
#endif

namespace Vault.Storage;

public sealed class FileVaultStore : IVaultStore
{
    public async Task<VaultFile> ReadAsync(string path, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Invalid path.", nameof(path));

#if ANDROID
        // On Android, check if this is a content:// URI
        if (path.StartsWith("content://", StringComparison.OrdinalIgnoreCase))
        {
            return await ReadFromContentUriAsync(path, ct);
        }
#endif

        // Traditional file system path
        using var fs = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        return await ReadFromStreamAsync(fs, ct);
    }

    public async Task WriteAtomicAsync(string path, VaultFile file, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Invalid path.", nameof(path));
        ArgumentNullException.ThrowIfNull(file);

#if ANDROID
        // On Android, check if this is a content:// URI
        if (path.StartsWith("content://", StringComparison.OrdinalIgnoreCase))
        {
            await WriteToContentUriAsync(path, file, ct);
            return;
        }
#endif

        // Traditional file system path - atomic write with temp file
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
            await WriteToStreamAsync(fs, file, ct);
        }

        // Atomic replace (Windows)
        if (File.Exists(path))
            File.Replace(tempPath, path, null);
        else
            File.Move(tempPath, path);
    }

    // -----------------------
    // Stream-based operations (platform-agnostic)
    // -----------------------

    private static async Task<VaultFile> ReadFromStreamAsync(Stream stream, CancellationToken ct)
    {
        if (stream.Length < VaultFormatConstants.HeaderSizeV1 + VaultFormatConstants.TagSize)
            throw new InvalidDataException("File too small to be a vault.");

        // Read header
        var headerBytes = new byte[VaultFormatConstants.HeaderSizeV1];
        await ReadExactAsync(stream, headerBytes, ct);

        var header = DeserializeHeader(headerBytes);

        // Remaining = ciphertext + tag
        var remaining = stream.Length - VaultFormatConstants.HeaderSizeV1;
        if (remaining <= VaultFormatConstants.TagSize)
            throw new InvalidDataException("Invalid vault file.");

        var cipherLen = remaining - VaultFormatConstants.TagSize;

        var ciphertext = new byte[cipherLen];
        await ReadExactAsync(stream, ciphertext, ct);

        var tag = new byte[VaultFormatConstants.TagSize];
        await ReadExactAsync(stream, tag, ct);

        return new VaultFile(header, ciphertext, tag);
    }

    private static async Task WriteToStreamAsync(Stream stream, VaultFile file, CancellationToken ct)
    {
        var headerBytes = SerializeHeader(file.Header);
        await stream.WriteAsync(headerBytes, ct);
        await stream.WriteAsync(file.Ciphertext, ct);
        await stream.WriteAsync(file.Tag, ct);
        await stream.FlushAsync(ct);
    }

#if ANDROID
    // -----------------------
    // Android-specific content:// URI handling
    // -----------------------

    private static async Task<VaultFile> ReadFromContentUriAsync(string uriString, CancellationToken ct)
    {
        var context = Android.App.Application.Context;
        var contentResolver = context.ContentResolver;
        
        if (contentResolver == null)
            throw new InvalidOperationException("ContentResolver not available");

        var uri = Android.Net.Uri.Parse(uriString);
        if (uri == null)
            throw new ArgumentException($"Invalid content URI: {uriString}");

        using var inputStream = contentResolver.OpenInputStream(uri);
        if (inputStream == null)
            throw new IOException($"Could not open input stream for URI: {uriString}");

        using var ms = new MemoryStream();
        await inputStream.CopyToAsync(ms, ct);
        ms.Position = 0;

        return await ReadFromStreamAsync(ms, ct);
    }

    private static async Task WriteToContentUriAsync(string uriString, VaultFile file, CancellationToken ct)
    {
        var context = Android.App.Application.Context;
        var contentResolver = context.ContentResolver;
        
        if (contentResolver == null)
            throw new InvalidOperationException("ContentResolver not available");

        var uri = Android.Net.Uri.Parse(uriString);
        if (uri == null)
            throw new ArgumentException($"Invalid content URI: {uriString}");

        // Write to content URI (truncates existing content)
        using var outputStream = contentResolver.OpenOutputStream(uri, "wt"); // "wt" = write truncate
        if (outputStream == null)
            throw new IOException($"Could not open output stream for URI: {uriString}");

        await WriteToStreamAsync(outputStream, file, ct);
    }
#endif

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
