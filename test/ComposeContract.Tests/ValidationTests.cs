using ComposeContract.Core;
using ComposeContract.Engine;
using ComposeContract.Env;
using Xunit;

namespace ComposeContract.Tests;

public class ValidationTests
{
    private static void CopyFixture(string name, string dest)
    {
        var src = Path.Combine(FindRepoRoot(), "test", "ComposeContract.Tests", "Fixtures", name);
        foreach (var file in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(src, file);
            var target = Path.Combine(dest, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, true);
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "ComposeContract.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }
        dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "ComposeContract.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new InvalidOperationException("Repo root not found");
    }

    [Fact]
    public void Missing_required_service_returns_SVC_MISSING()
    {
        var dir = Path.Combine(Path.GetTempPath(), "cc-" + Guid.NewGuid().ToString("n"));
        CopyFixture("missing-service", dir);
        var report = new ContractValidator().Validate(new ValidationOptions
        {
            WorkingDirectory = dir,
            ContractPath = "composecontract.yml"
        });
        Assert.Contains(report.Findings, f => f.Code == FindingCodes.SvcMissing && f.Severity == Severity.Error);
        Assert.True(report.HasErrors);
    }

    [Fact]
    public void Missing_required_env_in_example_returns_ENV_REQUIRED_MISSING()
    {
        var dir = Path.Combine(Path.GetTempPath(), "cc-" + Guid.NewGuid().ToString("n"));
        CopyFixture("missing-env", dir);
        var report = new ContractValidator().Validate(new ValidationOptions
        {
            WorkingDirectory = dir,
            ContractPath = "composecontract.yml"
        });
        Assert.Contains(report.Findings, f => f.Code == FindingCodes.EnvRequiredMissing);
        Assert.True(report.HasErrors);
    }

    [Fact]
    public void Local_env_values_never_appear_in_reports()
    {
        var dir = Path.Combine(Path.GetTempPath(), "cc-" + Guid.NewGuid().ToString("n"));
        CopyFixture("override", dir);
        var report = new ContractValidator().Validate(new ValidationOptions
        {
            WorkingDirectory = dir,
            ContractPath = "composecontract.yml"
        });
        var text = ReportFormatter.FormatText(report);
        var json = ReportFormatter.FormatJson(report);
        Assert.DoesNotContain("s3cret", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("s3cret", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password=", text);
    }

    [Fact]
    public void Profile_worker_service_absent_without_flag()
    {
        var dir = Path.Combine(Path.GetTempPath(), "cc-" + Guid.NewGuid().ToString("n"));
        CopyFixture("profiles", dir);
        var report = new ContractValidator().Validate(new ValidationOptions
        {
            WorkingDirectory = dir,
            ContractPath = "composecontract.yml"
        });
        Assert.Contains(report.Findings, f => f.Code == FindingCodes.SvcMissing && f.Message.Contains("worker"));
    }

    [Fact]
    public void Profile_worker_service_present_with_flag()
    {
        var dir = Path.Combine(Path.GetTempPath(), "cc-" + Guid.NewGuid().ToString("n"));
        CopyFixture("profiles", dir);
        var report = new ContractValidator().Validate(new ValidationOptions
        {
            WorkingDirectory = dir,
            ContractPath = "composecontract.yml",
            Profiles = new[] { "default", "worker" }
        });
        Assert.DoesNotContain(report.Findings, f => f.Code == FindingCodes.SvcMissing);
    }

    [Fact]
    public void Override_merges_service_env_keys_and_services()
    {
        var dir = Path.Combine(Path.GetTempPath(), "cc-" + Guid.NewGuid().ToString("n"));
        CopyFixture("override", dir);
        var report = new ContractValidator().Validate(new ValidationOptions
        {
            WorkingDirectory = dir,
            ContractPath = "composecontract.yml"
        });
        Assert.False(report.HasErrors, ReportFormatter.FormatText(report));
    }

    [Fact]
    public void Init_writes_valid_contract_from_compose()
    {
        var dir = Path.Combine(Path.GetTempPath(), "cc-" + Guid.NewGuid().ToString("n"));
        CopyFixture("override", dir);
        File.Delete(Path.Combine(dir, "composecontract.yml"));
        var doc = new InitService().Create(dir, "compose.yml", "compose.override.yml");
        var path = Path.Combine(dir, "composecontract.yml");
        ContractLoader.Save(path, doc);
        Assert.True(File.Exists(path));
        var loaded = ContractLoader.Load(path);
        Assert.Equal(1, loaded.Version);
        Assert.Contains("api", loaded.Rules.Services.Required);
        Assert.Contains("db", loaded.Rules.Services.Required);
    }

    [Fact]
    public void Env_parser_keeps_keys_only()
    {
        var env = EnvParser.Parse("FOO=bar\n# c\nBAZ=super-secret\n");
        Assert.Contains("FOO", env.Keys);
        Assert.Contains("BAZ", env.Keys);
        Assert.Equal(2, env.Keys.Count);
    }
}
