namespace ComposeContract.Core;

public static class SeverityParser
{
    public static Severity Parse(string? value, Severity fallback = Severity.Error)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        return value.Trim().ToLowerInvariant() switch
        {
            "error" => Severity.Error,
            "warn" or "warning" => Severity.Warn,
            "info" => Severity.Info,
            _ => fallback
        };
    }
}
