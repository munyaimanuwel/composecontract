namespace StackContract.Core;

public sealed class ValidationReport
{
    public List<Finding> Findings { get; } = new();
    public bool HasErrors => Findings.Any(f => f.Severity == Severity.Error);
    public bool HasWarnings => Findings.Any(f => f.Severity == Severity.Warn);
}
