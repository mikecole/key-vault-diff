using DotMake.CommandLine;

namespace cole.key_vault_diff;

[CliCommand(Description = "Root command")]
public class RootCommand
{
    public void Run()
    {
        Console.WriteLine($@"Handler for '{GetType().FullName}' is run: Root command");
        Console.WriteLine();
    }
}
