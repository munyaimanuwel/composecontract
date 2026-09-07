# ComposeContract

Local-first **Compose / config contract checker** for .NET. Catch missing services and env keys before runtime — no Docker daemon required for `validate`.

MIT licensed. Open-core: the engine stays free forever with **zero network** in Core/Engine.

## 5-minute path

```bash
# requires .NET 8 SDK
dotnet pack ComposeContract.sln -c Release -o ./nupkg
dotnet tool install --add-source ./nupkg -g composecontract --version 0.1.0

cd samples/aspnet-compose
composecontract init --compose compose.yml --force
composecontract validate --strict
```

Exit codes: `0` ok/warns · `1` any error · `2` usage/parse. Use `--strict` in CI to promote warnings to errors.

## CLI

```
composecontract init [--compose PATH] [--override PATH] [--options ASSEMBLY] [--force]
composecontract validate [--contract PATH] [--profile NAME]* [--env-example PATH] [--env-local PATH] [--format text|json] [--strict]
composecontract version
```

Finding codes: `SVC_MISSING`, `ENV_REQUIRED_MISSING`, `ENV_OPTIONAL_MISSING`, `ENV_EXAMPLE_UNKNOWN`, `COMPOSE_PARSE`, `CONTRACT_INVALID`, `PROFILE_UNKNOWN`.

Local `.env` is checked for **key presence only** — values never appear in text or JSON reports.

## AspNetCore

`ComposeContract.AspNetCore` fails host start when required configuration keys are missing (complements `ValidateOnStart`).

## GitHub Action

See [`action/action.yml`](action/action.yml) — packs/installs the tool and runs `validate --strict`.

## What it is not

Not a Compose emulator, vault product, K8s/Helm checker, or SaaS. No phone-home in the engine.
