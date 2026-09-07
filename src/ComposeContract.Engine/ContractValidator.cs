using ComposeContract.Compose;
using ComposeContract.Core;
using ComposeContract.Env;

namespace ComposeContract.Engine;

public sealed class ContractValidator
{
    public ValidationReport Validate(ValidationOptions options)
    {
        var report = new ValidationReport();
        var root = options.WorkingDirectory;
        var contractPath = Path.GetFullPath(Path.Combine(root, options.ContractPath));
        ContractDocument contract;
        try
        {
            if (!File.Exists(contractPath))
            {
                report.Findings.Add(new Finding(FindingCodes.ContractInvalid, Severity.Error, $"Contract file not found: {options.ContractPath}", options.ContractPath));
                return ApplyStrict(report, options.Strict);
            }
            contract = ContractLoader.Load(contractPath);
        }
        catch (Exception ex)
        {
            report.Findings.Add(new Finding(FindingCodes.ContractInvalid, Severity.Error, $"Failed to parse contract: {ex.Message}", options.ContractPath));
            return ApplyStrict(report, options.Strict);
        }
        var contractDir = Path.GetDirectoryName(contractPath) ?? root;
        var sev = contract.Severity;
        var requested = options.Profiles.Count > 0 ? options.Profiles.ToList() : new List<string> { "default" };
        foreach (var p in requested)
            if (!contract.Profiles.ContainsKey(p) && !string.Equals(p, "default", StringComparison.OrdinalIgnoreCase))
                report.Findings.Add(new Finding(FindingCodes.ProfileUnknown, Severity.Error, $"Unknown profile '{p}'.", options.ContractPath));
        ComposeFile? merged = null;
        var composeRel = contract.Project.Compose;
        var composePath = Path.GetFullPath(Path.Combine(contractDir, composeRel));
        try
        {
            if (!File.Exists(composePath)) report.Findings.Add(new Finding(FindingCodes.ComposeParse, Severity.Error, $"Compose file not found: {composeRel}", composeRel));
            else
            {
                var main = ComposeParser.ParseFile(composePath);
                ComposeFile? over = null;
                if (!string.IsNullOrWhiteSpace(contract.Project.Override))
                {
                    var overPath = Path.GetFullPath(Path.Combine(contractDir, contract.Project.Override));
                    if (File.Exists(overPath)) over = ComposeParser.ParseFile(overPath);
                }
                merged = ComposeParser.Merge(main, over);
            }
        }
        catch (Exception ex) { report.Findings.Add(new Finding(FindingCodes.ComposeParse, Severity.Error, $"Failed to parse compose: {ex.Message}", composeRel)); }
        if (merged is not null)
        {
            var activeProfiles = new HashSet<string>(requested, StringComparer.OrdinalIgnoreCase);
            var activeServices = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (name, svc) in merged.Services)
            {
                var svcProfiles = svc.GetProfiles();
                if (svcProfiles.Count == 0 || svcProfiles.Overlaps(activeProfiles)) activeServices.Add(name);
            }
            foreach (var profileName in activeProfiles)
                if (contract.Profiles.TryGetValue(profileName, out var listed))
                    foreach (var s in listed) if (merged.Services.ContainsKey(s)) activeServices.Add(s);
            var missingSev = SeverityParser.Parse(sev.MissingService, Severity.Error);
            foreach (var required in contract.Rules.Services.Required)
            {
                if (!merged.Services.TryGetValue(required, out var svcDef)) report.Findings.Add(new Finding(FindingCodes.SvcMissing, missingSev, $"Required service '{required}' is missing from compose.", composeRel));
                else if (svcDef.GetProfiles().Count > 0 && !svcDef.GetProfiles().Overlaps(activeProfiles)) report.Findings.Add(new Finding(FindingCodes.SvcMissing, missingSev, $"Required service '{required}' is not active for profile(s): {string.Join(',', activeProfiles)}.", composeRel));
            }
        }
        var exampleRel = options.EnvExamplePath ?? contract.Env.Example;
        var examplePath = Path.GetFullPath(Path.Combine(contractDir, exampleRel));
        var exampleKeys = File.Exists(examplePath) ? EnvParser.ParseFile(examplePath).Keys : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(examplePath) && contract.Rules.Env.Required.Count > 0) report.Findings.Add(new Finding(FindingCodes.EnvRequiredMissing, SeverityParser.Parse(sev.MissingEnvRequired), $"Env example file not found: {exampleRel}", exampleRel));
        var reqSev = SeverityParser.Parse(sev.MissingEnvRequired, Severity.Error);
        foreach (var key in contract.Rules.Env.Required) if (!exampleKeys.Contains(key)) report.Findings.Add(new Finding(FindingCodes.EnvRequiredMissing, reqSev, $"Required env key '{key}' missing from {exampleRel}.", exampleRel));
        var optSev = SeverityParser.Parse(sev.MissingEnvOptional, Severity.Warn);
        foreach (var key in contract.Rules.Env.Optional) if (!exampleKeys.Contains(key)) report.Findings.Add(new Finding(FindingCodes.EnvOptionalMissing, optSev, $"Optional env key '{key}' missing from {exampleRel}.", exampleRel));
        var known = new HashSet<string>(contract.Rules.Env.Required, StringComparer.OrdinalIgnoreCase);
        foreach (var k in contract.Rules.Env.Optional) known.Add(k);
        var unkSev = SeverityParser.Parse(sev.UnknownEnvInExample, Severity.Warn);
        foreach (var key in exampleKeys) if (!known.Contains(key) && known.Count > 0) report.Findings.Add(new Finding(FindingCodes.EnvExampleUnknown, unkSev, $"Env key '{key}' in {exampleRel} is not listed in contract rules.", exampleRel));
        var localRel = options.EnvLocalPath ?? contract.Env.Local;
        if (!string.IsNullOrWhiteSpace(localRel))
        {
            var localPath = Path.GetFullPath(Path.Combine(contractDir, localRel));
            if (File.Exists(localPath))
            {
                var localKeys = EnvParser.ParseFile(localPath).Keys;
                foreach (var key in contract.Rules.Env.Required) if (!localKeys.Contains(key)) report.Findings.Add(new Finding(FindingCodes.EnvRequiredMissing, reqSev, $"Required env key '{key}' missing from local env file '{localRel}'.", localRel));
            }
        }
        return ApplyStrict(report, options.Strict);
    }
    private static ValidationReport ApplyStrict(ValidationReport report, bool strict)
    {
        if (!strict) return report;
        for (var i = 0; i < report.Findings.Count; i++) if (report.Findings[i].Severity == Severity.Warn) report.Findings[i] = report.Findings[i] with { Severity = Severity.Error };
        return report;
    }
}
