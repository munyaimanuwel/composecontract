using StackContract.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace StackContract.AspNetCore;
public static class StackContractHostExtensions
{
    public static IHostBuilder UseStackContractKeyCheck(this IHostBuilder host, IEnumerable<string> requiredKeys, Action<StackContractKeyCheckOptions>? configure = null)
    {
        var opts = new StackContractKeyCheckOptions(); configure?.Invoke(opts);
        return host.ConfigureServices((ctx, services) => { services.AddSingleton(opts); services.AddSingleton(new RequiredKeys(requiredKeys.ToArray())); services.AddHostedService<StackContractKeyCheckService>(); });
    }
}
public sealed class StackContractKeyCheckOptions { public bool Enabled { get; set; } = true; }
internal sealed record RequiredKeys(IReadOnlyList<string> Keys);
internal sealed class StackContractKeyCheckService : IHostedService
{
    private readonly IConfiguration _config; private readonly RequiredKeys _keys; private readonly StackContractKeyCheckOptions _options; private readonly ILogger<StackContractKeyCheckService> _logger;
    public StackContractKeyCheckService(IConfiguration config, RequiredKeys keys, StackContractKeyCheckOptions options, ILogger<StackContractKeyCheckService> logger) { _config = config; _keys = keys; _options = options; _logger = logger; }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled) return Task.CompletedTask; var missing = new List<string>(); foreach (var key in _keys.Keys) { var value = _config[key]; if (string.IsNullOrWhiteSpace(value)) missing.Add(key); }
        if (missing.Count == 0) return Task.CompletedTask; var msg = $"StackContract: missing required configuration keys: {string.Join(", ", missing)}"; _logger.LogCritical("{Code}: {Message}", FindingCodes.EnvRequiredMissing, msg); throw new InvalidOperationException(msg);
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
