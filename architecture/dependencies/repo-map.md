# Dependency map

Last updated: 2026-08-18

SpaceAge-2024 is a **single repository**. There are no sibling application repos and no runtime package feeds other than nuget.org.

## Projects

```mermaid
flowchart LR
  sln[SpaceAge.sln]
  sln --> game[Game.exe SpaceAge]
  sln --> tests[Tests.dll]
  tests -->|project reference| game
  tests --> nunit[NUnit 4.1.0]
```

| From | To | Contract |
|------|----|----------|
| `Tests` | `Game` | Compile-time project reference; tests construct `DataFile` / `Game` in-process |
| GM / mailer (external, not in repo) | `Game.exe` | CLI + files: `/data`, `/turn-dir`, reports with `To:` headers |
| Cursor Cloud | this repo | Checkout + `.cursor/install.sh` |

Do not add a second engine repo or a shared “core” library unless an ADR splits the solution.

## NuGet (nuget.org)

See [`../technology.md`](../technology.md) for pins. NUnit and its net48 transitives (`System.Runtime.CompilerServices.Unsafe`, `System.Threading.Tasks.Extensions`) are restored from `Tests/packages.config` only. `Game` has an empty `packages.config`.

## Filesystem contracts (integration surface)

Documented in [`../modules-and-integrations.md`](../modules-and-integrations.md). No HTTP, no message bus, no database.
