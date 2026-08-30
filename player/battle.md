# Battle (rules of engagement)

Checked **30 Aug 2026** against engine **0.1.148**.

Sources: `Game/battle/Battle.cs`, `Game/battle/Battles.cs`, `Game/battle/CombatMatchup.cs`, `Game/battle/ETactic.cs`, `Game/Game.cs` (`ExecuteBattles`), `Game/reports/ReportWriter.cs` (blank line before `Battles report:`), `Game/data structures/ModuleStack.cs` (attack, defense, initiative, tactics, `IsArmed`, `HasOperationalModules`, `GetFiringModules`), `Game/data structures/ModuleType.cs` (`IsShuttleUnit` / `IsHangarCraft` / `IsDroneBay`, `WeaponGroup` / `Resists` / `ArmorModule`), `Game/data structures/Faction.cs` / `FactionAttitude.cs`, `Game/orders/AttackOrder.cs`, `CaptureOrder.cs`, `DeclareOrder.cs`, `TacticOrder.cs`, `SetOrder.cs`, `Game/game/CatalogLoader.cs` (`weapon-group`, `resists`, `armor-module`). Catalog bonuses: `Tests/data.xml` (`attack`, `defense`, `damage`, `initiative` on modules, techs, skills, items). SampleGame catalog is **flat**: no `weapon-group` / `resists` / `armor-module` attributes, so typed-matchup multipliers, shield intercept, and armor hit-weight do not fire there.

Not source of truth: `Game/documentation/Rules.txt` combat chapters (Alderson CONVERT / 60% command / mixed leftover modules). Ground and space use the **same** battle loop; unused `GroundUnit` / `BattleField` do not run.

## When a battle starts

Each of the **13 weeks**, after orders, sick-bay heal, medical consume, contracts, and buy offers, `Game.ExecuteBattles` calls `Battle.StartAtLocations`.

A battle starts when:

- A **root**, **armed**, **operational** (`HasOperationalModules`) located stack has `Owner.AttitudeTowardUnit(other) == enemy` toward another stack at the **same location**, and
- That other stack still has intact (not wrecked) modules.

Fully disabled armed stacks do not initiate battles. Nested stacks are skipped (`!stack.IsRootModuleStack`). `IsArmed` walks nested children, so a hull with a loaded fighter drone bay is one armed **root**; the drones themselves are not roots and cannot initiate.

`StartAtLocations` scans `ModuleStack.All` in dictionary / load order. The first qualifying armed root with enemy attitude toward another stack here becomes the **initiator** (`Battle(initiator, target)`). SampleGame loads hull `[101]` before Gelvaren shuttle `[117]`. Hangar launch has not run yet: it is later in `Execute`, **after** the Round 1 Attackers/Defenders roster (not before `Round 1:`).

`STACK OUT` of fighter drones **before** the fight makes them independent armed roots: they can initiate and they fire from round 1. Leave them nested in a `drnbay` if the hull should initiate and drones should skip round 1.

At most **one battle per location per unordered faction pair** per week (`FactionPairKey` + location). Further enemy pairs at that location that week are skipped.

`ATTACK <unit>` and `CAPTURE <unit>` set that unit to **enemy** (one-way). `DECLARE FACTION` / `DECLARE UNIT` set stance without starting a fight by themselves.

## Sides

Constructor `Battle(initiator, target)` builds two lists.

**Attackers** — armed **root** stacks at the initiator’s location that `canJoinAsAttacker` (`IsArmed` and `HasOperationalModules`) and `joinsAttack`:

- Same faction as the initiator, or
- `AttitudeToward(initiator’s faction) == ally`

Unarmed stacks, fully disabled armed stacks, and nested stacks (unless they are the initiator) do not join the attacker list. The initiator is added only if still operational and armed, even if the location scan missed it. Constructor sides are built **before** hangar launch. Nested shuttle-units (`ModuleType.IsShuttleUnit` / hangar craft: drones `[alndrn]` group `shuttle`, and shuttles `[shuttl]` group `production`) in a `drnbay` are not attacker-list combatants yet. The Round 1 Attackers/Defenders roster still prints them **indented** under the bay (not a sibling root). After that roster, `launchHangarCraft` detaches them. `orbrkt` is not a shuttle unit; it stays nested.

