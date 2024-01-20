using cole.key_vault_diff;
using cole.key_vault_diff.Secret;
using DotMake.CommandLine;
using Microsoft.Extensions.DependencyInjection;

Cli.Ext.ConfigureServices(services =>
{
    services.AddTransient<IConsoleWrapper, ConsoleWrapper>();
    services.AddTransient<ISecretDiffEngine, SecretDiffEngine>();
    services.AddTransient<ISecretWriter, SecretWriter>();
});

await Cli.RunAsync<RootCommand>();
