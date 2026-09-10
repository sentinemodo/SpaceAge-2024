# ModuleStack refactor — implementation checklist

Last updated: 2026-09-09  
Decision: [ADR-0008](../adr/ADR-0008-modulestack-decomposition.md)  
Engine cited: current `master` (~2,960 lines across three partial files)

This is the **TDD execution plan** for splitting `ModuleStack` into named `partial` files. The ADR names seams and forbids DI, namespace splits, new public service types, and a big-bang rewrite. This file is the commit-by-commit checklist for one refactor PR (or one commit per phase across PRs).

## Vehicle

1. **First commit:** architecture only (this file + ADR-0008). No C#.
2. **Later commits:** one ADR phase per commit. Pause between planned steps (TDD rule). Stay green before the next phase.
3. **Public API stays on `ModuleStack`.** Do not retarget `Battle`, `OrdersReader`, `ReportWriter`, `DataFile`, or tests to new public types.
4. Do not bump `EngineVersion` for a behavior-neutral file move. Do not regenerate SampleGame goldens for whitespace-only diffs.
5. Legacy `Game/Game.csproj` does not glob: add `<Compile Include="data structures\ModuleStack.*.cs" />` for each new partial next to existing `ModuleStack.cs` entries.
6. [ADR-0005](../adr/ADR-0005-modulestack-partials.md) partials (`Ownership`, `Upkeep`) are **not** moved again unless a bugfix requires it.

## Every commit must stay green

| Layer | Proof |
|-------|--------|
| **Unit** `Tests/TModuleStack.cs`, `Tests/TModuleStacks.cs` | Formation, size, capacity, consume |
| **Unit** movement | `TMove`, `TMoveMode`, `TMoveDuration`, `TMoveEnvironment`, `TAlderson`, `TBelt` |
| **Unit** combat | `TBattle`, `TItemCombat`, `TUseRepairEffect`, `TRepair` |
| **Unit** reports | `TReport` |
| **Unit** persistence | `TDataFile` (stack load/save, module damage round-trip) |
| **Unit** other `Tests/T*.cs` | Any fixture that constructs stacks or runs orders on stacks (`TResearch`, `TConsume`, `TUse`, `TGetGiveHas`, `TFormStack`, `TSee`, `TTrain`, `TMarket`, `TDiplomacy`, `TCampaign`, …) |
| **Integration** `Tests/SampleGame/` | Load/save goldens, execute-turn reports. Do not enable ignored turns 4–5 |

Characterize with existing tests. Add assertions only when a seam has no proof. New goldens need `/player` plus explicit human approval.

## Commits

| # | Phase | Production change | Prove with |
|---|--------|-------------------|------------|
| 1 | Docs | This checklist + ADR-0008. No C#. | n/a |
| 2 | 0 — characterize | Optional: short comments in ADR or test docstrings mapping `#region` → future partial. Tests only if a seam lacks coverage (e.g. `HasPresence` recursion). **No** production move. | Full `Tests.dll` or targeted `TModuleStack` + `TDataFile` + `TBattle` + `TReport` |
| 3 | 1 — `ModuleStack.Xml.cs` | Move `LoadXml` / `SaveXml` from main file. Same method bodies; delegate to existing collection `LoadXml`/`SaveXml` unchanged. | `TDataFile` save/load, `SaveLoad_PersistsModuleDamageBetweenTurns`, SampleGame `*_saved.xml` |
| 4 | 2 — `ModuleStack.Movement.cs` | Move `movement` region (`MovingTo`, `IsRoot`, `MoveModes`). | `TMove*`, `TAlderson`, `TBelt`, SampleGame move orders if exercised |
| 5 | 3 — economy + effects | `ModuleStack.Economy.cs` (`HasBankAccess`, `Offers`, `ResearchPoints`). `ModuleStack.Effects.cs` (`Effects`, `EventReports`, `ExecutedLongOrder`). | `TMarket`, `TResearch`, `TConsume`, effects/order tests touching stacks |
| 6 | 4 — `ModuleStack.ItemStacks.cs` | Move `IItemStacksHolder Members` region. | `TModuleStack`, `TGetGiveHas`, `TUse`, `TConsume` |
| 7 | 5 — `ModuleStack.Orders.cs` | Move `IOrderable Members` + `Execute(int week)`. | `TOrder`, `TTrain`, `TFormStack`, SampleGame order execution goldens |
| 8 | 6 — `ModuleStack.Activation.cs` | Move `requirements`, `settlement modules`, `energy modules` regions. | `TModuleStack`, `TUse`, `TResearch`, `TSee`, campaign/fixture tests using activation |
| 9 | 7 — `ModuleStack.Combat.cs` | Move `combat` region (~400 lines). Optional static pure helpers **only** if file remains unreviewably large (ADR-0008). | `TBattle`, `TItemCombat`, `TRepair`, `TUseRepairEffect` |
| 10 | 8 — `ModuleStack.Reporting.cs` | Move `report` / `IReporting Members` (not battle report). | `TReport`, SampleGame report goldens |
| 11 | 9 — `ModuleStack.BattleReport.cs` | Move `battle report` region. | `TReport`, `TBattle`, SampleGame battle report lines |
| 12 | 10 — `ModuleStack.Relations.cs` | Move `relations` region last (parent, owner, location, nesting, formation). Main `ModuleStack.cs` retains constructors + `All` + any shared glue. | `TModuleStack`, `TModuleStacks`, `TFormStack`, `TDataFile`, SampleGame load/save |

After phase 10, verify main `ModuleStack.cs` is mostly constructors/registry; no orphaned `#region` blocks for moved concerns.

## Out of this PR series

- **`Person.cs` decomposition** — parallel six-interface type; needs its own ADR after ModuleStack is stable.
- **`Person.Location` null-branch fix** — separate bugfix PR with regression test; not during extract commits.
- **Order factory duplication** (`OrderXml` vs `OrdersReader`) — ADR-0006 concern, not stack partials.
- **Combat/report formula or wording changes** — follow-up PR, failing tests first.
- **DI, `*.All` replacement, namespace splits, SDK migration** — [`../future-work.md`](../future-work.md).
- **SampleGame turns 4–5**, stub pipeline (`Request`, `Events`, `OrdersReader.Check`), encoding change.

## After merge

- [ADR-0008](../adr/ADR-0008-modulestack-decomposition.md) status is **Accepted** (2026-09-09).
- Supersedes [ADR-0005](../adr/ADR-0005-modulestack-partials.md) “do not split the rest” **for the named seams only**; ownership/upkeep partials unchanged.
- Optional follow-up: `Person` partial ADR; static combat helpers if Phase 7 partial is still too large.
- Remaining modernization stays in [`../future-work.md`](../future-work.md).