**Defenders** — stacks at the target’s location that still have intact modules (`HasIntactModules`) and `joinsDefense`. Disabled-but-intact stacks can still be collected as defenders:

- Same faction as the target, or
- `AttitudeToward(target’s faction)` is **ally** or **friendly**

Every **root** that qualifies is a combatant. Nested stacks are scanned (`isDefenderStack`: the target itself, armed stacks not under the initiator’s root, siblings under the target’s parent) but **skipped** when their **root already joins** (that root has intact modules and `joinsDefense`). Nested HQ or cargo under an NPC city whose root does **not** join still appear as their own defenders. The target is added if the scan missed it.

Nested armed modules under a joining parent still **print indented** in that parent’s `BattleReport`. They are not a second same-id sibling on the defender list.

**Firing:** `executeAttack` walks `GetFiringModules` on each combatant: the stack’s own operational combat-armed modules, then nested stacks except hangar craft. Nested shuttle-units still in the bay are skipped; nested `orbrkt` (military, not a shuttle unit) still fires as a weapon on the parent. One shot sequence per combatant. The fire line names the **combatant** and the **weapon type** (`shuttle [117] fires orbital rocket launcher`), not a nested stack firing as its own combatant plus the parent firing the same launcher.

**Sit out:** a third faction that would qualify as **both** attacker (ally of initiator) **and** defender (ally or friendly to the target) joins **neither** side (`sitsOutBothSides`).

Participants for reports are the distinct owners of both lists.

## Diplomacy

Attitudes (one-way): `enemy` (0), `hostile` (1), `neutral` (2), `friendly` (3), `ally` (4).

| Stance | Combat |
|--------|--------|
| **enemy** | Needed to **start** a battle (`AttitudeTowardUnit`). Firing happens because the stacks are already on opposite sides. |
| **hostile** | Comment: interdict, do not fire. **Does not** start a battle. **Does not** join a side. |
| **neutral** | Default toward known factions. No join, no start. |
| **friendly** | Joins **defense** of that faction, not attack. |
| **ally** | Joins **attack** with that faction and **defense** of that faction. |

Resolution: per-unit declaration (`UnitAttitudes`) else stance toward the unit’s owner (`Attitudes`) else `DefaultAttitude`. Unknown factions use `UnknownAttitude` (baseline **hostile**). Stale per-unit stances are dropped at turn start and after each week’s battles (`DropStaleUnitAttitudes`): missing ids, empty stacks, or stacks now owned by the declaring faction (including after capture).

`SET AVOID TRUE` (`IsAvoiding`) is **not** a battle tactic. After a stack fires, `considerRetreat` only prints that an avoiding unit tries to escape and **fails** (or cannot, if immobile). It does not leave the fight. `SET ONLINE TRUE|FALSE` is also not a battle tactic; it only toggles stack and module `Online` (captured modules start deactivated).

## Turns and rounds

- **Turn:** 13 weeks. Battles may start **every week** if enemy armed roots share a location.
- **Battle:** up to **`MaxRounds` = 10** rounds, or until one side’s list is empty.
- Each round: print attacker/defender battle reports; **after Round 1’s roster only**, hangar launch as a **ship action** (not an attack): `{hull} launches {craft} from fighter drone bay [drnbay].`; then every operational combatant **fires** (see initiative), then evade-leave is checked. The carrier does **not** fire the empty bay (no `fires fighter drone bay` / 0-damage shot) and skips its own round-1 fire. Launched craft do not fire in round 1; they fire from round 2 as roots.
- End lines: “Battle won by attackers/defenders” or “Battle ended indecisively.”
- `ReportWriter` inserts a **blank line** before the `Battles report:` block when any battles ran. `Battles.Report` prints `Battles report:` plus a blank after the header, then each battle’s report lines followed by a **blank line** (not before the first battle body).
- Capture damage on modules is **zeroed at the start of each battle** (`resetCaptureDamage`). Hit-point `Damage` is not reset here.

