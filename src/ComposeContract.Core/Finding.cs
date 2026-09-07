namespace ComposeContract.Core;

public sealed record Finding(
    string Code,
    Severity Severity,
    string Message,
    string? Path = null);
