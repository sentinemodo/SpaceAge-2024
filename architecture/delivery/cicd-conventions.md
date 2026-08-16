# CI/CD and delivery conventions

Last updated: 2026-08-16

There is **no** GitHub Actions (or other hosted CI) workflow in this repository today. “CI” for Cursor Cloud is: environment image → `install` (`.cursor/install.sh`) → `.cursor/run-tests.sh`.

## Entrypoints

| Environment | Restore / build | Test |
|-------------|-----------------|------|
| Windows (local) | `nuget restore SpaceAge.sln` then `msbuild SpaceAge.sln /p:Configuration=Debug` | `vstest.console Tests\bin\Debug\Tests.dll` or VS NUnit adapter |
| Cursor Cloud (Mono) | `bash .cursor/install.sh` (also the `install` field in `.cursor/environment.json`) | `bash .cursor/run-tests.sh` |

Do not open a PR until `.cursor/run-tests.sh` succeeds, unless the failure is a documented existing baseline (today: `SampleGame._5_ExecuteTurn2`, a `//`-vs-`;` order-comment data bug, and the six `[Ignore("not ready")]` tests).

## Branches and promotion

No named `dev` / `test` / `prod` environments exist for this engine. Treat:

- **Default branch** — playable engine + passing tests.
- **Feature branches** — one behavior slice, test-first; merge when unit + SampleGame integration that is not `[Ignore]` are green.

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
