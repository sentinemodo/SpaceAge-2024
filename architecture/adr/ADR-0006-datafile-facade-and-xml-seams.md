# ADR-0006: DataFile facade and XML seams

Date: 2026-08-18  
Status: **Accepted** (strategy and seams; implementation is phased)  
Engine cited: `0.1.141`

This ADR is the gate that [`../future-work.md`](../future-work.md) and the TDD rule required before anyone splits `DataFile`. Implementers may execute the phases below; they must not invent extra seams, DI, or a big-bang rewrite.

## Context

`Game/game/DataFile.cs` (~1640 lines) inherits `XMLProcessing` and currently owns:

- **Catalog** (`data.xml`): `LoadConfiguration` → `LoadConfDocument` → two-pass `LoadConfigurationItems(true)` stubs then `LoadConfigurationItems(false)` fill → `ValidateTypeNameUniqueness()`. Two-pass exists because catalog entries cross-reference (`technology/@requires`, `use-produce` modules/items, race as both `ItemType` and `Race`).
- **Game state** (`gamein.xml`): `LoadGame` requires config loaded, then `LoadGameDocument` → `LoadTurnNumber` → `LoadFactions` → `LoadGalaxy` → `LoadContracts` → `LoadOrders`.
- **Save** (`gameout.{turn}.xml` and per-faction XML reports): `SaveGame(dir, file, factionXMLreport)` writes factions, contracts, galaxy (visibility-filtered when a faction is set), then `SaveOrders`. `ReportWriter` reuses `SaveGame(..., faction)` for XML sidecars. Faction name `"1"` is treated as NPC and the filter is cleared.

`LoadXml` / `SaveXml` on `DataFile` throw `NotImplementedException` with the comment that XML should eventually live on relevant classes. That comment is the **target**, not a license to rewrite in one PR.

Already extracted toward domain types (do not undo):

- `Contract.All.LoadXml` / `SaveXml`
- `ModuleStack.All.LoadXml` / `ModuleStacks.SaveXml`
- `Order.LoadXml` / `Order.SaveXml` (instance payload only)
- `NamedObject.LoadXml` / `SaveXml` (name / `name-en` only)
- Offers, effects, item stacks, technologies-on-stack, tactics, event reports

Still centralized in `DataFile`:

- Entire catalog parse, including `getModuleTypeGroup` and `LoadLocationType`
- Faction XML (bank, attitudes, technologies-seen)
- Galaxy graph (systems, stars, planets, moons, regions, orbits, resources, capacities, **exits in a second pass** `loadGalaxyExits`)
- Order **type factory** switch plus `repeat` attribute
- Symmetric save for factions, galaxy, regions, capacities, exits, resources, orbits
- Shared helpers: `assignItemStacks` (catalog fill-pass). Names go through `NamedObject.LoadXml` / `LoadMultipleNames`.

Stable host/test API (must keep working without a mass call-site change):

| Caller | Methods |
|--------|---------|
| `Game/Program.cs` | `new DataFile(dir)`, `LoadConfiguration()`, `LoadGame()`, `SaveGame()` |
| `Game/reports/ReportWriter.cs` | `SaveGame(turnDir, reportFile + ".xml", faction)` |
| `Tests/TTest.cs` | `LoadConfiguration` + `LoadGame` |
| `Tests/TDataFile.cs` | uniqueness, catalog content, granular load, round-trips |
| Other unit tests (`TSee`, `TGetGiveHas`, `TReport`, `TMarket`, `TDiplomacy`, `TResearch`, SampleGame, …) | `LoadGameDocument`, `LoadFactions`, `LoadGalaxy`, `LoadOrders`, `LoadContracts`, `SaveGame` |

Constraints that stay in force: net48, non-SDK csproj, single `SpaceAge` namespace, static `*.All` registries, Windows-1251 via `XMLProcessing.loadXmlDocument` and `XmlTextWriter` with `Encoding.GetEncoding(1251)` ([ADR-0001](ADR-0001-net48-legacy-csproj.md), [ADR-0002](ADR-0002-windows-1251-io.md), [ADR-0003](ADR-0003-filesystem-pbem-batch.md)). No DI, async, SDK-style, or namespace splits.