`executeMovement` is empty. In-battle orders (move/use in the round) are comments only.

## Initiative (who shoots when)

All attackers and defenders are grouped by `ModuleStack.Initiative` in a `SortedList` and fire **lowest initiative first**, then higher.

`Initiative` = this stack’s `InitiativeBonus` + nested stacks’ `InitiativeBonus`.

`InitiativeBonus`:

- If **root:** `InitiativeManeuverabilityBonus` = `int(energyReserveRatio + massCapacityRatio) * 10` (energy produced/required and mass capacity/mass; 0 if a denominator is 0).
- Plus this stack’s **module type** `initiative` (catalog, e.g. fighter drone `[alndrn]` 20).
- Plus sum of **technologies** `initiative` on the stack.
- Plus sum of **people** `Initiative` (each person’s skills).

Catalog examples: military tactics `[miltac]` initiative 5 (held as a tech copy on a command stack); skills frigate pilot / armor platoon leader / infantry battalion commander initiative 5.

Same initiative value: those stacks fire in list order (not shuffled).

## Tactics (live)

`TACTIC` (`TacticOrder`) on a modulestack. Immobile stacks may only set **destroy**. `CAPTURE` or `TACTIC capture` / `TACTIC evade` on an immobile unit fails once per week (`CAPTURE failed. Immobile units may only use destroy.` / `TACTIC failed. Immobile units may only use destroy.`) and marks the order executed; `prioritize` still applies.

| Order | Effect in `Battle` |
|-------|-------------------|
| `TACTIC destroy` | Firing tactic **destroy** (default). |
| `TACTIC capture` | Firing tactic **capture**. Exclusive with destroy. |
| `TACTIC evade` | Stance: half to-hit; command/propulsion half hit-weight; leave after **two consecutive rounds unhit**. May coexist with destroy/capture. |
| `TACTIC prioritize armed` | Prefer an armed target; may shoot a disabled-but-armed stack. Exclusive with other prioritize kinds. |
| `TACTIC prioritize command` | Prefer a command-group stack. Disabled command is included like cargo. Exclusive with other prioritize kinds. |
| `TACTIC prioritize storage` | Prefer a storage-group stack (`IsCargoStack()`, e.g. `cargob`). Disabled cargo is included like prioritize command. Exclusive with other prioritize kinds. Coexists with capture/destroy. |

`FiringTactic` is **capture** if `HasCapture`, else **destroy**. XML `disable` is stored then applied as **destroy** (`ApplyTactic` else-branch). `ETactic` also lists split, disarm, conquer, retreat, attackStrongest…, closeIn, longRange, support — **not** wired in `executeAttack` / `findTarget`.

Target pick: preferred unit id from `CAPTURE`/`ATTACK` if still in the enemy list; else prioritize armed, else command, else storage; else first armed, else first intact.

Only **operational** modules fire. Unarmed stacks skip `executeAttack`. Nested shuttle-units in a bay are skipped in `GetFiringModules` (the hull does not shoot them while docked). Nested `orbrkt` is not skipped.

**Armed:** formed, and module group **military** or **shuttle**, or group **vehicle** / **infantry** with `attack > 0`, including nested stacks. Group `production` shuttles (`shuttl`) are hangar craft by type id, not by group; a shuttle is armed when nested military (e.g. `orbrkt`) makes `IsArmed` true.

