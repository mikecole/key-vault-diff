using System.Text;
using cole.key_vault_diff;

namespace tests.Utility;

public class ConsoleWrapperStub : IConsoleWrapper
{
    private readonly StringBuilder _output = new();

    public List<ConsoleKey> ConsoleKeyQueue { get; set; } = new();

    public ConsoleKeyInfo ReadKey()
    {
        if (!ConsoleKeyQueue.Any()) throw new Exception("No input keys available.");

        var result = ConsoleKeyQueue.First();
        ConsoleKeyQueue.Remove(result);
        return new ConsoleKeyInfo((char)result, result, false, false, false);
    }

    public void Write(string? value)
    {
        _output.Append(value);
    }

    public void WriteLine(string? value)
    {
        _output.AppendLine(value);
    }

    public void WriteLine()
    {
        _output.AppendLine();
    }

    public string GetOutput()
    {
        return _output.ToString();
    }
}