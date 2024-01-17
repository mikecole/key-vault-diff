using Azure.Security.KeyVault.Certificates;
using cole.key_vault_diff.Secret;
using FluentAssertions;

namespace tests.Secret;

public class SecretDiffEngineTests : SecretsTestBase
{
    [Fact]
    public async Task GetDiff_DisplaysAdds_WhenDoNotExistInDestination()
    {
        var vaults = await GetCleanVaults();

        await vaults.SourceClient.SetSecretAsync("Foo", "foo");
        await vaults.SourceClient.SetSecretAsync("Bar", "bar");

        var sut = new SecretDiffEngine();
        var result = await sut.GetDiff(vaults.SourceClient, vaults.DestinationClient);

        result.Should().BeEquivalentTo(new List<SecretDiffResult>
        {
            new("Foo", DiffOperation.Add),
            new("Bar", DiffOperation.Add),
        });
    }

    [Fact]
    public async Task GetDiff_DisplaysDeletes_WhenDoNotExistInSource()
    {
        var vaults = await GetCleanVaults();

        await vaults.DestinationClient.SetSecretAsync("Foo", "foo");
        await vaults.DestinationClient.SetSecretAsync("Bar", "bar");

        var sut = new SecretDiffEngine();
        var result = await sut.GetDiff(vaults.SourceClient, vaults.DestinationClient);

        result.Should().BeEquivalentTo(new List<SecretDiffResult>
        {
            new("Foo", DiffOperation.Delete),
            new("Bar", DiffOperation.Delete),
        });
    }

    [Fact]
    public async Task GetDiff_DisplaysEquals_WhenSameInBoth()
    {
        var vaults = await GetCleanVaults();

        await vaults.SourceClient.SetSecretAsync("Foo", "foo");
        await vaults.SourceClient.SetSecretAsync("Bar", "bar");
        await vaults.DestinationClient.SetSecretAsync("Foo", "foo");
        await vaults.DestinationClient.SetSecretAsync("Bar", "bar");

        var sut = new SecretDiffEngine();
        var result = await sut.GetDiff(vaults.SourceClient, vaults.DestinationClient);

        result.Should().BeEquivalentTo(new List<SecretDiffResult>
        {
            new("Foo", DiffOperation.Equals),
            new("Bar", DiffOperation.Equals),
        });
    }
    
    [Fact]
    public async Task GetDiff_DisplaysModify_WhenDifferentInBoth()
    {
        var vaults = await GetCleanVaults();

        await vaults.SourceClient.SetSecretAsync("Foo", "foo");
        await vaults.SourceClient.SetSecretAsync("Bar", "bar");
        await vaults.DestinationClient.SetSecretAsync("Foo", "foo1");
        await vaults.DestinationClient.SetSecretAsync("Bar", "bar1");

        var sut = new SecretDiffEngine();
        var result = await sut.GetDiff(vaults.SourceClient, vaults.DestinationClient);

        result.Should().BeEquivalentTo(new List<SecretDiffResult>
        {
            new("Foo", DiffOperation.Modify),
            new("Bar", DiffOperation.Modify),
        });
    }
    
    [Fact]
    public async Task GetDiff_AggregatesAllOperationsInAlphabeticalOrderByName()
    {
        var vaults = await GetCleanVaults();

        await vaults.SourceClient.SetSecretAsync("Foo", "foo");
        await vaults.SourceClient.SetSecretAsync("Bar", "bar1");
        await vaults.DestinationClient.SetSecretAsync("Bar", "bar2");
        await vaults.SourceClient.SetSecretAsync("Baz", "baz");
        await vaults.DestinationClient.SetSecretAsync("Baz", "baz");
        await vaults.DestinationClient.SetSecretAsync("Quz", "bar2");

        var sut = new SecretDiffEngine();
        var result = await sut.GetDiff(vaults.SourceClient, vaults.DestinationClient);

        result.Should().BeEquivalentTo(new List<SecretDiffResult>
        {
            new("Bar", DiffOperation.Modify),
            new("Baz", DiffOperation.Equals),
            new("Foo", DiffOperation.Add),
            new("Quz", DiffOperation.Delete),
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetDiff_DoesNotIncludeManagedSecrets()
    {
        var vaults = await GetCleanVaults();
        
       var certificateClient = GetKeyVaultCertificateClient(vaults.DestinationClient.VaultUri);
       
        var certificatePolicy = new CertificatePolicy("Self", "CN=example.com")
        {
            KeyType = CertificateKeyType.Ec,
            KeyCurveName = CertificateKeyCurveName.P256,
            Exportable = true,
            ContentType = CertificateContentType.Pkcs12,
            ValidityInMonths = 12
        };
        
        //Creating a certificate will also create a managed secret
        var foo = await certificateClient.StartCreateCertificateAsync("test-cert", certificatePolicy, true);
        await foo.WaitForCompletionAsync();
        
        var sut = new SecretDiffEngine();
        var result = await sut.GetDiff(vaults.SourceClient, vaults.DestinationClient);

        result.Should().BeEmpty();
    }
}