[ADR-0005](ADR-0005-modulestack-partials.md) is the only prior file-split precedent: **one type**, `partial` files only when that is the chosen seam. Do not split the rest of `ModuleStack` under this ADR.

## Options considered

### A. Partials only

Keep a single `DataFile` type. Split files by concern (`DataFile.Catalog.cs`, `.Galaxy.cs`, `.Factions.cs`, `.Orders.cs`, `.Save.cs`). Public API unchanged. Same mechanical move as ADR-0005.

- **Pros:** Lowest golden risk; navigable diffs; no new types.
- **Cons:** `DataFile` remains a god class; no design improvement; later extracts would move the same methods a second time.

### B. Facade + extracted loaders

`DataFile` stays the host/test facade. New types own catalog vs game-state load vs save. No DI; `DataFile` constructs collaborators internally.

- **Pros:** Clear ownership; call sites unchanged; each loader is a reviewable PR.
- **Cons:** Easy to mint **new** god classes (`GalaxyLoader` ~400 lines). Collection XML that already has a domain home (`Contract.All`) would be inconsistent if galaxy/faction XML lived only in loaders.

### C. Push remaining XML onto domain types

Continue `Contract` / `ModuleStack` / `Order` until `DataFile` is only orchestration + document I/O.

- **Pros:** Matches the in-code `LoadXml` comment; no parallel loader hierarchy.
- **Cons:** Catalog two-pass and galaxy exit second-pass are **document-level** graphs, not “each type loads itself in isolation.” A naïve per-type `LoadXml` would break `requires` / exits. Largest golden risk if done as one rewrite. Tests call granular `DataFile` methods, so a facade is still required.

### D. Hybrid / phased (chosen)

Target is **C for instance XML** and **B for document-level graphs**. Optional ADR-0005-style partials only if a single extract PR is too large to review — not as the destination.

Rationale:

- Two-pass catalog and two-pass galaxy exits cannot collapse into independent per-entry `LoadXml` without an orchestrator that runs stub-then-fill / objects-then-exits.
- Faction XML, region/orbit instance XML, and the order **payload** already fit the domain `LoadXml`/`SaveXml` pattern.
- The order **factory switch** is not a domain instance concern; it is a factory next to `Order`.
- `SaveGame` faction filtering (NPC `"1"`, skip empty factions, `Visible(faction)` on galaxy objects) is orchestration and must not be reinvented inside every type.
- Small independently shippable slices match this repo’s TDD pause-between-plan-steps culture.

## Decision

**Phased hybrid.** `DataFile` remains the stable facade forever (option B at the host boundary). Remaining XML moves as follows (option C where a type already owns the instance; option B only for document-level graphs).

### What stays on `DataFile`

- `DataFile(string gameDir)` — creates `Game`, holds `gameDir`, `confDocument`, `gameDocument`, `configurationLoaded`.
- `LoadDocument` → `loadXmlDocument` (1251).
- Orchestration: `LoadConfiguration`, `LoadGame`, `SaveGame` / `SaveGame(dir, file, faction)`.
- Thin public delegates so existing tests keep compiling: `LoadConfDocument`, `LoadConfigurationItems`, `ValidateTypeNameUniqueness`, `LoadGameDocument` (both overloads), `LoadTurnNumber`, `LoadFactions`, `LoadGalaxy`, `LoadContracts`, `LoadOrders`, `SaveOrders`, `LoadItemstacks`, `LoadLocationType`.
- Faction XML-report policy (do not change semantics):
  - `factionXMLreport != null` and `Name == "1"` → treat as unfiltered (`factionXMLreport = null`).
  - Skip factions with no module stacks.
  - Skip other factions when a filter is set (`!=` identity check, including today’s bitwise `&` vs `&&` — do not “fix” in the same PR).
- `XmlTextWriter` 1251 indented write of the assembled `XmlDocument`.
- The throwing `LoadXml`/`SaveXml` overrides may remain until nothing inherits a need for them; do not add **new** `XMLProcessing` subclasses that only throw.

### Named seams (types and methods)

