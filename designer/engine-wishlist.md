# Engine wishlist (game designer → TDD)

Designer does **not** implement these. TDD adds a failing test first. Campaign XML stays loadable without them.

| Need | Objective | Suggested surface |
|------|-----------|-------------------|
| Moon constructed with moon `@name`, not planet name | Unique moon ids (`M00001`) survive load | `DataFile.LoadGalaxy` moon constructor |
| `loadGalaxyExits` walks moon regions and orbits | Moon maps and orbit↔surface space hops. Campaign currently uses **planet-region ↔ planet-region** space exits (Arbor/Anvil spaceports ↔ local dust/belt ↔ the pair hop) because those load today | Second pass over all `Region` / `Orbit` |
| Space transit time from ΔAU × drive `speed` | L10 ark crosses a system in weeks; L0 shuttle does not | `Moving` / exit duration formula; keep `AU` on planets |
| System `X Y Z` loaded | Interstellar placement (still no FTL) | Uncomment coordinate assign |
| Module group `capital` (or `ark`) | L10 hull is not a “frigate” | `EModuleTypesGroup` + `getModuleTypeGroup` |
| Contract triggers: `survive-weeks`, `destroy-stack`, `region-resource-below` | Bombardment, fauna, pirate hunt | `IContractTrigger` + XML attrs |
| `Events` pipeline for timed spawns | Alien reactivation, impact week | `Game/Events.cs` (today stub) |
| `use-produce effect` execution | Catalog `repair` and future ECLSS effects | `UseOrder` / `EProductionType.Effects` |
| Skill children (`usable-in`, cure-chance) applied in battle/medical | Officers matter on arks and labs | `SkillType` fill-pass |
| Item `radiation` / equipment bonuses in combat and vacuum | Vests, suits, dosimeters | `ItemType` + consume/medical |
| New `location-type` `atmosphere` | Gas-giant cloud regions | `LoadLocationType` |
| Asteroid belt as first-class or typed planet exits | Belt drift between rocks | planet `abelt` already; optional space exits among belt regions |
| Drive-dependent `fuel` for `plsdv` / `arkeng` using `heliu3` | High-Isp logistics | already expressible in module `fuel`; verify `Moving` consumes it on space hops |
| Gas-giant cloud `deutrm`/`heliu3` | Dictionary wants orbit resources on `gasgnt`; until then seed ice-moon surfaces | same as `atmosphere` location-type row above — no extra token |
| `SEE` / scan bonus from `survsc` | Anomaly gameplay | `SeeOrder` + module flag |
| Sick-bay heal cadence (`sckbay`) | Convert wounded crew on a timer so a ward actually saves people; medicines speed the ward without double-taxing the weekly `wndtrn` `medici` bill | Module `<effect type="heal" value="2" target="wndtrn" weeks="4" weeks-with-item="1" with-item-value="4" consume-item="medici" consume-quantity="1"/>`. **Per module quantity.** Each week, **before** race `medici` deduct and **before** week-13 25/50/25: if `medici` ≥ 1, convert up to 4 `wndtrn` → `terran` and consume 1 `medici` per conversion (not per occupied bed-week); a medici-funded week does **not** also tick the unmedicated clock. Else increment a 4-week clock and convert 2 with no item. Remaining `wndtrn` then pay weekly `medici` and, on week 13, roll death/stay/recover. Do not convert `madtrn`. Ignore `crwqrt` 0.1 and `medfac` 1.0 stacked heal until this lands |
| `use-allowed-in module="sckbay"` | Shipboard pharmacy runs in the sterile ward, not in any habitat (`crwqrt`, `jail`) | Extra attr on `use-allowed-in` / `operation-allowed-in`: `module` = module type id. Until then campaign `pharms` uses live `module-type-group="habitat"` (too broad, still loads) |
| Typed weapon group vs matching defence | Laser vs shield, drone vs EW, missile vs point-blank PD, kinetic vs armour. Same four groups at capital / fighter-drone / infantry-tank. Today Battle is flat `attack`/`defense`/`damage` (`player/battle.md`) | Catalog attrs e.g. `weapon-group="laser"` `resists="shield"` on modules/items; `Battle` matchup table. **Do not implement in this designer pass.** |
| Item `attack`/`damage`/`defense` in battle | Live `rctlnc` (and campaign `prllsr` `prlgun` `psnarm` `psnshd` `psnew` `prxgrd`) must affect to-hit and shot damage. Today `ModuleStack.Attack` ignores items (`player/battle.md`) | Add item stats into `getChance` / shot damage / defense dice |

When TDD lands a row, tick it here with engine version and date, then migrate any parked catalog lines from `designer/catalog.md` into `campaign/data.xml`.
