using ComposeContract.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace ComposeContract.AspNetCore;
public static class ComposeContractHostExtensions
{
    public static IHostBuilder UseComposeContractKeyCheck(this IHostBuilder host, IEnumerable<string> requiredKeys, Action<ComposeContractKeyCheckOptions>? configure = null)
    {
        var opts = new ComposeContractKeyCheckOptions(); configure?.Invoke(opts);
        return host.ConfigureServices((ctx, services) => { services.AddSingleton(opts); services.AddSingleton(new RequiredKeys(requiredKeys.ToArray())); services.AddHostedService<ComposeContractKeyCheckService>(); });
    }
}
public sealed class ComposeContractKeyCheckOptions { public bool Enabled { get; set; } = true; }
internal sealed record RequiredKeys(IReadOnlyList<string> Keys);
internal sealed class ComposeContractKeyCheckService : IHostedService
{
    private readonly IConfiguration _config; private readonly RequiredKeys _keys; private readonly ComposeContractKeyCheckOptions _options; private readonly ILogger<ComposeContractKeyCheckService> _logger;
    public ComposeContractKeyCheckService(IConfiguration config, RequiredKeys keys, ComposeContractKeyCheckOptions options, ILogger<ComposeContractKeyCheckService> logger) { _config = config; _keys = keys; _options = options; _logger = logger; }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled) return Task.CompletedTask; var missing = new List<string>(); foreach (var key in _keys.Keys) { var value = _config[key]; if (string.IsNullOrWhiteSpace(value)) missing.Add(key); }
        if (missing.Count == 0) return Task.CompletedTask; var msg = $"ComposeContract: missing required configuration keys: {string.Join(", ", missing)}"; _logger.LogCritical("{Code}: {Message}", FindingCodes.EnvRequiredMissing, msg); throw new InvalidOperationException(msg);
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
