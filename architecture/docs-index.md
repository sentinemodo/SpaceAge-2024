# Documentation index

Last updated: 2026-08-29  
Purpose: canonical vendor/spec links for implementers. Summaries only — do not paste manuals into architecture docs.

| Resource | Version / band | Purpose | Date retrieved |
|----------|----------------|---------|----------------|
| [.NET Framework 4.8](https://learn.microsoft.com/en-us/dotnet/framework/whats-new/) | 4.8 | Target runtime for `Game` and `Tests` | 2026-08-16 |
| [C# language reference](https://learn.microsoft.com/en-us/dotnet/csharp/) | C# as supported by VS / net48 compiler | Language surface available to this csproj | 2026-08-16 |
| [Encoding.GetEncoding](https://learn.microsoft.com/en-us/dotnet/api/system.text.encoding.getencoding) | net48 | Code page 1251 (Windows-1251) for XML/orders/reports | 2026-08-16 |
| [Windows-1251 (code page 1251)](https://learn.microsoft.com/en-us/windows/win32/intl/code-page-identifiers) | 1251 | Cyrillic ANSI used by game data | 2026-08-16 |
| [NUnit 4 documentation](https://docs.nunit.org/articles/nunit/intro.html) | 4.1.0 | Assertions, `[TestFixture]`, `[Ignore]`, `--where` filters | 2026-08-16 |
| [NUnit Console](https://docs.nunit.org/articles/nunit/running-tests/Console-Command-Line.html) | 3.18.3 | Cloud runner (`nunit3-console.exe` under Mono, `--inprocess`) | 2026-08-16 |
| [NuGet CLI](https://learn.microsoft.com/en-us/nuget/reference/nuget-exe-cli-reference) | `nuget.exe` under Mono (installed by `.cursor/install.sh`) | Restore packages.config and `nuget install` of the console runner | 2026-08-16 |
| [MSBuild](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild) | VS `msbuild` (Windows) / Mono `xbuild` (cloud) | Compile `SpaceAge.sln` | 2026-08-16 |
| [Mono](https://www.mono-project.com/docs/) | Ubuntu 24.04 `mono-complete` | Cloud CLR substitute for net48 | 2026-08-16 |
| [Cursor environment.json](https://cursor.com/docs/cloud-agent/setup) | current Cursor Cloud | `.cursor/environment.json` → `.cursor/install.sh` (default image, no Dockerfile) | 2026-08-16 |
| [Astro documentation](https://docs.astro.build/en/getting-started/) | current stable (static output) | Public lobby stack ([ADR-0007](adr/ADR-0007-public-campaign-website.md)); not the engine | 2026-08-29 |
| [Astro testing](https://docs.astro.build/en/guides/testing/) | current Astro docs | Official: Vitest (`getViteConfig`, Container API) + Playwright e2e | 2026-08-29 |
| [Vitest](https://vitest.dev/guide/) | current stable, Vite-native | Website unit tests in `website/` | 2026-08-29 |
| [Playwright](https://playwright.dev/docs/intro) | current stable | Website e2e against `astro preview` (Chromium MVP) | 2026-08-29 |
| [Astro deploy: GitHub Pages](https://docs.astro.build/en/guides/deploy/github/) | current Astro docs | Preferred static host for `website/` | 2026-08-29 |
| [GitHub Pages](https://docs.github.com/en/pages) | current | Hosting the Astro `dist/` artifact | 2026-08-29 |
| [Cloudflare Pages](https://developers.cloudflare.com/pages/) | current | Acceptable alternative static host | 2026-08-29 |
| [Atlantis New Origins (atlantis-pbem.com)](https://atlantis-pbem.com/) | live site | IA reference only (not a visual or PHP clone); reviewed for lobby widgets | 2026-08-29 |
| [Overlord / Vincent Archer](https://overlord.sourceforge.net/) | historical | 1998 generic PBEM engine; Overlord then Rise of Heroes lineage | 2026-08-29 |

## In-repo design notes (not vendor docs)

| File | Purpose |
|------|---------|
| `Game/documentation/Basics.txt` | Level-0 technologies always available to units |
| `Game/documentation/Concepts.txt` | Design intent: movement, combat, officers, markets |
| `Game/documentation/Rules.txt` | Player-facing rulebook (Alderson PBEM lineage) |
| `Game/documentation/links.txt` | External astronomy/spaceflight primers |
| `.cursor/install.sh` / `.cursor/run-tests.sh` | Cloud restore/build and test entrypoints (Mono) |
| `architecture/future-work.md` | Deferred modernization backlog (ADR-gated) |
| `designer/` | Game-designer specs: galaxy scale, tech tree, catalog, contracts (not engine) |
| `campaign/` | Live PBEM `data.xml` / `gamein.xml` (not NUnit fixtures) |
| `architecture/delivery/campaign-play.md` | Campaign load, engine TDD slices, CLI play loop, AI factions |
| `architecture/delivery/website.md` | Public lobby implementation plan (Astro, status JSON, visual-tool link); **Cursor agents and test pairing** (`/website-developer` + `/website-tester`) |
| `architecture/delivery/website-scenarios.md` | Seed acceptance catalog (WS-001…WS-009, reserved UT-*). After Phase 1: tester moves to `website/e2e/scenarios.md` (canonical) |
| `Game/documentation/Rules.txt` §§1, 1.1, 2.1 | Home-page flavour source (Alderson V 1.5); excerpt, do not dump |
| `player/rules.md` | Later `/rules` link — live order syntax, not flavour myth |

## In-repo ADRs

| File | Purpose | Date retrieved |
|------|---------|----------------|
| [`adr/ADR-0001-net48-legacy-csproj.md`](adr/ADR-0001-net48-legacy-csproj.md) | Stay on net48 + legacy csproj | 2026-08-18 |
| [`adr/ADR-0002-windows-1251-io.md`](adr/ADR-0002-windows-1251-io.md) | Windows-1251 for game XML/orders/reports | 2026-08-18 |
| [`adr/ADR-0003-filesystem-pbem-batch.md`](adr/ADR-0003-filesystem-pbem-batch.md) | Offline file-in / file-out host | 2026-08-18 |
| [`adr/ADR-0004-test-layers.md`](adr/ADR-0004-test-layers.md) | Unit = `UnitTests` / `T*.cs`; Integration = `IntegrationTests`; no module layer | 2026-08-18 |
| [`adr/ADR-0005-modulestack-partials.md`](adr/ADR-0005-modulestack-partials.md) | `ModuleStack` stays one type; limited `partial` files | 2026-08-18 |
| [`adr/ADR-0006-datafile-facade-and-xml-seams.md`](adr/ADR-0006-datafile-facade-and-xml-seams.md) | `DataFile` facade; catalog / order factory / domain XML phases | 2026-08-18 |
| [`adr/ADR-0007-public-campaign-website.md`](adr/ADR-0007-public-campaign-website.md) | Public website as new bounded context; Astro + status JSON | 2026-08-29 |

Prefer Microsoft Learn / NUnit docs over blog posts when versions matter.
