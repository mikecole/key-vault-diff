using Azure.Security.KeyVault.Secrets;
using cole.key_vault_diff.Secret;
using FluentAssertions;
using Moq;
using tests.Utility;

namespace tests.Secret;

public class SecretCommandTests
{
    private readonly ConsoleWrapperStub _consoleWrapper = new();
    private readonly Mock<ISecretDiffEngine> _secretDiffEngine = new();
    private readonly Mock<ISecretWriter> _secretWriter = new();
    private readonly SecretCommand _sut;

    public SecretCommandTests()
    {
        SetDiffResults(new List<SecretDiffResult>());

        _sut = new SecretCommand(_secretDiffEngine.Object, _consoleWrapper, _secretWriter.Object)
        {
            Source = "source", Destination = "destination"
        };

        _consoleWrapper.ConsoleKeyQueue.Add(ConsoleKey.Q);
    }

    [Fact]
    public async Task RunAsync_ShowsComparingMessage()
    {
        await _sut.RunAsync();
        _consoleWrapper.GetOutput().Should().StartWith("Comparing key vault secrets...");
    }

    [Fact]
    public async Task RunAsync_ShowsMessage_WhenNoResultsToCompare()
    {
        await _sut.RunAsync();
        _consoleWrapper.GetOutput().Should().Contain("There are no items to compare.");
    }

    [Fact]
    public async Task RunAsync_ShowsOutput_WhenRun()
    {
        SetDiffResults(new List<SecretDiffResult>
        {
            new("Add", DiffOperation.Add),
            new("Equals", DiffOperation.Equals),
            new("Delete", DiffOperation.Delete),
            new("Modify", DiffOperation.Modify)
        });

        await _sut.RunAsync();
        _consoleWrapper.GetOutput().Should().Contain(@"Add     +
Delete  -
Equals  =
Modify  ~");
    }

    [Fact]
    public async Task RunAsync_ShowsAddOption_WhenAddsExist()
    {
        SetDiffResults(new List<SecretDiffResult> { new("Add", DiffOperation.Add) });

        await _sut.RunAsync();

        _consoleWrapper.GetOutput().Should().Contain("[A] Add all new secrets to source.vault.azure.net");
    }

    [Fact]
    public async Task RunAsync_HidesAddOption_WhenAddsDoNotExist()
    {
        SetDiffResults(new List<SecretDiffResult> { new("Delete", DiffOperation.Delete) });

        await _sut.RunAsync();

        _consoleWrapper.GetOutput().Should().NotContain("[A] Add all new secrets to source.vault.azure.net");
    }

    [Fact]
    public async Task RunAsync_QuitsWithMessage_WithQInput()
    {
        SetDiffResults(new List<SecretDiffResult> { new("Delete", DiffOperation.Delete) });

        await _sut.RunAsync();

        _consoleWrapper.GetOutput().Should().EndWith("Quitting...");
    }

    [Fact]
    public async Task RunAsync_QuitsWithMessage_WithUnexpectedInput()
    {
        _consoleWrapper.ConsoleKeyQueue = new List<ConsoleKey> { ConsoleKey.B };

        SetDiffResults(new List<SecretDiffResult> { new("Delete", DiffOperation.Delete) });

        await _sut.RunAsync();

        _consoleWrapper.GetOutput().Should().EndWith("Unrecognized. Quitting...");
    }

    [Fact]
    public async Task RunAsync_CreatesNewWithMessage_WithAInput()
    {
        _consoleWrapper.ConsoleKeyQueue = new List<ConsoleKey> { ConsoleKey.A, ConsoleKey.Q };

        SetDiffResults(new List<SecretDiffResult> { new("Foo", DiffOperation.Add), new("Bar", DiffOperation.Add) });

        await _sut.RunAsync();

        _secretWriter.Verify(m => m.CreateSecret(It.Is<SecretClient>(c => c.VaultUri.ToString().Contains("source")),
            It.Is<SecretClient>(c => c.VaultUri.ToString().Contains("destination")), "Foo"));
        _secretWriter.Verify(m => m.CreateSecret(It.Is<SecretClient>(c => c.VaultUri.ToString().Contains("source")),
            It.Is<SecretClient>(c => c.VaultUri.ToString().Contains("destination")), "Bar"));

        var consoleOutput = _consoleWrapper.GetOutput();
        consoleOutput.Should().Contain("Writing Foo...");
        consoleOutput.Should().Contain("Writing Bar...");
    }

    private void SetDiffResults(List<SecretDiffResult> results)
    {
        _secretDiffEngine.Setup(mock => mock.GetDiff(
                It.Is<SecretClient>(s => s.VaultUri.ToString().Equals("https://source.vault.azure.net/")),
                It.Is<SecretClient>(s => s.VaultUri.ToString().Equals("https://destination.vault.azure.net/"))))
            .ReturnsAsync(results);
    }
}
