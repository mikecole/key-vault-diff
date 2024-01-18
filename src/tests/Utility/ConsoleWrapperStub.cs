using System.Text;
using cole.key_vault_diff;

namespace tests.Utility;

public class ConsoleWrapperStub : IConsoleWrapper
{
    private readonly StringBuilder _output = new();
    
    public ConsoleKeyInfo ReadKey()
    {
        throw new NotImplementedException();
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