**Hangar launch:** a **ship action**, not an attack. Not before `Round 1:`. After the Round 1 Attackers/Defenders roster (drones still nested in `drnbay`), `launchHangarCraft` detaches shuttle-units nested in a fighter drone bay to the location (`STACK OUT`) and joins the parent’s side: `{hull} launches {craft} from fighter drone bay [drnbay].` The empty bay is not a weapon: the hull does **not** print `fires fighter drone bay` (no 0-damage shot). The carrier is recorded in `hangarLaunchCarriers` and `executeAttack` returns for it in round 1, so it fires nothing that round. Launched craft **do not fire in round 1** (`executeAttack` returns if `round == 1` and the stack is in `launchedHangarCraft`); from round 2 they fight as roots and pay their own quarterly cash upkeep. There is no player `LAUNCH` verb. Launched drones have a slow space move at shuttle speed (`speed` 1, mass-capacity 750); with helium-3 they are not immobile. Drones that `STACK OUT` in orders before the fight never go through this path.

Fighter drones: high module `initiative` (20), small `damage` and hit points, cargo capacity 1 (one helium-3 `[heliu3]`), helium-3 fuel (1 per 13 weeks).

## Chance to hit

Each **operational firing module** on the shooter (and nested armed stacks) rolls once.

```
chance = (shooter.Attack + nested.Attack) / 2     // integer
if shooter.ModuleType.WeaponGroup is non-empty:
    chance = ceil(chance * CombatMatchup.ChanceMultiplier(weaponGroup, target.ModuleType.Resists))
if target.HasEvade:     chance = chance / 2
if target.IsImmobile:   chance = chance + chance / 2

`IsImmobile` is the root’s ability to relocate: formed, `IsActive` (not disabled/partially disabled), and a usable move (space via self or nested drive with fuel, or ground with fuel). Nested modules inherit the root — a factory on a moving ship is not immobile; the same factory on a city is. Independent tanks/infantry that are roots use their own disable and fuel state. Launched fighter drones have shuttle-speed space move and are mobile when fueled. Gun placements on a city stay immobile for the combat to-hit bonus.
dice   = shooter.Attack + nested.Attack + target.Defense + nested.Defense
roll   = uniform 1 … dice   (Sequence; if dice is 0, roll is forced to 1)
hit    if roll <= chance
```

The report prints `(chance: C/D)`.

**Typed matchup** (`CombatMatchup.ChanceMultiplier`) runs only when the **shooting combatant’s** module type has a non-empty `weapon-group` (not the nested weapon that fired). `resists` is the **target combatant’s** module type, not the hit-location module. Multipliers: `laser` vs `shield` → 0.5, else 1.5; `kinetic` vs `armour`/`armor` → 0.5, else 1.5; `missile` vs `pbpd` → 0.5, else 1.5; `drone` vs `ew` → 0.5, else 1.5; any other group → 1.0. Empty `weapon-group` (SampleGame catalog) skips this step.

`Attack` on a formed stack: `QuantityActive * moduleType.Attack` + technologies’ `attack` + people (skills’) `attack` + eligible cargo equipment `attack` (up to `min(item quantity, QuantityActive)` per item type, gated by `use-allowed-by module-type-group`). Nested stacks add through `ModuleStacks.Attack()`. Same shape for **Defense** (equipment `defense`).

Item `attack` / `damage` / `defense` / `initiative` on cargo count when the item’s `use-allowed-by module-type-group` matches the stack’s module `group`. Each item type contributes up to **`min(item quantity, QuantityActive)`** copies (one suit per active module, not one per stack). Equipment `attack` and `defense` feed `ModuleStack.Attack` / `Defense` (and thus `getChance` dice). Shot damage uses module `damage` plus item `damage` on the **first** boosted shots only: budget = sum of `min(item quantity, QuantityActive)` over eligible damage items; each boosted shot adds one copy of each eligible item’s `damage` until the budget is exhausted.

If `Defense` is 0, chance is half of attack and dice is attack, so about **50%** before evade/immobile.

## Damage

On a hit, a module on the target is chosen by **hit weight**, then:

