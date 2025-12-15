namespace Vault.Application.Abstractions;

public static class VaultFormatConstants
{
    public const string Magic = "VLT1";
    public const ushort Version = 1;

    public const int SaltSize = 16;
    public const int NonceSize = 12;
    public const int TagSize = 16;
    public const int ReservedSize = 16;

    // Header v1 fixed size (bytes) as specified in docs/file-format.md
    public const int HeaderSizeV1 = 82;

    public const byte KdfIdArgon2id = 1;
    public const byte PayloadJsonUtf8 = 1;
}
