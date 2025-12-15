using Vault.Application.Abstractions;
using Xunit;

namespace Vault.Tests;

public class VaultFormatConstantsTests
{
    [Fact]
    public void Constants_AreAsExpected()
    {
        Assert.Equal("VLT1", VaultFormatConstants.Magic);
        Assert.Equal((ushort)1, VaultFormatConstants.Version);

        Assert.Equal(16, VaultFormatConstants.SaltSize);
        Assert.Equal(12, VaultFormatConstants.NonceSize);
        Assert.Equal(16, VaultFormatConstants.TagSize);
        Assert.Equal(16, VaultFormatConstants.ReservedSize);

        Assert.Equal(82, VaultFormatConstants.HeaderSizeV1);
    }
}
