using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using tests.Utility;

namespace tests.Secret;

public abstract class SecretsTestBase
{
    protected async Task<(SecretClient SourceClient, SecretClient DestinationClient)> GetCleanVaults()
    {
        //lowkey-vault provides two faked key vaults by default. The addresses are the same but they register
        //as different key vaults
        const string sourceUri = "https://127.0.0.1:8443";
        const string destinationUri = "https://localhost:8443";

        await LowKeyManagementApi.DeleteVault(sourceUri);
        await LowKeyManagementApi.PurgeDeletedVault(sourceUri);
        await LowKeyManagementApi.CreateVault(sourceUri);
        await LowKeyManagementApi.DeleteVault(destinationUri);
        await LowKeyManagementApi.PurgeDeletedVault(destinationUri);
        await LowKeyManagementApi.CreateVault(destinationUri);

        return new(GetKeyVaultSecretClient(new Uri(sourceUri)), GetKeyVaultSecretClient(new Uri(destinationUri)));
    }
    
    private SecretClient GetKeyVaultSecretClient(Uri uri)
    {
        var credentials = new NoopCredentials();
        
        var options = new SecretClientOptions
        {
            DisableChallengeResourceVerification = true
        };
        
        return new SecretClient(uri, credentials, FakeClientOptions.GetClientOptions(options));
    }
    
    protected CertificateClient GetKeyVaultCertificateClient(Uri uri)
    {
        var credentials = new NoopCredentials();
        
        var options = new CertificateClientOptions(CertificateClientOptions.ServiceVersion.V7_3)
        {
            DisableChallengeResourceVerification = true,
        };
        
        return new CertificateClient(uri, credentials, FakeClientOptions.GetClientOptions(options));
    }
}