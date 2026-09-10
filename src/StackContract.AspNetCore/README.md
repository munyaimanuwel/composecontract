# StackContract.AspNetCore

Fails ASP.NET Core host start when required configuration keys are missing. Complements `ValidateOnStart`; it does not replace the `stackcontract` CLI.

## Install

```bash
dotnet add package StackContract.AspNetCore
```

## Usage

```csharp
using StackContract.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseStackContractKeyCheck(new[]
{
    "ConnectionStrings__Default",
    "ASPNETCORE_ENVIRONMENT"
});
var app = builder.Build();
```

On start, missing or blank keys throw `InvalidOperationException` and log `ENV_REQUIRED_MISSING`. Disable with `opts => opts.Enabled = false`.

The CLI (`dotnet tool install -g stackcontract`) still checks Compose services and env files in CI. This package only checks **process configuration** at host start.

MIT. Not affiliated with Docker, Inc. Repo: https://github.com/munyaimanuwel/stackcontract
