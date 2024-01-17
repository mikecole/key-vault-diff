//Setup DI for SecretDiffEngine
//Setup tests for console tests
//Test for: mocked operations show proper sign
//  mocked key names space correctly on console
//Future:
//  Test operation to view contents of modified values, with confirmation
//  Test to actually make the changes
//Bugs:
// Secret clients returns certs as well? strange.

using System.CommandLine.Builder;
using cole.key_vault_diff;
using cole.key_vault_diff.Secret;
using DotMake.CommandLine;
using Microsoft.Extensions.DependencyInjection;

Cli.Ext.ConfigureServices(services =>
{
    services.AddTransient<ISecretDiffEngine, SecretDiffEngine>();
});
        
//Do I need this extra dependency injection stuff?
await Cli.RunAsync<RootCommand>(args, builder => builder.UseDefaults().UseDependencyInjection(services =>
{
    services.AddSingleton<ISecretDiffEngine, SecretDiffEngine>();
}));