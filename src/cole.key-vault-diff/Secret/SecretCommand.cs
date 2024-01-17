using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DotMake.CommandLine;

namespace cole.key_vault_diff.Secret;

[CliCommand(Description = "Secret sub-command", Parent = typeof(RootCommand))]
public class SecretCommand
{
    private readonly ISecretDiffEngine _engine;

    public SecretCommand(ISecretDiffEngine engine)
    {
        _engine = engine;
    }

    [CliOption(Description = "Source Key Vault Name", Required = false)]
    public string? Source { get; set; }
    
    [CliOption(Description = "Destination Key Vault Name", Required = false)]
    public string? Destination { get; set; }
    
    public async Task RunAsync()
    {
        if (string.IsNullOrWhiteSpace(Source))
        {
            throw new ArgumentNullException(nameof(Source));
        }
        
        if (string.IsNullOrWhiteSpace(Destination))
        {
            throw new ArgumentNullException(nameof(Destination));
        }
        
        Console.WriteLine("Comparing key vault secrets...");

        var source = new SecretClient(new Uri($"https://{Source}.vault.azure.net/"), new DefaultAzureCredential());
        var destination = new SecretClient(new Uri($"https://{Destination}.vault.azure.net/"), new DefaultAzureCredential());
            
        var results = await _engine.GetDiff(source, destination);

        if (!results.Any())
        {
            Console.WriteLine("There are no items to compare.");
            return;
        }

        results = results.OrderBy(c => c.KeyName).ToList();

        var maxKeyNameLength = results
            .Select(c => c.KeyName.Length)
            .Max();
        
        foreach (var result in results)
        {
            Console.Write(result.KeyName.PadRight(maxKeyNameLength + 2));

            switch (result.Operation)
            {
                case DiffOperation.Add:
                    Console.Write("+");
                    break;
                case DiffOperation.Delete:
                    Console.Write("-");
                    break;
                case DiffOperation.Equals:
                    Console.Write("=");
                    break;
                case DiffOperation.Modify:
                    Console.Write("~");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            Console.WriteLine();
        }
        
        Console.WriteLine("Options:");

        if (results.Any(r => r.Operation == DiffOperation.Add))
        {
            Console.WriteLine($"[A] Add all new secrets to {destination.VaultUri.Host}");
        }
        Console.WriteLine("[Q] Quit");

        var input = Console.ReadKey(true);

        switch (input.Key)
        {
            case ConsoleKey.Q:
                Console.WriteLine("Quitting...");
                break;
            case ConsoleKey.A:
                Console.WriteLine("Adding...");
                foreach (var secret in results.Where(r => r.Operation == DiffOperation.Add))
                {
                    Console.WriteLine($"Writing {secret.KeyName}...");
                    var value = await source.GetSecretAsync(secret.KeyName);
                    await destination.SetSecretAsync(secret.KeyName, value.Value.Value);
                }
                await RunAsync();
                break;
            default:
                Console.WriteLine("Unrecognized. Quitting...");
                break;
        }
    }
}