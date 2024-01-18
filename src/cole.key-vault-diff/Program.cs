using cole.key_vault_diff;
using cole.key_vault_diff.Secret;
using DotMake.CommandLine;
using Microsoft.Extensions.DependencyInjection;

Cli.Ext.ConfigureServices(services =>
{
    services.AddTransient<ISecretDiffEngine, SecretDiffEngine>();
});

await Cli.RunAsync<RootCommand>();