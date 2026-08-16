# SpaceAge-2024 — technology choices

Last updated: 2026-08-16  
Engine version: `Program.EngineVersion` = `0.1.137`

This is a **legacy console engine**, not a service stack. Choices below describe what the repo already uses. Changing the runtime or project style requires an ADR.

## Preferred stack (current)

### Application

| Area | Choice | Constraint |
|------|--------|------------|
| Language | C# | Existing idioms; nullable/`required`/records only where the net48 compiler and neighboring code already allow it |
| Runtime (Windows) | **.NET Framework 4.8** (`TargetFrameworkVersion` v4.8) | Do not retarget to net8 / SDK-style unless asked |
| Project files | Legacy non-SDK `.csproj` (ToolsVersion 12.0) | Do not convert |
| Runtime (Cursor Cloud) | **Mono** on Ubuntu 24.04 | `.cursor/environment.json` → `.cursor/install.sh` (default Ubuntu image; Mono installed by the script, no custom Dockerfile) |
| I18n on Mono | `mono-complete` | Provides code page 1251 for `Encoding.GetEncoding(1251)` (verified end-to-end); no separate i18n package needed |
| Persistence | XML + text files | No SQL, no ORM |
| Encoding | Windows-1251 (code page 1251) | All game XML, orders, reports, `error.log` |

Why stay on net48 / non-SDK:

- Matches the historical engine and Visual Studio workflow.
- Cloud agents build the same solution under Mono via `.cursor/install.sh`.
- A framework jump is a product-wide migration, not a local refactor (tracked in [`future-work.md`](future-work.md)).

### Build and packages

| Tool | Version / source |
|------|------------------|
| NuGet restore | `nuget.exe restore SpaceAge.sln` under Mono (packages.config); default nuget.org feed, no committed `nuget.config` |
| Windows build | `msbuild SpaceAge.sln /p:Configuration=Debug` |
| Cloud build | `xbuild` via `.cursor/install.sh` |
| Test runner (Windows) | Visual Studio NUnit 3 adapter or `vstest.console Tests\bin\Debug\Tests.dll` |
| Test runner (Cloud) | NUnit Console **3.18.3** under `.cursor/tools/`, invoked with `mono --inprocess` (`.cursor/run-tests.sh`) |

### NuGet packages (both `Game` and `Tests`)

Pinned in `packages.config`; HintPaths `..\packages\{id}.{version}\lib\net462\...`.

| Package | Version | Role |
|---------|---------|------|
| NUnit | **4.1.0** | `NUnit.Framework` (and `nunit.framework.legacy` as pulled by 4.x) |
| NUnit3TestAdapter | 4.6.0 | VS / vstest discovery |
| NUnit.Analyzers | 4.3.0 | Dev-time analyzers |
| System.Runtime.CompilerServices.Unsafe | 6.0.0 | Transitive for NUnit 4 on net48 |
| System.Threading.Tasks.Extensions | 4.5.4 | Transitive for NUnit 4 on net48 |

**Quirk:** `Game.csproj` references NUnit. Do not add more test-only packages to the engine without an ADR. Prefer keeping production free of NUnit if a future cleanup is explicitly requested.

**Never** restore NUnit 2 (`NUnit.Core`) or machine-local HintPaths.

### Observability

- Console progress lines in `Program.Main`.
- RELEASE: uncaught exceptions appended to `error.log` (1251), `ExitCode = 1`.
- No OpenTelemetry, no hosted logging framework.

SonarQube helper scripts (`Sonar.bat`, `sonar-project.properties`) exist locally; they are **not** the cloud CI path.

## Alternatives and trade-offs (explicitly not chosen)

| Area | Current | Alternative | Why not now |
|------|---------|-------------|-------------|
| Runtime | net48 + Mono | .NET 8 SDK-style | Large csproj/test/cloud rewrite; out of scope |
| Persistence | XML files | SQLite / JSON | Breaks PBEM GM workflow and 1251 reports |
| Tests | One `Tests.dll`, NUnit 4 | xUnit + `dotnet test` | net48 + Mono console path already works |
| Isolation | Static `*.All` | DI / per-game containers | Pervasive; needs a dedicated ADR and test rewrite |

## Version guidance

- Engine string: bump `Program.EngineVersion` when behavior visible to players/GMs changes; echo it in reports.
- NUnit: stay on **4.1.x** until an ADR + full `.cursor/run-tests.sh` pass on Mono.
- NUnit Console runner: **3.18.3** as installed by `.cursor/install.sh`.
- Mono: whatever `mono-complete` on Ubuntu 24.04 provides (installed by `.cursor/install.sh`); do not add a second CLR in the cloud image.
