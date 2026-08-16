# Documentation index

Last updated: 2026-08-16  
Purpose: canonical vendor/spec links for implementers. Summaries only — do not paste manuals into architecture docs.

| Resource | Version / band | Purpose | Date retrieved |
|----------|----------------|---------|----------------|
| [.NET Framework 4.8](https://learn.microsoft.com/en-us/dotnet/framework/whats-new/) | 4.8 | Target runtime for `Game` and `Tests` | 2026-08-16 |
| [C# language reference](https://learn.microsoft.com/en-us/dotnet/csharp/) | C# as supported by VS / net48 compiler | Language surface available to this csproj | 2026-08-16 |
| [Encoding.GetEncoding](https://learn.microsoft.com/en-us/dotnet/api/system.text.encoding.getencoding) | net48 | Code page 1251 (Windows-1251) for XML/orders/reports | 2026-08-16 |
| [Windows-1251 (code page 1251)](https://learn.microsoft.com/en-us/windows/win32/intl/code-page-identifiers) | 1251 | Cyrillic ANSI used by game data | 2026-08-16 |
| [NUnit 4 documentation](https://docs.nunit.org/articles/nunit/intro.html) | 4.1.0 | Assertions, `[TestFixture]`, `[Ignore]`, `--where` filters | 2026-08-16 |
| [NUnit Console](https://docs.nunit.org/articles/nunit/running-tests/Console-Command-Line.html) | 3.19.2 | Cloud runner (`nunit3-console.exe` under Mono) | 2026-08-16 |
| [NuGet CLI](https://learn.microsoft.com/en-us/nuget/reference/nuget-exe-cli-reference) | 6.12.1 in Dockerfile | Restore and `nuget install` of the console runner | 2026-08-16 |
| [MSBuild](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild) | VS / Mono `msbuild` or `xbuild` | Compile `SpaceAge.sln` | 2026-08-16 |
| [Mono](https://www.mono-project.com/docs/) | Ubuntu 24.04 packages | Cloud CLR substitute for net48 | 2026-08-16 |
| [Cursor environment.json](https://cursor.com/docs/cloud-agent/setup) | current Cursor Cloud | `.cursor/environment.json` + Dockerfile install path | 2026-08-16 |

## In-repo design notes (not vendor docs)

| File | Purpose |
|------|---------|
| `Game/documentation/Basics.txt` | Level-0 technologies always available to units |
| `Game/documentation/Concepts.txt` | Design intent: movement, combat, officers, markets |
| `Game/documentation/Rules.txt` | Player-facing rulebook (Alderson PBEM lineage) |
| `Game/documentation/links.txt` | External astronomy/spaceflight primers |
| `AGENTS.md` | Build, test, and cloud-agent operating notes |

Prefer Microsoft Learn / NUnit docs over blog posts when versions matter.
