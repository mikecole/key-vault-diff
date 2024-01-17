using Azure.Security.KeyVault.Secrets;

namespace cole.key_vault_diff.Secret;

public interface ISecretDiffEngine
{
    Task<List<SecretDiffResult>> GetDiff(SecretClient sourceClient, SecretClient destinationClient);
}

public class SecretDiffEngine : ISecretDiffEngine
{
    public async Task<List<SecretDiffResult>> GetDiff(SecretClient sourceClient, SecretClient destinationClient)
    {
        var sourceSecrets = await sourceClient.GetPropertiesOfSecretsAsync()
            .Where(c => !c.Managed)
            .ToDictionaryAsync(c => c.Name);
        
        var destinationSecrets = await destinationClient.GetPropertiesOfSecretsAsync()
            .Where(c => !c.Managed)
            .ToDictionaryAsync(c => c.Name);

        var newAdds = sourceSecrets
            .Where(c => !destinationSecrets.ContainsKey(c.Key))
            .Select(c => new SecretDiffResult(c.Key, DiffOperation.Add));

        var newDeletes = destinationSecrets
            .Where(c => !sourceSecrets.ContainsKey(c.Key))
            .Select(c => new SecretDiffResult(c.Key, DiffOperation.Delete));

        var matches = await Task.WhenAll(
            sourceSecrets
                .Where(c => destinationSecrets.ContainsKey(c.Key))
                .Select(async c =>
                {
                    var sourceSecret = await sourceClient.GetSecretAsync(c.Key);
                    var destinationSecret = await destinationClient.GetSecretAsync(c.Key);

                    return new SecretDiffResult(c.Key,
                        sourceSecret.Value.Value.Equals(destinationSecret.Value.Value)
                            ? DiffOperation.Equals
                            : DiffOperation.Modify);
                })
        );

        return newAdds.Union(newDeletes).Union(matches).OrderBy(c => c.KeyName).ToList();
    }
}

public class SecretDiffResult
{
    public SecretDiffResult(string keyName, DiffOperation operation)
    {
        KeyName = keyName;
        Operation = operation;
    }

    public string KeyName { get; set; }
    public DiffOperation Operation { get; set; }
}

public enum DiffOperation
{
    Add,
    Delete,
    Equals,
    Modify,
}