All new types stay in namespace `SpaceAge`. Prefer folder `Game/game/` for loaders/factories and existing `Game/data structures/` / `Game/orders/` for domain XML. Legacy `Game.csproj` needs a `<Compile Include="..."/>` for every new file.

| Seam | Type | Owns | `DataFile` keeps |
|------|------|------|------------------|
| Catalog two-pass | `CatalogLoader` | `LoadItems(XmlDocument conf, Game game, bool loadStub)` (today’s `LoadConfigurationItems` body), catalog use of `NamedObject.LoadXml` / `LoadMultipleNames` / `DataFile.assignItemStacks` | `LoadConfiguration` sequence, `LoadConfDocument`, `ValidateTypeNameUniqueness` (post-condition on `ItemType.All` vs `ModuleType.All`), public `LoadConfigurationItems` as a one-line delegate |
| Module-group tokens | `ModuleTypeGroupXml` (static) | `Parse(string)` / `ToToken(EModuleTypesGroup)` copied from `getModuleTypeGroup` and the capacity **save** switch | Call sites only |
| Location tokens | stay on `DataFile.LoadLocationType` until catalog extract, then move next to `ModuleTypeGroupXml` or `RegionType` | `orbit` / `solid-surface` / `liquid-surface` / `space` | Public `LoadLocationType` delegate if tests call it |
| Order factory | `OrderXml` (static) | Subject switch, `FirstChild.Name` factory, `order.LoadXml`, `repeat` (`unlimited` → `-1`) | Public `LoadOrders` / `SaveOrders` delegates; `SaveOrders` may move onto `OrderXml.SaveAll` in the same seam |
| Faction instance XML | `Faction.LoadXml` / `Faction.SaveXml` (override `NamedObject`) | Bank, options, attitudes, technologies-seen | Loop + empty-stack skip + NPC/filter policy |
| Galaxy graph | `Galaxy.LoadXml` / `Galaxy.LoadExits` / `Galaxy.SaveXml(doc, Faction filter)` | Systems, stars, planets, moons, regions, orbits, resources, capacities; **exits second pass**; save visibility using existing `Visible(faction)` | Public `LoadGalaxy` delegate; `SaveGame` still creates the document and appends `galaxy` |
| Nested location XML | optional later slices on `Region`, `Orbit`, `Planet`, `Moon` | Move loops out of `Galaxy.LoadXml` only after the galaxy move is green | Do not require this in the first galaxy PR |
| Name helpers | `NamedObject` | Fold `assignNames` (and description) into existing `LoadXml`; `assignNamesMultiple` onto `IMultiple` implementers | Stop duplicating helpers on `DataFile` once catalog/galaxy call the domain methods |
| Contracts / stacks | unchanged | Already on `Contract.All` / `ModuleStack.All` | `LoadContracts` stays a one-liner |

`CatalogLoader` and `OrderXml` **do not** inherit `XMLProcessing`. Promote `XMLAssignDouble` and `XMLAssignString` from `protected` to `public` on `XMLProcessing` when the first extract needs them (they are already shared parse helpers). Do not create more types whose `LoadXml`/`SaveXml` only throw.

`Galaxy` does not need to become an `XMLProcessing` subclass; collection-style `LoadXml`/`SaveXml` (as on `Contracts` / `ModuleStacks`) is enough.

### Preserve these behaviors exactly (this PR)

Copy current parse/save quirks during the extract. **Do not fix them here.** A follow-up PR will change them, with failing tests first.

- Two-pass catalog. Do not load `requires` / `use-produce` on the stub pass.
- Galaxy: construct all systems/planets/moons/regions/orbits, then `loadGalaxyExits`. Today that second pass walks **planet regions only**.
- Encoding 1251 on read and write.
- ReportWriter faction filter, including NPC `"1"`.
- Capacity **save** switch as written (no `research` case even though load parses `research`).
- Region exit **save** always `elExit.SetAttribute("region", exit.To.Name)` even when `exit.To` is an orbit.

## Interim vs target

Dated: 2026-08-18, engine `0.1.141`.

**Interim (after Phase 1–2):** `DataFile` still contains galaxy/faction/save loops; catalog and order factory have left. Host API unchanged.

