# SpaceAge-2024 — technology choices

Last updated: 2026-08-29  
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

### NuGet packages (`Tests`; `Game` has none)

Pinned in `Tests/packages.config`; HintPaths `..\packages\{id}.{version}\lib\net462\...`. `Game` restores an empty `packages.config` and must stay free of test-only references.

| Package | Version | Role |
|---------|---------|------|
| NUnit | **4.1.0** | `NUnit.Framework` (and `nunit.framework.legacy` as pulled by 4.x) — `Tests` only |
| NUnit3TestAdapter | 4.6.0 | VS / vstest discovery — `Tests` only |
| NUnit.Analyzers | 4.3.0 | Dev-time analyzers — `Tests` only |
| System.Runtime.CompilerServices.Unsafe | 6.0.0 | Transitive for NUnit 4 on net48 |
| System.Threading.Tasks.Extensions | 4.5.4 | Transitive for NUnit 4 on net48 |

**Never** restore NUnit 2 (`NUnit.Core`) or machine-local HintPaths. Do not add test-only packages to `Game` without an ADR.

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

## Website (new, not engine)

This section does **not** replace the net48 stack above. The public lobby is a separate product ([ADR-0007](adr/ADR-0007-public-campaign-website.md), plan [`delivery/website.md`](delivery/website.md)). Do not retarget `Game.exe`, add ASP.NET to the engine, or build the site from `SpaceAge.sln`.

| Area | Choice | Constraint |
|------|--------|------------|
| Location | `website/` at repo root | Never under `Game/` or `Tests/` |
| Framework | **Astro**, static output (`output: 'static'`) | Current stable from npm (`npm create astro@latest`); no SSR adapter in MVP |
| Language | HTML/Astro templates + CSS; TypeScript only for small islands | Do not import C# or parse 1251 game XML in Node. Phase 4 may parse **user-pasted UTF-8 text** in the browser (`/eta`) |
| Styling | Hand-written CSS | Space / hard-science look; no Atlantis hex theme; Tailwind optional, not required |
| Status data | UTF-8 `website/public/status.json` | Written by GM or `play/` PowerShell; not an engine REST API |
| Hosting | GitHub Pages (preferred); Cloudflare Pages or Netlify OK | GM does not run PHP or IIS |
| Tests | **`astro check`** + **Vitest** (`getViteConfig`) + **Playwright** (Chromium vs `astro preview`) | Inside `website/` only. Not NUnit, not `Tests.dll`, not Mono. [Astro testing](https://docs.astro.build/en/guides/testing/). Cursor agents are **paired**: `/website-developer` (Vitest) + `/website-tester` (scenario catalog + Playwright); see [`delivery/website.md`](delivery/website.md) |
| Visual tool | Link only (`/client` → `/visual-tool/` placeholder) | Separate future app; may use React later |
| Phase 4 tools | `/eta`, `/battle` TypeScript islands | Port published formulas (`SpaceTransit`, `au-transit.md`, `player/battle.md`). No `Game.exe`, no live `data.xml` |

Rejected for the lobby: PHP/Laravel Atlantis clone, ASP.NET on net48, Next.js static export (heavier than four pages), embedding UI in `Game.exe`, a CMS.

Engine XML/orders stay Windows-1251. Website source and `status.json` are UTF-8.

## Revision

- 2026-08-29: Added **Website (new, not engine)**. Engine pins unchanged.
- 2026-08-29: Website tests = Vitest + Playwright; engine stays NUnit.
- 2026-08-29: Website Cursor agents paired (`/website-developer` + `/website-tester`).
- 2026-08-29: Phase 4 `/eta` and `/battle` islands noted; engine pins unchanged.