- **Shield intercept** (before the HP/capture split): if the target or a nested stack has a non-wrecked shield module (`Resists == "shield"`), `CombatMatchup.ShieldIntercept` takes **floor(90%)** of the weapon’s `Damage` as HP on that shield (capped by remaining shield HP). The remainder is the shot that hits the rolled location. No dedicated intercept line is printed. SampleGame catalog has no `resists="shield"` modules, so this is skipped.
- **Destroy:** `hpDamage =` remaining weapon damage after intercept. All of it is hit-point damage.
- **Capture:** `hpDamage = 25%` of that remaining damage (integer); the rest is **capture** damage. `HpDamageFromShot` / `CaptureDamageFromShot`. If the **hit location** is armor (`ArmorModule` or `Resists` `armour`/`armor`), both HP and capture from that shot are **0**.

Both are capped by remaining pool `HitPoints - Damage - CaptureDamage`.

- `Damage >= HitPoints` → **wrecked**.
- Capture: `Damage + CaptureDamage >= HitPoints` and not wrecked → **capture complete** (module offline, peeled to the **battle initiator’s owner** — `this.attacker.Owner`, not necessarily the firing stack). The peeled module goes onto a **new stack** with a 6-character id `c` + 5 digits (`c00001`, `c00002`, …), skipping ids already in `ModuleStack.All`. Not `c` plus the source stack id.

Hit weight for a stack: `DamageCapacity * intact module count`. **Armor** stacks (`ArmorModule` or `resists` armour/armor) **×5**. Command or propulsion: **×2** if the shot is capture, **÷2** if the target is evading. Nested stacks of the **same owner** are included; other owners contribute 0 to this roll. Nested `orbrkt` is on that roll (not a shuttle unit). Nested shuttle-units already launched are roots and are not hit as cargo of the carrier.

Disabling the last operational module can drop the stack from the battle (command/energy flavor lines). Losing the last military module marks unarmed; losing propulsion marks immobile.

## Equipment and officers

**Modules (weapons):** `attack` feeds to-hit; `damage` is the shot (then shield intercept); `defense` feeds the defender’s dice; `hit-points` / `DamageCapacity` size the hit-location roll. Catalog `weapon-group`, `resists`, and `armor-module` are live in `Battle` when set; SampleGame `Tests/data.xml` omits them. Quantity active scales attack/defense. Nested military/vehicle/infantry fire as separate weapons on the parent’s shot sequence. Nested shuttle-units (`IsHangarCraft`: group `shuttle`, or types `alndrn` / `shuttl`) are skipped while still in the bay. `orbrkt` is military, not a shuttle unit: it stays nested, fires on the shuttle’s sequence, and can be the hit location. Drones `[alndrn]` are catalog group **shuttle** (not military); shuttles `[shuttl]` stay group **production** so orbit `USE` still works. Both are shuttle units. `GetFiringModules` also skips fighter drone bays (`IsDroneBay`), so an empty bay is not a weapon.

**Technologies** on the stack: `attack`, `defense`, `initiative` summed (e.g. `[miltac]` initiative 5). Battle techs are **held copies**, not `USE`d.

**Officers / people:** each person’s **skills** add `attack`, `defense`, `initiative` to the stack they are on (`People.Attack` / `Defense` / `Initiative`). Catalog: armor platoon leader and infantry battalion commander +5 attack and +5 initiative; frigate pilot +5 defense and +5 initiative; space station command +5 defense. Skill `produce effect="effective attack"` is catalog text; `Battle` does not read it.

**Items:** rocket launchers and other personal equipment add `attack`/`damage`/`defense`/`initiative` when `use-allowed-by module-type-group` matches the carrier stack’s module `group` (up to `min(item qty, QuantityActive)` per item type). Shot damage: module `damage` on every shot; item `damage` on the first boosted shots only (budget = eligible item copies capped by active modules). Campaign catalog items (`prllsr`, `psnarm`, etc.) follow the same rules as `[rctlnc]`.

## Evade leave

After the round’s shots: an evading stack that was **not** hit this round increments a counter; at **2** consecutive unhit rounds it “evades and leaves combat” and is removed from both lists. A hit resets the counter.