**Target (after Phase 3–5):** `DataFile` is roughly constructor + 1251 document I/O + orchestration + thin delegates. Catalog two-pass lives in `CatalogLoader`. Instance XML lives on domain types. Order construction lives in `OrderXml`. Save still runs through `DataFile.SaveGame` so `ReportWriter` does not grow a second writer.

Partials of `DataFile` are allowed **inside** a phase if the remaining file is still large, but they are not a substitute for the named type seams. Do not split the rest of `ModuleStack` here.

## Consequences

- TDD may extract along these seams one phase at a time. Opportunistic extra splits of `DataFile` without updating this ADR are still forbidden.
- Call sites (`Program`, `ReportWriter`, `TTest`, `TDataFile`, granular unit tests) must keep compiling against the same `DataFile` methods.
- New types are not a second persistence API: tests and host code go through `DataFile`.
- Known parse/save quirks stay **out of this extract**. They are the subject of a **follow-up PR** (failing tests first), not drive-bys on extract commits.
- DI, `*.All` replacement, namespace splits, SDK-style, async, and UTF-8 remain in [`../future-work.md`](../future-work.md).

## Out of this extract

### Follow-up PR (parse/save quirks)

Do not fix these while extracting. The **next PR after this refactor** owns them (failing tests first):

- `LoadOrders` TODO: validate conditional orders load in XML.
- Moon constructor uses `elPlanet.GetAttribute("name")` rather than the moon element (possible bug).
- `loadGalaxyExits` walks planet regions only; moon regions/exits look incomplete.
- Capacity save switch omits `research` (load has `research`).
- Region save always sets `region=` even if the exit is to an orbit.
- Catalog tech `use-consume` module branch checks `el.HasAttribute("module")` (parent) not `elConsume`.
- `research` / `see` text vs XML divergence — fix both `OrdersReader` and XML together ([`../modules-and-integrations.md`](../modules-and-integrations.md) anti-pattern).
- Bitwise `&` instead of `&&` in save visibility checks (works for `bool`; smell only).

When extracting `ModuleTypeGroupXml.ToToken`, **copy the save switch including the missing `research` arm**. Completing the switch is the follow-up PR.

### Still not this extract or the quirk PR

- Activating stub pipeline steps (`Request`, `Events`, `OrdersReader.Check`).
- Any encoding, runtime, or namespace change.

## Migration phases (TDD handoff)

Each phase is **one planned TDD step**. It may land as its own GitHub PR **or** as a sequential commit on a single refactor PR (docs first). Stay green on `UnitTests` + `IntegrationTests` before the next phase. Prefer characterizing existing load/save rather than a third test layer ([ADR-0004](ADR-0004-test-layers.md)).

Checklist and commit order for the current refactor PR: [`../delivery/datafile-refactor.md`](../delivery/datafile-refactor.md).

### Tests that must stay green (every phase)

| Layer | What |
|-------|------|
| **Unit** `Tests/TDataFile.cs` | Uniqueness, `LoadConfiguration_LoadsResearchContent`, granular `LoadFactions` / `LoadGalaxy` / `LoadOrders`, save/load round-trips (`SaveLoad_PersistsModuleDamageBetweenTurns`, condition-order XML, …) |
| **Unit** other `Tests/T*.cs` | Any fixture that calls `LoadGameDocument` + granular loads (`TMarket`, `TResearch`, `TDiplomacy`, `TSee`, `TGetGiveHas`, `TReport`, `TTest` setup) |
| **Integration** `Tests/SampleGame/` | Load/save goldens (`_2_SaveGameIn1`, `*_saved.xml`, execute-turn `gameout.*_saved.xml`, reports). Do not enable ignored turns 4–5 as part of this work |

Do not add `*.UnitTests` projects, `[Category]`, or a module test layer.

**Characterize first (Phase 0, tests only if coverage is thin):** run `TDataFile` + SampleGame save goldens on current `DataFile` before moving code. Add assertions only when a seam has no existing proof (for example catalog two-pass still resolves `requires`). Do not snapshot new goldens unless `/player` + human approval says so.

### Suggested extract order

