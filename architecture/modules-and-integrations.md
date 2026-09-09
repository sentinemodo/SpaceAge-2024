# SpaceAge-2024 — modules and integrations

Last updated: 2026-08-29

All engine types live in namespace `SpaceAge`. Folders below are **bounded contexts by ownership**, not separate assemblies.

## Modules

| Module | Path | Owns | Talks to |
|--------|------|------|----------|
| **Host** | `Game/Program.cs`, `Game.cs` | CLI (`/data`, `/turn-dir`, `/check`), turn orchestration, `EngineVersion` | DataFile, OrdersReader, ReportWriter, stub Events/Request |
| **Persistence** | `Game/game/` | `DataFile` (host/test facade), `XMLProcessing` (1251 XML), `Sequence` RNG, `Market`, `Bank`. Target collaborators per [ADR-0006](adr/ADR-0006-datafile-facade-and-xml-seams.md): `CatalogLoader`, `OrderXml`, `ModuleTypeGroupXml`; faction/galaxy instance XML on those types | Catalog + game XML; fills static `*.All` registries. Host and tests talk to `DataFile` only |
| **Orders** | `Game/orders/` | Parse `order.*`, condition graph, weekly execute (immediate then one long) | `IOrderable` subjects (`Faction`, `ModuleStack`, `Person`); spawns **Effects** |
| **Effects** | `Game/effects/` | Timed activities (`Producing*`, `Moving`, `Training*`, `Receiving*`, damage/fuel) | `IEffectable` on stacks/persons; runs after orders in the week |
| **Battle** | `Game/battle/` | Combat instance, field, units, tactics | Triggered from movement/attack; reports via `IBattleReporting` |
| **Reports** | `Game/reports/` | `ReportWriter`, line wrapping, event lines | Reads world + `DataFile` for faction-filtered XML sidecar |
| **World model** | `Game/data structures/` | Galaxy graph, factions, stacks, items, techs, offers | Used by every other module via `*.All` and object references |
| **Tests** | `Tests/` | Unit and SampleGame integration | Project reference to `Game`; filesystem fixtures |
| **Website** | `website/` (not yet created) | Public closed PBEM lobby: flavour, `/turns` orders status, `/client` link. Phase 4: `/eta` (transit ETA) and `/battle` (what-if) | Reads **`status.json` only**; links to a future visual tool. Phase 4 islands use **user-entered** text/numbers in the browser. **No** `Game` project reference. [ADR-0007](adr/ADR-0007-public-campaign-website.md), [`delivery/website.md`](delivery/website.md) |

Website is a **separate bounded context**, not a `SpaceAge` namespace folder. The visual tool is a third context (out of scope here).

### World object graph

```mermaid
flowchart TD
  galaxy[Galaxy] --> system[SpaceSystem]
  system --> star[Star]
  system --> planet[Planet]
  planet --> moon[Moon]
  planet --> region[Region]
  planet --> orbit[Orbit]
  region --> stack[ModuleStack]
  orbit --> stack
  stack --> nested[Nested ModuleStack]
  stack --> module[Module]
  stack --> items[ItemStack]
  stack --> person[Person]
  stack --> tech[Technology]
  faction[Faction] --> stack
  faction --> bank[Bank]
```

### Anti-patterns to avoid

- **Do not** introduce a second live `Game` instance without resetting `*.All` (`ClearDictionaries`).
- **Do not** add a database, HTTP API, or SMTP sender without an ADR — the integration surface is files.
- **Do not** collapse unit and SampleGame tests into one fixture style; keep golden files in `Tests/SampleGame/`.
- **Do not** parse orders with a different encoding than 1251.
- **Do not** silently add order types to `OrdersReader` without the matching XML load/save path in `DataFile` / `OrderXml` (today `research` / `see` already diverge — fix both sides together).
- **Do not** convert folders into C# namespaces as a drive-by refactor.
- **Do not** bypass the `DataFile` facade from `Program`, `ReportWriter`, or tests (`new CatalogLoader()` / `Galaxy.LoadXml` as a second public persistence API). Extracts construct collaborators inside `DataFile`.
- **Do not** change Windows-1251 (`loadXmlDocument` / `XmlTextWriter` with `Encoding.GetEncoding(1251)`).
- **Do not** collapse catalog two-pass (`LoadConfigurationItems(true)` then `(false)`); stubs must exist before `requires` / `use-produce`.
- **Do not** change faction XML-report filter semantics: name `"1"` is NPC (unfiltered); skip factions with no stacks; visibility uses existing `Visible(faction)`.
- **Do not** opportunistic-split `DataFile` outside [ADR-0006](adr/ADR-0006-datafile-facade-and-xml-seams.md) phases, and do not split the rest of `ModuleStack` (ADR-0005).
- **Do not** “complete” the capacity save switch (`research` group) or rewrite moon/exit save quirks in the same PR as an extract.
- **Do not** call `Game.exe` over HTTP or invent an engine REST API for the lobby ([ADR-0003](adr/ADR-0003-filesystem-pbem-batch.md), [ADR-0007](adr/ADR-0007-public-campaign-website.md)).
- **Do not** serve `gamein.xml`, `gameout.*.xml`, `data.xml`, `order.*`, or faction reports from the public website. Phase 4 `/eta` may parse a **user-pasted text** excerpt in the browser; it must not persist or POST that paste.
- **Do not** implement the visual tool (star map, unit tree, order editor) inside `website/`. Phase 4 `/battle` is a formula what-if, not a `Battle.cs` port and not `Game.exe`.

