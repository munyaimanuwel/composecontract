using System.Text;
using System.Text.Json;
using ComposeContract.Core;
namespace ComposeContract.Engine;
public static class ReportFormatter
{
    public static string FormatText(ValidationReport report)
    {
        if (report.Findings.Count == 0) return "OK — no findings.\n";
        var sb = new StringBuilder();
        foreach (var f in report.Findings.OrderByDescending(x => x.Severity).ThenBy(x => x.Code)) { sb.Append(f.Severity.ToString().ToUpperInvariant()); sb.Append(' '); sb.Append(f.Code); sb.Append(": "); sb.Append(f.Message); sb.AppendLine(); }
        return sb.ToString();
    }
    public static string FormatJson(ValidationReport report) => JsonSerializer.Serialize(new { ok = !report.HasErrors, findings = report.Findings.Select(f => new { code = f.Code, severity = f.Severity.ToString().ToLowerInvariant(), message = f.Message, path = f.Path }) }, new JsonSerializerOptions { WriteIndented = true });
}
