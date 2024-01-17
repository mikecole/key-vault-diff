using Azure.Security.KeyVault.Secrets;
using cole.key_vault_diff.Secret;
using FluentAssertions;
using Moq;
using tests.Utility;

namespace tests.Secret;

public class SecretCommandTests
{
    private readonly SecretCommand _sut;
    private readonly Mock<ISecretDiffEngine> _secretDiffEngine = new();

    public SecretCommandTests()
    {
        SetDiffResults(new List<SecretDiffResult>());

        _sut = new (_secretDiffEngine.Object)
        {
            Source = "source",
            Destination = "destination"
        };
    }
    
    [Fact]
    public async Task RunAsync_ShowsComparingMessage()
    {
        using var consoleOutput = new ConsoleOutput();
        await _sut.RunAsync();
        consoleOutput.GetOuput().Should().StartWith("Comparing key vault secrets...");
    }
    
    [Fact]
    public async Task RunAsync_ShowsMessage_WhenNoResultsToCompare()
    {
        using var consoleOutput = new ConsoleOutput();
        await _sut.RunAsync();
        consoleOutput.GetOuput().Should().Contain("There are no items to compare.");
    }
    
    [Fact]
    public async Task RunAsync_ShowsOutput_WhenRun()
    {
        using var consoleOutput = new ConsoleOutput();
        
        SetDiffResults(new List<SecretDiffResult>
        {
            new("Add", DiffOperation.Add),
            new("Equals", DiffOperation.Equals),
            new("Delete", DiffOperation.Delete),
            new("Modify", DiffOperation.Modify),
        });

        await _sut.RunAsync();
        consoleOutput.GetOuput().Should().Contain(@"Add     +
Delete  -
Equals  =
Modify  ~");
    }

    [Fact]
    public async Task RunAsync_ShowsAddOption_WhenAddsExist()
    {
        using var consoleOutput = new ConsoleOutput();
        
        SetDiffResults(new List<SecretDiffResult>
        {
            new("Add", DiffOperation.Add)
        });
        
        await _sut.RunAsync();

        consoleOutput.GetOuput().Should().Contain("[A] Add all new secrets to source.vault.azure.net");
    }
    
    [Fact]
    public async Task RunAsync_HidesAddOption_WhenAddsDoNotExist()
    {
        using var consoleOutput = new ConsoleOutput();
        
        SetDiffResults(new List<SecretDiffResult>
        {
            new("Delete", DiffOperation.Delete)
        });
        
        await _sut.RunAsync();

        consoleOutput.GetOuput().Should().NotContain("[A] Add all new secrets to source.vault.azure.net");
    }

    private void SetDiffResults(List<SecretDiffResult> results)
    {
        _secretDiffEngine.Setup(mock => mock.GetDiff(
                It.Is<SecretClient>(s => s.VaultUri.ToString().Equals("https://source.vault.azure.net/")),
                It.Is<SecretClient>(s => s.VaultUri.ToString().Equals("https://destination.vault.azure.net/"))))
            .ReturnsAsync(results);
    }
}
