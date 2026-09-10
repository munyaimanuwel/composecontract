namespace StackContract.Env;

public sealed class EnvFile
{
    /// <summary>Keys only — values are never retained.</summary>
    public HashSet<string> Keys { get; } = new(StringComparer.OrdinalIgnoreCase);
}