1. **Helpers for the first extract.** If `CatalogLoader` needs `XMLAssignDouble` / `XMLAssignString`, make those `public` on `XMLProcessing`. Optionally extract `ModuleTypeGroupXml.Parse` / `ToToken` in the same PR as catalog **or** immediately before it. `ToToken` must match the save switch (no `research`).
2. **`CatalogLoader`.** Move `LoadConfigurationItems` body. `DataFile.LoadConfiguration` still does stub pass, fill pass, `ValidateTypeNameUniqueness`. `LoadConfigurationItems` remains a public delegate. Prove with `TDataFile.LoadConfiguration*` and uniqueness tests. Do **not** also push fill-pass into `ItemType`/`Technology`/`ModuleType` in this PR.
3. **`OrderXml`.** Move the subject switch, type factory, `LoadXml`, and `repeat`. `DataFile.LoadOrders` delegates. Prove with `TDataFile` order-load tests and SampleGame `gamein.*_saved_orders.xml` / execute-turn goldens. Adding a new XML order type means `OrderXml` **and** `OrdersReader` (existing anti-pattern).
4. **`Faction` XML.** Move load/save of one faction element onto `Faction`. `DataFile` keeps the loop, empty-stack skip, and NPC `"1"` filter. Prove with `TDataFile.LoadFactions`, `TDiplomacy`, `TResearch` known-tech round-trip, SampleGame faction XML goldens.
5. **`Galaxy` XML.** Move `LoadGalaxy` + private helpers + `loadGalaxyExits` to `Galaxy.LoadXml` + `Galaxy.LoadExits`. Move galaxy/region/orbit/resource save (including `Visible` filters) to `Galaxy.SaveXml(doc, filter)`. Keep moon-name constructor quirk and planet-only exit walk. Prove with `TDataFile.LoadGalaxy`, module-damage round-trip, SampleGame `gamein.*_saved.xml` / `gameout.*_saved.xml`. Nested `Region`/`Orbit` methods are a **follow-up slice** of this phase, not a second god-class extract.
6. **Name helpers (when duplication appears).** Fold `assignNames` into `NamedObject.LoadXml` (include `description` if that is current `assignNames` behavior). Stop catalog/galaxy from using `DataFile` private copies.

Optional: ADR-0005-style `partial` files on `DataFile` **instead of** step 2 only if the catalog move cannot be reviewed as one PR. Prefer not to partial-then-extract the same methods.

Optional follow-on (still this ADR, later slice): catalog fill-pass bodies on `ItemType` / `Technology` / `ModuleType` `LoadXml`, with `CatalogLoader` only looping stub-then-fill. Do not collapse the two passes.

### What not to change in the same PRs

Everything in **Out of this extract** (quirks wait for the follow-up PR; stubs/encoding stay later still). Also: do not retarget call sites from `DataFile` to `CatalogLoader`/`OrderXml`/`Galaxy` in tests or `Program`. Do not bump `EngineVersion` for a behavior-neutral extract (docs/code-only). Do not regenerate SampleGame goldens “because whitespace”; 1251 `XmlTextWriter` formatting must stay identical.

### csproj / layout notes for implementers

- Add `<Compile Include="game\CatalogLoader.cs" />` (etc.) to `Game/Game.csproj`; the legacy csproj does not glob.
- Tests: extend `TDataFile` (unit) rather than a new fixture type unless a single method’s contract needs a focused test in the same file.
- `ClearDictionaries` teardown stays mandatory; extracts must not introduce a second live `Game` without reset.

## Revision

- 2026-08-18: Accepted. Names seams so TDD can extract without opportunistic god-class splits.
- 2026-08-19: Implementation vehicle: sequential commits on one refactor PR (architecture docs first, then one phase per commit). Delivery checklist in [`../delivery/datafile-refactor.md`](../delivery/datafile-refactor.md).
- 2026-08-19: Parse/save quirks are a **follow-up PR** after this extract, not drive-bys here.
- 2026-08-19: Phase 6 — `assignNames` / `assignNamesMultiple` folded into `NamedObject.LoadXml` (including `description`) and `LoadMultipleNames`. `DataFile` keeps `assignItemStacks`.
