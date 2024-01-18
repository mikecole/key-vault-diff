using cole.key_vault_diff.Secret;
using FluentAssertions;

namespace tests.Secret;

public class SecretWriterTests: SecretsTestBase
{
    [Fact]
    public async Task GetDiff_DisplaysAdds_WhenDoNotExistInDestination()
    {
        var vaults = await GetCleanVaults();

        await vaults.SourceClient.SetSecretAsync("Foo", "foo");

        var sut = new SecretWriter();
        await sut.CreateSecret(vaults.SourceClient, vaults.DestinationClient, "Foo");

        var secret = await vaults.DestinationClient.GetSecretAsync("Foo");
        secret.Value.Value.Should().Be("foo");
    }
}