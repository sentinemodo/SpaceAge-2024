# DataFile refactor — implementation checklist

Last updated: 2026-08-19  
Decision: [ADR-0006](../adr/ADR-0006-datafile-facade-and-xml-seams.md)  
Engine cited: `0.1.141`

This is the **TDD execution plan** for extracting XML from `Game/game/DataFile.cs`. The ADR names seams and forbids extra types, DI, and a big-bang rewrite. This file is the commit-by-commit checklist for one refactor PR.

## Vehicle

1. **First commit:** architecture only (this file + ADR-0006 pointer). No C#.
2. **Later commits:** one ADR phase per commit. Pause between planned steps (TDD rule). Stay green before the next phase.
3. Host/test API stays on `DataFile`. Do not retarget `Program`, `ReportWriter`, or tests to `CatalogLoader` / `OrderXml` / `Galaxy`.
4. Do not bump `EngineVersion` for a behavior-neutral extract. Do not regenerate SampleGame goldens for whitespace; 1251 `XmlTextWriter` formatting must stay identical.
5. Legacy `Game/Game.csproj` does not glob: add `<Compile Include="game\CatalogLoader.cs" />` (etc.) next to `game\DataFile.cs`.

## Every commit must stay green

| Layer | Proof |
|-------|--------|
| **Unit** `Tests/TDataFile.cs` | Uniqueness, `LoadConfiguration*`, granular load, save/load round-trips |
| **Unit** other `Tests/T*.cs` | Fixtures that call `LoadGameDocument` + granular loads |
| **Integration** `Tests/SampleGame/` | Load/save goldens. Do not enable ignored turns 4–5 |

Characterize with existing tests. Add assertions only when a seam has no proof. New goldens need `/player` plus explicit human approval.

## Commits

| # | Phase | Production change | Prove with |
|---|--------|-------------------|------------|
| 1 | Docs | This checklist + ADR-0006 revision. No C#. | n/a |
| 2 | 0 — characterize | Tests only if coverage is thin (e.g. two-pass still resolves `requires`). No move yet. | `TDataFile` + SampleGame save goldens |
| 3 | 1 — helpers | Promote `XMLAssignDouble` / `XMLAssignString` to `public` on `XMLProcessing` if the next extract needs them. Optional `ModuleTypeGroupXml.Parse` / `ToToken` (copy the capacity **save** switch, including missing `research`). | Uniqueness / catalog tests still pass |
| 4 | 2 — `CatalogLoader` | Move `LoadConfigurationItems` body. `DataFile.LoadConfiguration` still does stub pass, fill pass, `ValidateTypeNameUniqueness`. Public `LoadConfigurationItems` remains a one-line delegate. Do **not** push fill-pass onto `ItemType` / `Technology` / `ModuleType` here. | `TDataFile.LoadConfiguration*` |
| 5 | 3 — `OrderXml` | Move subject switch, type factory, `LoadXml`, `repeat`. `DataFile.LoadOrders` delegates. New XML order types still need `OrderXml` **and** `OrdersReader`. | `TDataFile` order-load + SampleGame `*_saved_orders.xml` / execute-turn goldens |
| 6 | 4 — `Faction` XML | `Faction.LoadXml` / `SaveXml`. `DataFile` keeps the loop, empty-stack skip, NPC `"1"` filter. | `TDataFile.LoadFactions`, `TDiplomacy`, `TResearch` known-tech, SampleGame faction XML |
| 7 | 5 — `Galaxy` XML | `Galaxy.LoadXml` + `LoadExits` + `SaveXml(doc, filter)`. Keep moon-name constructor quirk and planet-only exit walk. Nested `Region` / `Orbit` is a follow-up slice of this phase, not a new god class. | `TDataFile.LoadGalaxy`, module-damage round-trip, SampleGame `*_saved.xml` |
| 8 | 6 — name helpers | Fold `assignNames` into `NamedObject.LoadXml` when duplication appears. | Same load/save goldens |

Optional: ADR-0005-style `partial` files on `DataFile` **instead of** commit 4 only if the catalog move cannot be reviewed as one commit. Prefer not to partial-then-extract the same methods.

Optional later slice (still ADR-0006, not required to merge this PR): catalog fill-pass bodies on `ItemType` / `Technology` / `ModuleType` `LoadXml`, with `CatalogLoader` only looping stub-then-fill. Do not collapse the two passes.

## Out of scope on this PR

Do not “fix” as drive-bys (ADR-0006):

- Conditional-order XML validation TODO
- Moon constructor using the planet name
- `loadGalaxyExits` walking planet regions only
- Capacity save omitting `research`
- Exit save always `region=` (including orbit targets)
- Tech `use-consume` module branch checking the parent element
- `research` / `see` text vs XML divergence
- Bitwise `&` in save visibility checks
- Stub pipeline (`Request`, `Events`, `OrdersReader.Check`)
- Runtime, encoding, namespace, or DI changes

## After merge

`DataFile` is constructor + 1251 document I/O + orchestration + thin delegates. Remaining modernization (`*.All`, SDK-style, namespaces) stays in [`../future-work.md`](../future-work.md).
