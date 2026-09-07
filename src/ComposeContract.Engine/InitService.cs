using ComposeContract.Compose;
using ComposeContract.Core;
using ComposeContract.Env;
using ComposeContract.Options;
namespace ComposeContract.Engine;
public sealed class InitService
{
    public ContractDocument Create(string workingDirectory, string? composePath = null, string? overridePath = null, string? optionsAssembly = null)
    {
        var composeRel = composePath ?? "compose.yml";
        if (!File.Exists(Path.Combine(workingDirectory, composeRel)) && File.Exists(Path.Combine(workingDirectory, "docker-compose.yml"))) composeRel = "docker-compose.yml";
        var doc = new ContractDocument { Version = 1, Project = new ProjectSection { Compose = composeRel, Override = overridePath }, Env = new EnvSection { Example = ".env.example", Local = File.Exists(Path.Combine(workingDirectory, ".env")) ? ".env" : null }, Profiles = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase) { ["default"] = new List<string>() }, Rules = new RulesSection(), Severity = new SeveritySection() };
        var composeFull = Path.Combine(workingDirectory, composeRel);
        if (File.Exists(composeFull))
        {
            try
            {
                var main = ComposeParser.ParseFile(composeFull); ComposeFile? over = null;
                if (!string.IsNullOrWhiteSpace(overridePath)) { var op = Path.Combine(workingDirectory, overridePath); if (File.Exists(op)) over = ComposeParser.ParseFile(op); }
                var merged = ComposeParser.Merge(main, over);
                foreach (var (name, svc) in merged.Services)
                {
                    var profiles = svc.GetProfiles();
                    if (profiles.Count == 0) doc.Rules.Services.Required.Add(name);
                    else foreach (var p in profiles) { if (!doc.Profiles.TryGetValue(p, out var list)) { list = new List<string>(); doc.Profiles[p] = list; } if (!list.Contains(name, StringComparer.OrdinalIgnoreCase)) list.Add(name); }
                    foreach (var key in svc.GetEnvKeys()) if (!doc.Rules.Env.Required.Contains(key, StringComparer.OrdinalIgnoreCase)) doc.Rules.Env.Required.Add(key);
                }
            }
            catch { }
        }
        var examplePath = Path.Combine(workingDirectory, doc.Env.Example);
        if (File.Exists(examplePath)) foreach (var key in EnvParser.ParseFile(examplePath).Keys) if (!doc.Rules.Env.Required.Contains(key, StringComparer.OrdinalIgnoreCase) && !doc.Rules.Env.Optional.Contains(key, StringComparer.OrdinalIgnoreCase)) doc.Rules.Env.Optional.Add(key);
        if (!string.IsNullOrWhiteSpace(optionsAssembly))
        {
            var asmPath = Path.IsPathRooted(optionsAssembly) ? optionsAssembly : Path.Combine(workingDirectory, optionsAssembly);
            if (File.Exists(asmPath)) { doc.Rules.Options.Assemblies.Add(optionsAssembly); foreach (var key in OptionsKeyExtractor.ExtractFromAssembly(asmPath)) if (!doc.Rules.Env.Required.Contains(key, StringComparer.OrdinalIgnoreCase)) doc.Rules.Env.Required.Add(key); }
        }
        return doc;
    }
}
