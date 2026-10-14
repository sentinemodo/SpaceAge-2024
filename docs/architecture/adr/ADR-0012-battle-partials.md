# ADR-0012: Battle partial seams

Date: 2026-10-14  
Status: **Accepted**  
Engine cited: `Game/Program.cs` → `EngineVersion` **0.8.004** (not bumped for this split)

## Context

`Battle` is the largest unsplit engine type: one class owns side joining, fire resolution, hangar launch, victory, loot, capture, and battle reports. A combat fix can change capture and report text in the same edit. Tactics are already small types. `BattleSimulatorRunner` calls `Battle`; it does not replace it.

`ModuleStack` combat hooks stay on `ModuleStack` ([ADR-0008](ADR-0008-modulestack-decomposition.md)). This ADR does not reopen that split.

## Decision

Keep a **single** `Battle` type. Move methods into named `partial` files. Private fields and `Execute(int week)` stay in `Game/battle/Battle.cs`. `Battle.All` stays. Public and internal method names stay the same. No new battle service type, no DI, no namespace split, and no `EngineVersion` bump.

| File | Owns |
|------|------|
| `Battle.cs` | Fields, `Execute`, empty `executeMovement` |
| `Battle.Report.cs` | `prepareBattleReports`, `report*`, `Report` |
| `Battle.Hangar.cs` | `launchHangarCraft`, `addLaunchedHangarCraft` |
| `Battle.Engagement.cs` | Constructors, side joining, `StartAtLocations`, `ShouldCommenceBattle` |
| `Battle.Fire.cs` | `executeAttack` through hit location and GM fire logs |
| `Battle.Victory.cs` | Capture reset, evade, loot, scavenge, `ApplyCaptures`, `ApplyVictoryResolution` |

Each partial is behavior-preserving. Gate with `Tests/TBattle.cs`, `Tests/TBattleSim.cs`, and SampleGame battle report goldens. Do not regenerate goldens for a file move.

## Not in this ADR

- Tactic types, `BattleSimulatorRunner`, `BattleSimulatorTemplates`
- `ModuleStack.Combat` / `ModuleStack.BattleReport`
- Changing round order, damage formulas, or capture rules
