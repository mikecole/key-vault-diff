namespace tests.Utility;

public class ConsoleOutput : IDisposable
{
    private readonly StringWriter _stringWriter;
    private readonly TextWriter _originalOutput;
    private readonly TextReader _originalInput;
    private readonly StringReader _stringReader;

    public ConsoleOutput()
    {
        _stringWriter = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_stringWriter);

        _stringReader = new StringReader("");
        _originalInput = Console.In;
        Console.SetIn(_stringReader);
    }

    public string GetOuput()
    {
        return _stringWriter.ToString();
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _stringWriter.Dispose();
    }
}
