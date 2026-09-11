# SpaceAge-2024 — future development backlog

Last updated: 2026-09-09

This file tracks **deferred modernization** — good practices that are intentionally **out of scope** for day-to-day work on the current engine. They are recorded here (not enforced by the TDD rule or coding guidance) so the running net48 engine stays stable and diffs stay small.

Each item requires a numbered **ADR** in [`adr/`](adr/) plus a full test pass on Mono (`.cursor/run-tests.sh`) before it is adopted. Nothing here is a commitment or a schedule; it is a menu of improvements to weigh when a change is explicitly requested.

## Modern C# / language

- **Nullable reference types**, `required` members, and `record` value objects where they clarify intent — gated by what the non-SDK net48 compiler and neighboring code allow.
- Disciplined `async`/`await` (`CancellationToken`, `IAsyncEnumerable`, deterministic disposal) *if* any I/O ever becomes async. The current engine is synchronous and file-based; do not add async speculatively.

## Structure and design

- **Dependency injection / interfaces at boundaries** to replace the pervasive static `*.All` registries, enabling parallel tests and multiple in-process games. Today [ADR-0003](adr/ADR-0003-filesystem-pbem-batch.md) and the `*.All` pattern are load-bearing.
- **Split the single `SpaceAge` namespace** into folder-aligned namespaces (currently folders are organizational only).
- **`DataFile` extracts** — seams and phases are named in [ADR-0006](adr/ADR-0006-datafile-facade-and-xml-seams.md). Remaining work is executing those phases (catalog loader, order factory, faction/galaxy XML on domain types). Do not opportunistic-split `DataFile` or invent extra loader types. Optional later slice: catalog fill-pass on `ItemType`/`Technology`/`ModuleType` without collapsing two-pass.
- **`ModuleStack` decomposition** — seams and phased partial files in [ADR-0008](adr/ADR-0008-modulestack-decomposition.md) (**Accepted**); execution checklist in [`delivery/modulestack-refactor.md`](delivery/modulestack-refactor.md). **`Person`** mirror (six shared interfaces) is a follow-up ADR, not the first stack PR series.

## Build, test, and delivery

- **Migrate to SDK-style projects / .NET 8** (`dotnet build` / `dotnet test`) — a product-wide migration, not a local refactor (see [ADR-0001](adr/ADR-0001-net48-legacy-csproj.md)). Would replace the Mono `xbuild` + NUnit-console path.
- **Test categories / filters** (`[Category]` or split projects) so "fast" vs "full" pipelines can be gated independently. Today layering is by namespace only ([ADR-0004](adr/ADR-0004-test-layers.md)).
- **Hosted CI** (e.g. GitHub Actions) around restore → build → test. Today "CI" is the Cursor Cloud environment running `.cursor/install.sh` then `.cursor/run-tests.sh`.

## Engine completeness (currently stubbed / partial)

- Implement the stub pipeline hooks `Request.Load`, `EventsReaders.Load` / `Events.Execute`, and `OrdersReader.Check` (with tests) when a feature needs them.
- ~~Finish the economy methods `Game.GenerateOffers` / `UpdateRates`~~ — live **0.1.159** (open beta).
- Add goldens and enable the `[Ignore("not ready")]` SampleGame turns 4–5. Turns 1–3 already load committed `gamein` files independently; do not reintroduce a `copyFile` daisy chain. Follow-ups: a `data.unit.xml` catalog for unit tests, `Tests/Stories/` scenario fixtures, and an optional `[Explicit]` chain-consistency test (`gameout.N` vs committed `gamein.N+1`).
- Fix known data/parse gaps, e.g. the `//`-vs-`;` order-comment bug that fails `IntegrationTests.SampleGame._5_ExecuteTurn2`.
