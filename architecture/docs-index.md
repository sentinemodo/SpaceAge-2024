# Documentation index

Last updated: 2026-09-09  
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
| [Ollama](https://ollama.com/) / [Ollama docs](https://docs.ollama.com/) | current | Local / pod OpenAI-compatible inference for player-agent ([ADR-0009](adr/ADR-0009-local-llm-player-agent.md)) | 2026-09-09 |
| [RunPod](https://www.runpod.io/) / [RunPod docs](https://docs.runpod.io/) | current | Approved rented GPU host (RTX 4090 + Ollama); re-check $/hr before sessions | 2026-09-09 |
| [Qwen3 (Ollama library)](https://ollama.com/library/qwen3-coder) | `qwen3-coder` tags | Approved chat model family for order drafting | 2026-09-09 |
| [nomic-embed-text (Ollama)](https://ollama.com/library/nomic-embed-text) | current | Approved embeddings model for RAG | 2026-09-09 |

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
| `architecture/delivery/local-player-agent.md` | Ollama / RunPod player-agent runner: env prep, RAG corpus, post-turn and engine/data refresh, usage tracker, cost guardrails |

## In-repo ADRs

| File | Purpose | Date retrieved |
|------|---------|----------------|
| [`adr/ADR-0001-net48-legacy-csproj.md`](adr/ADR-0001-net48-legacy-csproj.md) | Stay on net48 + legacy csproj | 2026-08-18 |
| [`adr/ADR-0002-windows-1251-io.md`](adr/ADR-0002-windows-1251-io.md) | Windows-1251 for game XML/orders/reports | 2026-08-18 |
| [`adr/ADR-0003-filesystem-pbem-batch.md`](adr/ADR-0003-filesystem-pbem-batch.md) | Offline file-in / file-out host | 2026-08-18 |
| [`adr/ADR-0004-test-layers.md`](adr/ADR-0004-test-layers.md) | Unit = `UnitTests` / `T*.cs`; Integration = `IntegrationTests`; no module layer | 2026-08-18 |
| [`adr/ADR-0005-modulestack-partials.md`](adr/ADR-0005-modulestack-partials.md) | `ModuleStack` stays one type; limited `partial` files | 2026-08-18 |
| [`adr/ADR-0006-datafile-facade-and-xml-seams.md`](adr/ADR-0006-datafile-facade-and-xml-seams.md) | `DataFile` facade; catalog / order factory / domain XML phases | 2026-08-18 |
| [`adr/ADR-0009-local-llm-player-agent.md`](adr/ADR-0009-local-llm-player-agent.md) | Ollama + Qwen3-Coder + RAG; RunPod rented GPU | 2026-09-09 |

Prefer Microsoft Learn / NUnit docs over blog posts when versions matter.
