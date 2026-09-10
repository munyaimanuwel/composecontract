# StackContract

Local-first **.NET stack/config contract checker**. Catch missing services and env keys before runtime — no Docker daemon required for `validate`.

Not affiliated with Docker, Inc. Referential mentions of Docker Compose in prose describe the compose-file format this tool reads.

MIT licensed. Open-core: the engine stays free forever with **zero network** in Core/Engine.

## Migration from ComposeContract

- CLI tool: `composecontract` → `stackcontract`
- Contract file: `composecontract.yml` → `stackcontract.yml` (legacy `composecontract.yml` still accepted as a deprecated alias when the new default is missing)
- Packages / namespaces: `ComposeContract.*` → `StackContract.*`

## 5-minute path

```bash
# requires .NET 8 SDK
dotnet pack StackContract.sln -c Release -o ./nupkg
dotnet tool install --add-source ./nupkg -g stackcontract --version 0.1.0

cd samples/aspnet-compose
stackcontract init --compose compose.yml --force
stackcontract validate --strict
```

Exit codes: `0` ok/warns · `1` any error · `2` usage/parse. Use `--strict` in CI to promote warnings to errors.

## CLI

```
stackcontract init [--compose PATH] [--override PATH] [--options ASSEMBLY] [--force]
stackcontract validate [--contract PATH] [--profile NAME]* [--env-example PATH] [--env-local PATH] [--format text|json] [--strict]
stackcontract version
```

Finding codes: `SVC_MISSING`, `ENV_REQUIRED_MISSING`, `ENV_OPTIONAL_MISSING`, `ENV_EXAMPLE_UNKNOWN`, `COMPOSE_PARSE`, `CONTRACT_INVALID`, `PROFILE_UNKNOWN`.

Local `.env` is checked for **key presence only** — values never appear in text or JSON reports.

## AspNetCore

`StackContract.AspNetCore` fails host start when required configuration keys are missing (complements `ValidateOnStart`).

## GitHub Action

See [`action/action.yml`](action/action.yml) — packs/installs the tool and runs `validate --strict`.

## Sponsors / Pro

MIT core (CLI, engine, libraries, GitHub Action) stays free and local-first — **zero network** in Core/Engine.

**StackContract Pro Kit** (after the tool is on NuGet): paid templates, CI extras, and kits. Those files are not in this repository.

Pricing and delivery will be linked here when the Polar product is live.

## What it is not

Not a Compose emulator, vault product, K8s/Helm checker, or SaaS. No phone-home in the engine.
