# Parse/save quirks — implementation checklist

Last updated: 2026-08-19  
Decision: [ADR-0006](../adr/ADR-0006-datafile-facade-and-xml-seams.md) follow-up  
Engine cited: `0.1.141`

ADR-0006 forbade fixing parse/save quirks during the `DataFile` extract ([PR #6](https://github.com/sentinemodo/SpaceAge-2024/pull/6), merged). This file is the **TDD execution plan** for that follow-up: identify gaps, one failing test then the smallest fix, pause between planned steps.

## Vehicle

1. **First commit:** architecture only (this file + ADR-0006 pointer). No C#.
2. **Later commits:** one wave row per commit. Stay green before the next phase.
3. Host/test API stays on `DataFile`. Do not retarget `Program` / `ReportWriter` / tests off the facade.
4. **Failing tests first.** Do not “complete” a switch or constructor as a drive-by without an assertion that failed on the old behavior.
5. New or replaced goldens need `/player` validation, a **±10-line diff hunk in chat**, and explicit human approval. Do not regenerate SampleGame goldens for whitespace.
6. Bump `EngineVersion` only in the commit that changes player-visible reports or committed SampleGame goldens. Docs, unit-only, and smell-only commits do not bump.

## Every commit must stay green

| Layer | Proof |
|-------|--------|
| **Unit** `Tests/TDataFile.cs` (and `TResearch` / `TSee` / `TOrder` where named) | New characterization + the failing-then-passing assertion for that wave |
| **Unit** other `Tests/T*.cs` | Existing granular load/save |
| **Integration** `Tests/SampleGame/` | Load/save goldens. Do not enable ignored turns 4–5 |

Five tests stay `[Ignore("not ready")]` (`ProcessGenerateAutoOffers`, two `SaveLoadUseOrder_*`, `ExecuteTurn4`, `ExecuteTurn5`). Do not enable them here.

## Inventory

Severity: **corrupt** = save→load or catalog load drops or mis-keys data; **lock** = an existing test or golden asserts the buggy behavior; **smell** = equivalent for `bool` or unused in fixtures.

### Wave A — load/save bugs SampleGame goldens do not lock

Current SampleGame capacities are `group="settlement"` only. Moon XML uses `name="P00001"` on both planet and moon, so a moon-id constructor fix is golden-neutral until a **distinct-id** fixture exists.

| # | Gap | Symptom | Where | Prove with |
|---|-----|---------|-------|------------|
| A1 | Catalog `use-consume` module branch | `else if (el.HasAttribute("module"))` tests the **technology parent**, so `<use-consume module="city"/>` on `ctypln` never sets `UseConsumeModules` | `CatalogLoader.cs` fill-pass consume loop | `TDataFile` — `Technology.All["ctypln"].UseConsumeModules` is `city` |
| A2 | Capacity save omits `research` | `ModuleTypeGroupXml.Parse` accepts `research`; `ToToken` has no arm → `null` → save skips `group` | `ModuleTypeGroupXml.ToToken`; `Galaxy` capacity save | Flip `ModuleTypeGroupXml_ToToken_OmitsResearchGroup`; add a region `<capacity group="research"/>` round-trip |
| A3 | Moon id from planet element | `new Moon(..., elPlanet.GetAttribute("name"))` ignores `elMoon` `name` | `Galaxy.LoadXml` | `TDataFile.LoadGalaxy_MoonNameComesFromMoonElement` with distinct ids |
| A4 | Exits: planet regions only | `LoadExits` walks `elPlanet/region` only; moon regions never get `<exit>` | `Galaxy.LoadExits` | Fixture with a moon-region exit; assert `Region.All[moonRegion].Exits` |
| A5 | Exit save always `region=` | Load accepts `region` **or** `orbit`; save always `SetAttribute("region", exit.To.Name)` | `Galaxy` region save | Orbit-target exit save/load round-trip |

### Wave B — in-progress XML round-trip

| # | Gap | Symptom | Where | Prove with |
|---|-----|---------|-------|------------|
| B1 | Conditional orders not loaded | Nested `<order conditions="…">` is saved; `OrderXml.LoadAll` never rebuilds `Level` / `ConditionalOrders`. TODO at load. | `OrderXml.cs`; `Order.saveXml_*` | Save via `OrdersReader`, `LoadOrders`, assert graph. `SaveConditionOrder` golden is save-only today |
| B2 | Conditional save duplicates | `saveXml_post` walks both `ConditionalOrders` and `ConditionedOrders` → repeated sibling trees | `Order.cs` | After B1 load works: de-dup save; golden replace only with `/player` + human |
| B3 | Effect types saved but not loaded | `Effects.LoadXml` handles `fuelled` / `moving` / `producing-modules` only. `receiving-*`, `producing-items`, damage, training throw `Unknown effect type` on reload | `Effects.cs`; `Receiving*.cs` (SaveXml, no LoadXml) | Mid-transfer or in-progress USE save/load (`SaveLoad_PersistsModuleDamageBetweenTurns` pattern) |
| B4 | `ProducingModule` drops `technology` | Load reads `technology`; save writes `module` / `receiver` / `receiver-parent` only | `ProducingModule.SaveXml` | In-progress produce effect round-trip keeps the tech id |

### Wave C — text vs XML (fix both sides together)

Anti-pattern already in [`modules-and-integrations.md`](../modules-and-integrations.md): do not patch only `OrdersReader` or only `OrderXml`.

| # | Gap | Symptom | Where | Prove with |
|---|-----|---------|-------|------------|
| C1 | `research` group / token mismatch | Text `parseModuleTypeGroup` vs XML `ModuleTypesGroup.ToString()` (`spaceStation` vs `"space station"`); bare `@research military` vs typed `research-type` attributes | `ResearchOrder.Parse` / `LoadXml` / `SaveXml_core` | Parse text → save XML → reload XML; assert same `ResearchType` / token; cover `space station` / `settlement` |
| C2 | `see` surface divergence | Text `see <name> person` vs XML `see-type="person"`; both paths call `GetOrCreateNewModuleStack` | `SeeOrder` | Text parse vs XML load equivalence in `TSee` / `TDataFile` |

### Wave D — smell (optional last)

| # | Gap | Symptom | Where | Prove with |
|---|-----|---------|-------|------------|
| D1 | Bitwise `&` on bools | `factionXMLreport != null & !Visible(...)` (equivalent to `&&` for `bool`) | `Galaxy.SaveXml`, `OrderXml.SaveAll`, `DataFile.SaveGame`, `NamedObject.SaveXml` | Existing save goldens must stay identical |

## Out of this PR

- **TRANSFER text path** — `OrderXml` has `transfer`; `OrdersReader` does not; `TransferOrder.Parse` is still a TODO stub. Player manuals already mark TRANSFER as XML-only. Separate feature, not a copy-quirk.
- Stub pipeline (`Request`, `Events`, `OrdersReader.Check`), economy `GenerateOffers` / `UpdateRates`.
- SampleGame turns 4–5 goldens / un-ignore.
- `NamedObject.LoadXml` applying XML `name` (would change constructor semantics globally; moon fix is A3).
- `loadOrbit` NRE if `<orbit>` is missing (fixtures always include it).
- Market transfer cost/time and skill thresholds “migrate to XML” TODOs.
- Encoding, runtime, namespaces, DI ([`future-work.md`](../future-work.md)).
- Optional ADR-0006 leftover: catalog fill-pass bodies on `ItemType` / `Technology` / `ModuleType` (not a quirk).

## Commits

| # | Phase | Production change | Prove with |
|---|--------|-------------------|------------|
| 1 | Docs | This checklist + ADR-0006 pointer. No C#. | n/a |
| 2 | A1 | `elConsume.HasAttribute("module")` | `TDataFile` `ctypln` consume-module |
| 3 | A2 | `ToToken(research) → "research"` | Flip omit-research test + capacity round-trip |
| 4 | A3 | Moon constructed from `elMoon` name | Distinct-id `LoadGalaxy` test |
| 5 | A4 | `LoadExits` walks moon regions | Moon-region exit fixture |
| 6 | A5 | Save `orbit=` when `exit.To` is `Orbit` | Orbit-exit round-trip |
| 7 | B1 | Load nested conditional orders | `TDataFile` condition graph round-trip |
| 8 | B2 | De-dup conditional save | `SaveConditionOrder` golden only after `/player` + human |
| 9 | B3 | Load saved effect types | Effect round-trip unit test |
| 10 | B4 | Persist `technology` on producing-modules | Produce-effect round-trip |
| 11 | C1 | Align `research` text and XML | `TResearch` + XML round-trip; goldens if save tokens change |
| 12 | C2 | Align `see` text and XML | `TSee` / `TDataFile` |
| 13 | D1 | `&` → `&&` in visibility checks | SampleGame save goldens unchanged |

Do not batch waves. Pause after each planned step.

## After merge

Parse/save quirks listed in ADR-0006 are gone or explicitly deferred above. Remaining modernization stays in [`future-work.md`](../future-work.md).
