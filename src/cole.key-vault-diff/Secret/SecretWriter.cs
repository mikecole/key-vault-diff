using Azure.Security.KeyVault.Secrets;

namespace cole.key_vault_diff.Secret;

public interface ISecretWriter
{
    Task CreateSecret(SecretClient sourceClient, SecretClient destinationClient, string name);
}

public class SecretWriter : ISecretWriter
{
    public async Task CreateSecret(SecretClient sourceClient, SecretClient destinationClient, string name)
    {
        var value = await sourceClient.GetSecretAsync(name);
        await destinationClient.SetSecretAsync(new KeyVaultSecret(name, value.Value.Value));
    }
}
