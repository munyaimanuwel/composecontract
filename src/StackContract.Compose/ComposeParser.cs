using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
namespace StackContract.Compose;
public static class ComposeParser
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).IgnoreUnmatchedProperties().Build();
    public static ComposeFile ParseFile(string path) => Parse(File.ReadAllText(path));
    public static ComposeFile Parse(string yaml)
    {
        var raw = Deserializer.Deserialize<Dictionary<object, object>>(yaml) ?? new Dictionary<object, object>(); var file = new ComposeFile();
        if (!raw.TryGetValue("services", out var servicesObj) || servicesObj is not Dictionary<object, object> services) return file;
        foreach (var (nameObj, svcObj) in services) { var name = nameObj?.ToString(); if (string.IsNullOrWhiteSpace(name) || svcObj is not Dictionary<object, object> svcMap) continue; var svc = new ComposeService(); if (svcMap.TryGetValue("image", out var image)) svc.Image = image?.ToString(); if (svcMap.TryGetValue("environment", out var env)) svc.Environment = env; if (svcMap.TryGetValue("profiles", out var profiles)) svc.Profiles = profiles; file.Services[name] = svc; }
        return file;
    }
    public static ComposeFile Merge(ComposeFile main, ComposeFile? overrideFile)
    {
        if (overrideFile is null) return main; var merged = new ComposeFile(); foreach (var (name, svc) in main.Services) merged.Services[name] = Clone(svc);
        foreach (var (name, svc) in overrideFile.Services) { if (!merged.Services.TryGetValue(name, out var existing)) { merged.Services[name] = Clone(svc); continue; } var envKeys = existing.GetEnvKeys(); foreach (var k in svc.GetEnvKeys()) envKeys.Add(k); existing.Environment = envKeys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).Select(k => (object)$"{k}=").ToList(); var profiles = existing.GetProfiles(); foreach (var p in svc.GetProfiles()) profiles.Add(p); if (profiles.Count > 0) existing.Profiles = profiles.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).Cast<object>().ToList(); if (!string.IsNullOrWhiteSpace(svc.Image)) existing.Image = svc.Image; }
        return merged;
    }
    private static ComposeService Clone(ComposeService s) => new() { Image = s.Image, Environment = s.Environment, Profiles = s.Profiles, Labels = new Dictionary<string, string>(s.Labels, StringComparer.OrdinalIgnoreCase) };
}
