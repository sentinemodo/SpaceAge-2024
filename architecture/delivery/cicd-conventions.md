# CI/CD and delivery conventions

Last updated: 2026-08-19

There is **no** GitHub Actions (or other hosted CI) workflow in this repository today. “CI” for Cursor Cloud is: environment image → `install` (`.cursor/install.sh`) → `.cursor/run-tests.sh`.

## Entrypoints

| Environment | Restore / build | Test |
|-------------|-----------------|------|
| Windows (local) | `nuget restore SpaceAge.sln` then `msbuild SpaceAge.sln /p:Configuration=Debug` | `vstest.console Tests\bin\Debug\Tests.dll` or VS NUnit adapter |
| Cursor Cloud (Mono) | `bash .cursor/install.sh` (also the `install` field in `.cursor/environment.json`) | `bash .cursor/run-tests.sh` |

Do not open a PR until `.cursor/run-tests.sh` succeeds, unless the failure is a documented existing baseline (today: `SampleGame._5_ExecuteTurn2`, a `//`-vs-`;` order-comment data bug, and the five `[Ignore("not ready")]` tests).

## Branches and promotion

No named `dev` / `test` / `prod` environments exist for this engine. Treat:

- **Default branch** — playable engine + passing tests.
- **Feature branches** — one behavior slice, test-first; merge when unit + SampleGame integration that is not `[Ignore]` are green.
- **`DataFile` extract** — sequential commits on one refactor PR; checklist [`datafile-refactor.md`](datafile-refactor.md) ([ADR-0006](../adr/ADR-0006-datafile-facade-and-xml-seams.md)). Do not bump `EngineVersion` for a behavior-neutral extract.

There is no production deploy artifact beyond `Game.exe` + `data.xml` shipped to the GM.

## Versioning

| What | Where | When to change |
|------|--------|----------------|
| Engine string shown to players | `Game/Program.cs` → `EngineVersion` (currently `0.1.137`) | Visible turn/report behavior change |
| NuGet pins | `Game/packages.config`, `Tests/packages.config` | Only with an ADR + Mono test pass |
| Cloud toolchain | `.cursor/install.sh` | System packages (`mono-complete`), `nuget.exe`, NUnit console runner; default Ubuntu image (no custom Dockerfile) |

Do not bump `EngineVersion` for docs-only or test-only commits.

## Config and secrets

- Game passwords live **inside XML / order files** (PBEM convention). Do not add a secrets manager for them.
- Do not commit `packages/`, `bin/`, `obj/`, or `.cursor/tools/`.
- Cloud secrets: none required for restore/build/test (public nuget.org).

## Nomenclature

| Name | Meaning |
|------|---------|
| `Game` | Engine project / `Game.exe` |
| `Tests` | NUnit assembly |
| `UnitTests` / `IntegrationTests` | Test-layer namespaces ([ADR-0004](../adr/ADR-0004-test-layers.md)) |
| `turn` | Integer game turn; also substring in `gameout.{turn}.xml` and report filenames |

## Revision

- 2026-08-19: `DataFile` extract lands as sequential commits on one refactor PR ([`datafile-refactor.md`](datafile-refactor.md)).
