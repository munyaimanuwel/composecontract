namespace ComposeContract.Compose;
public sealed class ComposeFile { public Dictionary<string, ComposeService> Services { get; set; } = new(StringComparer.OrdinalIgnoreCase); }
public sealed class ComposeService
{
    public string? Image { get; set; } public object? Environment { get; set; } public object? Profiles { get; set; } public Dictionary<string, string> Labels { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> GetEnvKeys()
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase); if (Environment is null) return keys;
        if (Environment is Dictionary<object, object> map) foreach (var kv in map) { var k = kv.Key?.ToString(); if (!string.IsNullOrWhiteSpace(k)) keys.Add(k); }
        else if (Environment is List<object> list) foreach (var item in list) { var s = item?.ToString(); if (string.IsNullOrWhiteSpace(s)) continue; var eq = s.IndexOf('='); keys.Add(eq > 0 ? s[..eq] : s); }
        return keys;
    }
    public HashSet<string> GetProfiles()
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase); if (Profiles is null) return set;
        if (Profiles is string one) { if (!string.IsNullOrWhiteSpace(one)) set.Add(one); return set; }
        if (Profiles is List<object> list) foreach (var item in list) { var s = item?.ToString(); if (!string.IsNullOrWhiteSpace(s)) set.Add(s); }
        return set;
    }
}
