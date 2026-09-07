namespace ComposeContract.Engine;

public sealed class ValidationOptions
{
    public string ContractPath { get; init; } = "composecontract.yml";
    public IReadOnlyList<string> Profiles { get; init; } = Array.Empty<string>();
    public string? EnvExamplePath { get; init; }
    public string? EnvLocalPath { get; init; }
    public bool Strict { get; init; }
    public string WorkingDirectory { get; init; } = Directory.GetCurrentDirectory();
}