## Persistence (`DataFile`)

Dated: 2026-08-18, engine `0.1.141`. Seams: [ADR-0006](adr/ADR-0006-datafile-facade-and-xml-seams.md).

`DataFile` is the **only** persistence entry point for the host and tests: `new DataFile(dir)`, `LoadConfiguration()`, `LoadGame()`, `SaveGame()` / `SaveGame(dir, file, faction)`, plus the granular methods tests already call. Encoding stays 1251.

Catalog load is two-pass because types cross-reference. Game load is ordered: factions → galaxy (objects, then exits) → contracts → orders. Save applies an optional faction filter for XML reports (`ReportWriter`); faction name `"1"` clears the filter.

### Interim (current code, and after catalog/order extracts)

```mermaid
flowchart LR
  host[Program / ReportWriter / tests] --> df[DataFile facade]
  df --> cfg[LoadConfiguration two-pass]
  cfg --> cat[CatalogLoader or DataFile catalog methods]
  df --> game[LoadGame]
  game --> fac[LoadFactions]
  game --> gal[LoadGalaxy then loadGalaxyExits]
  game --> con[Contract.All.LoadXml]
  game --> ord[LoadOrders factory then Order.LoadXml]
  df --> save[SaveGame 1251 XmlTextWriter]
  save --> vis[faction filter NPC 1 / Visible]
```

### Target (after ADR-0006 phases)

```mermaid
flowchart LR
  host[Program / ReportWriter / tests] --> df[DataFile facade]
  df --> io[1251 LoadDocument / XmlTextWriter]
  df --> cat[CatalogLoader stub then fill]
  df --> fac[Faction.LoadXml / SaveXml]
  df --> gal[Galaxy.LoadXml then LoadExits / SaveXml]
  df --> con[Contract.All]
  df --> ord[OrderXml factory then Order.LoadXml]
  cat --> all[static *.All registries]
  fac --> all
  gal --> stacks[ModuleStack.All.LoadXml]
```

`CatalogLoader` and `OrderXml` are internal collaborators, not a second API. `Galaxy` stays a domain type with collection-style XML (like `Contracts`), not a new `GalaxyLoader` god class.

## Turn pipeline (current)

Dated: 2026-08-18, engine `0.1.141`.

```mermaid
flowchart TD
  cli[Program.Main] --> loadCfg[DataFile.LoadConfiguration data.xml]
  loadCfg --> loadGame[DataFile.LoadGame gamein.xml]
  loadGame --> stubs[Request.Load and EventsReaders.Load stubs]
  stubs --> orders[OrdersReader.Load order.*]
  orders --> exec[Game.Execute 13 weeks]
  exec --> reports[ReportWriter.GenerateReports]
  reports --> save[DataFile.SaveGame gameout.turn.xml]
```

**`/check <file>`** skips the turn and calls `OrdersReader.Check` (currently a stub).

### `Game.Execute()` (inner)

1. Increment `Turn`.
2. For `Week` 1..13: clear long/immediate executed flags → `ExecuteOrders()` (factions, then stacks until idle, then persons until idle) → `ProcessBuyOffers()`.
3. After weeks: `ClearUnformed()`, `UpdateBankAccounts()`, `UpdateRates()`, `GenerateOffers()` (several economy methods are incomplete/TODO).

### Order execution (`Orders.Execute`)

Per subject per week: all pending **immediate** orders → at most one **long** order → immediate again → drop non-repeating executed orders. Prefix `+`/`-` builds a condition tree; `@` is unlimited repeat.

## File contracts

Encoding: **Windows-1251** for engine XML, orders, reports, and `error.log` (XML declaration `encoding="windows-1251"`). `status.json` is **UTF-8** and is not an engine file.

