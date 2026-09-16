# SpaceAge-2024 — future development backlog

Last updated: 2026-09-16

This file tracks **deferred modernization** — good practices that are intentionally **out of scope** for day-to-day work on the current engine. They are recorded here (not enforced by the TDD rule or coding guidance) so the running net48 engine stays stable and diffs stay small.

Each item requires a numbered **ADR** in [`adr/`](adr/) plus a full test pass on Mono (`.cursor/run-tests.sh`) before it is adopted. Nothing here is a commitment or a schedule; it is a menu of improvements to weigh when a change is explicitly requested.

## Modern C# / language

- **Nullable reference types**, `required` members, and `record` value objects where they clarify intent — gated by what the non-SDK net48 compiler and neighboring code allow.
- Disciplined `async`/`await` (`CancellationToken`, `IAsyncEnumerable`, deterministic disposal) *if* any I/O ever becomes async. The current engine is synchronous and file-based; do not add async speculatively.

## Structure and design

- **Dependency injection / interfaces at boundaries** to replace the pervasive static `*.All` registries, enabling parallel tests and multiple in-process games. Today [ADR-0003](adr/ADR-0003-filesystem-pbem-batch.md) and the `*.All` pattern are load-bearing.
- **Split the single `SpaceAge` namespace** into folder-aligned namespaces (currently folders are organizational only).
- **`DataFile` extracts** — seams and phases are named in [ADR-0006](adr/ADR-0006-datafile-facade-and-xml-seams.md). Remaining work is executing those phases (catalog loader, order factory, faction/galaxy XML on domain types). Do not opportunistic-split `DataFile` or invent extra loader types. Optional later slice: catalog fill-pass on `ItemType`/`Technology`/`ModuleType` without collapsing two-pass.
- **`ModuleStack` decomposition** — **Implemented** (2026-09-16); see [ADR-0008](adr/ADR-0008-modulestack-decomposition.md) and [`delivery/modulestack-refactor.md`](delivery/modulestack-refactor.md). **`Person`** mirror (six shared interfaces) remains a follow-up ADR.

## Build, test, and delivery

- **Migrate to SDK-style projects / .NET 8** (`dotnet build` / `dotnet test`) — a product-wide migration, not a local refactor (see [ADR-0001](adr/ADR-0001-net48-legacy-csproj.md)). Would replace the Mono `xbuild` + NUnit-console path.
- **Test categories / filters** (`[Category]` or split projects) so "fast" vs "full" pipelines can be gated independently. Today layering is by namespace only ([ADR-0004](adr/ADR-0004-test-layers.md)).
- **Hosted CI for the engine** — GitHub Actions restore → build → `Tests.dll`. Today only [`.github/workflows/website.yml`](../../.github/workflows/website.yml) covers the Astro lobby; engine proof remains `.cursor/install.sh` + `.cursor/run-tests.sh` (or local MSBuild + vstest).

## Engine completeness (currently stubbed / partial)

### Events pipeline vs `EventReports` (do not conflate)

| Mechanism | Status | Role |
|-----------|--------|------|
| **`EventReports`** | **Live** | Per-entity turn log (`ModuleStack`, `Person`, `Faction`, …). Orders and effects call `EventReports.Add(week, …)` during `Game.Execute()`. Lines appear in **faction/unit report sections** via `Faction.Report()` / stack reports. Saved on stacks and persons in game XML. |
| **`Events` pipeline** | **Stub** | Turn-start hook in [`Program.cs`](../../Game/Program.cs): `Request.Load` → `EventsReaders.Load` → `game.Events.Execute()` **before** orders load. Intended for scripted world mutations (fauna growth, militia raids, hostility flips). |
| **`Events.Execute` today** | Partial | Only [`FaunaRumors.IssueAll()`](../../Game/FaunaRumors.cs) (settlement-adjacent fauna press rumors). Also invoked from [`ReportWriter.GenerateReports`](../../Game/reports/ReportWriter.cs) on `/reports`-only runs. |
| **`EventsReaders.Load`** | Stub | Returns `null`; does not load event files from `turn_dir`. |
| **`Request.Load`** | Stub | No-op. |
| **`OrdersReader.Check`** | Stub | No-op (CLI check-order path). |
| Report header | Misleading | Text reports still print hardcoded `Events during turn:` / `none.` — **not** wired to `EventReports` or the pipeline. |

Designer wishlist rows that say “`Events` pipeline or GM orders” ([`play/designer/engine-wishlist.md`](../../play/designer/engine-wishlist.md)) mean the stub hook above, not the live `EventReports` machinery.

### Remaining backlog

- Implement `Request.Load`, `EventsReaders.Load`, and flesh out `Events.Execute` (with tests) when a feature needs scripted turn-start or week-13 world updates.
- ~~Finish the economy methods `Game.GenerateOffers` / `UpdateRates`~~ — live **0.1.159** (open beta).
- ~~SampleGame turns 4–5 goldens~~ — **live** (2026-09-16). `ExecuteTurn4` / `ExecuteTurn5` in [`Tests/SampleGame/SampleGame.cs`](../../Tests/SampleGame/SampleGame.cs) load committed `gamein.4.xml` / `gamein.5.xml` (no `[Ignore]`). Turns 1–3 use the same independent-`gamein` pattern; do not reintroduce a `copyFile` daisy chain.
- **SampleGame follow-ups (optional):** `Tests/data.unit.xml` catalog for unit tests, `Tests/Stories/` scenario fixtures, and an `[Explicit]` chain-consistency test (`gameout.N` vs committed `gamein.N+1`).
- Wire report header `Events during turn:` to pipeline output (or drop the section) when `Events.Execute` grows beyond fauna rumors.
