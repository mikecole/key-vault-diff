namespace cole.key_vault_diff;

public interface IConsoleWrapper
{
    ConsoleKeyInfo ReadKey();
    void Write(string? value);
    void WriteLine(string? value);
    void WriteLine();
}

public class ConsoleWrapper : IConsoleWrapper
{
    public ConsoleKeyInfo ReadKey()
    {
        return Console.ReadKey(true);
    }

    public void Write(string? value)
    {
        Console.Write(value);
    }

    public void WriteLine(string? value)
    {
        Console.WriteLine(value);
    }

    public void WriteLine()
    {
        Console.WriteLine();
    }
}