| File | Directory | Direction | Role |
|------|-----------|-----------|------|
| `data.xml` | `/data` game dir | in | Static catalog (`<entry>` under star/planet/moon/region/item/race/skill/technology/module) |
| `gamein.xml` | game dir | in | `<game turn="N">` factions, galaxy, persisted `<orders>` |
| `order.*` | `/turn-dir` | in | Text orders (`#faction`, `#modulestack`, `#person`, `#end`; `;` comments) |
| `gameout.{turn}.xml` | game dir | out | Full state after the turn |
| `report.{turn}.{faction}.txt` | turn dir | out | Player report (To/Subject headers for an external mailer) |
| `report.{turn}.{faction}.xml` | turn dir | out | Faction-visible XML subset (when `xml-report` is enabled) |
| `error.log` | CWD | out | RELEASE-only uncaught exceptions (1251) |
| `status.json` | `website/public/` (published to the static host) | out (from `play/` / GM) | Allow-listed lobby status. UTF-8. **Not** an engine output. [ADR-0007](adr/ADR-0007-public-campaign-website.md) |

The engine has **no** network, message bus, or shared database. Faction `email` is metadata for the GM mailer. The public website is a **separate static origin** that may fetch `status.json` only — never `gamein.xml` or reports. Phase 4 tools do not change that file contract.

### Order text sketch

```
#faction <name> "<password>"
#modulestack <id>
<optional +/-><repeat|@><command> <args>
#end
```

Subjects implementing `IOrderable`: `Faction`, `ModuleStack`, `Person`.

## Test layers (architecture-aligned)

One test assembly: `Tests.dll`. Layers are **namespaces**, not extra `.csproj` files. Do not add `*.UnitTests` / `*.IntegrationTests` projects unless an ADR says so.

| Layer | Namespace | Typical scope | Placement |
|-------|-----------|---------------|-----------|
| **Unit** | `UnitTests` | Single type or small in-process collaboration; XML fixtures `Tests/data.xml` + `Tests/gamein.xml`, or owned copies under `Tests/fixtures/`; no SampleGame goldens | `Tests/T*.cs` (`TOrder`, `TUse`, `TGetGiveHas`, `TMove`, `TFormStack`, `TSee`, `TTrain`, `TDataFile`, `TBattle`, `TMarket` (includes `TContract` fixture), `TResearch`, `TConsume`, `TRepair`, `TDiplomacy`, `TModuleStack(s)`, `TPoint2D/3D`, `TNamedObject`, `TTests`) |
| **Module** | — | **Not used.** Do not invent a third layer. | — |
| **Integration** | `IntegrationTests` | Multi-file SampleGame turns, report goldens, program smoke | `Tests/SampleGame/` (`SampleGame`, `TProgram`), `Tests/TReport.cs` |

**Rules**

- Put new tests in **Unit** unless the behavior can only be proved by a full turn or golden report/XML — then **Integration**.
- Base helpers live on `TTest` (`compareFiles`, `executeOrder`, 1251 file load). Teardown must clear static registries.
- Load **committed** files only. SampleGame tests must not `copyFile` (or otherwise write) the next turn’s `gamein`. `_4a` / `_6a` may save a generated contract XML and compare it to the checked-in `gamein.2_contract.xml` / `gamein.3_contract.xml`; they must not be the next execute-turn’s runtime input unless that XML is already committed.
- Unit tests must not load `Tests/SampleGame/` worlds. Campaign-shaped cases live under `Tests/fixtures/` (for example `scout-declare/`, `has-order-production/`).
- There are **no** NUnit `[Category]` / `[Trait]` attributes today. Optional filters:
  - Unit: `--where "namespace == UnitTests"`
  - Integration: `--where "namespace == IntegrationTests"`
- Fast local/cloud default: **entire** `Tests.dll` (`.cursor/run-tests.sh` on Mono / `vstest.console` on Windows).
- Integration tests for SampleGame turns 4–5 are `[Ignore("not ready")]` — do not enable them without goldens. Turns 1–3 are independently runnable from committed `gamein` files.
- `DataFile` extracts (ADR-0006): characterize with `TDataFile` (unit) and SampleGame load/save goldens (integration). Do not add a third test layer.

## Stub / incomplete boundaries

Treat as **not live integrations** until implemented with tests:

| Type | Status |
|------|--------|
| `Request.Load` | Stub |
| `EventsReaders.Load` / `Events.Execute` | Stub |
| `OrdersReader.Check` | Stub |
| `Game.GenerateOffers` / `UpdateRates` | Partial / TODO |
| SampleGame turns 4–5 | Ignored |

## Revision

- 2026-08-18: Persistence seams for `DataFile` (ADR-0006). Interim/target diagrams; anti-patterns for facade, 1251, two-pass catalog, faction XML-report filter. Engine citation `0.1.141`.
- 2026-08-29: Website bounded context (ADR-0007). Status via `status.json`; do not HTTP-call `Game.exe` or publish `gamein.xml`.
- 2026-08-29: Phase 4 lobby tools (`/eta`, `/battle`) — client-side user input only; still no engine files on the origin.
