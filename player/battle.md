# Battle (rules of engagement)

Checked **20 Aug 2026** against engine **0.1.142**.

Sources: `Game/battle/Battle.cs`, `Game/battle/ETactic.cs`, `Game/Game.cs` (`ExecuteBattles`), `Game/data structures/ModuleStack.cs` (attack, defense, initiative, tactics, `IsArmed`, `HasOperationalModules`, `GetFiringModules`), `Game/data structures/Faction.cs` / `FactionAttitude.cs`, `Game/orders/AttackOrder.cs`, `CaptureOrder.cs`, `DeclareOrder.cs`, `TacticOrder.cs`, `SetOrder.cs`. Catalog bonuses: `Tests/data.xml` (`attack`, `defense`, `damage`, `initiative` on modules, techs, skills, items).

Not source of truth: `Game/documentation/Rules.txt` combat chapters (Alderson CONVERT / 60% command / mixed leftover modules). Ground and space use the **same** battle loop; unused `GroundUnit` / `BattleField` do not run.

## When a battle starts

Each of the **13 weeks**, after orders, sick-bay heal, medical consume, contracts, and buy offers, `Game.ExecuteBattles` calls `Battle.StartAtLocations`.

A battle starts when:

- A **root**, **armed**, **operational** (`HasOperationalModules`) located stack has `Owner.AttitudeTowardUnit(other) == enemy` toward another stack at the **same location**, and
- That other stack still has intact (not wrecked) modules.

Fully disabled armed stacks do not initiate battles.

At most **one battle per location per unordered faction pair** per week (`FactionPairKey` + location). Further enemy pairs at that location that week are skipped.

`ATTACK <unit>` and `CAPTURE <unit>` set that unit to **enemy** (one-way). `DECLARE FACTION` / `DECLARE UNIT` set stance without starting a fight by themselves.

## Sides

Constructor `Battle(initiator, target)` builds two lists.

**Attackers** — armed **root** stacks at the initiator’s location that `canJoinAsAttacker` (`IsArmed` and `HasOperationalModules`) and `joinsAttack`:

- Same faction as the initiator, or
- `AttitudeToward(initiator’s faction) == ally`

Unarmed stacks and fully disabled armed stacks do not join the attacker list. The initiator is added only if still operational and armed, even if the location scan missed it.

**Defenders** — stacks at the target’s location that still have intact modules (`HasIntactModules`) and `joinsDefense`. Disabled-but-intact stacks can still be collected as defenders:

- Same faction as the target, or
- `AttitudeToward(target’s faction)` is **ally** or **friendly**

Root stacks and some nested stacks (the target itself, armed stacks not under the initiator’s root, siblings under the target’s parent) can appear.

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

Resolution: per-unit declaration (`UnitAttitudes`) else stance toward the unit’s owner (`Attitudes`) else `DefaultAttitude`. Unknown factions use `UnknownAttitude` (baseline **hostile**).

`SET AVOID TRUE` (`IsAvoiding`) is **not** a battle tactic. After a stack fires, `considerRetreat` only prints that an avoiding unit tries to escape and **fails** (or cannot, if immobile). It does not leave the fight. `SET ONLINE TRUE|FALSE` is also not a battle tactic; it only toggles stack and module `Online` (captured modules start deactivated).

## Turns and rounds

- **Turn:** 13 weeks. Battles may start **every week** if enemy armed roots share a location.
- **Battle:** up to **`MaxRounds` = 10** rounds, or until one side’s list is empty.
- Each round: print attacker/defender battle reports, then every operational combatant **fires** (see initiative), then evade-leave is checked.
- End lines: “Battle won by attackers/defenders” or “Battle ended indecisively.”
- Capture damage on modules is **zeroed at the start of each battle** (`resetCaptureDamage`). Hit-point `Damage` is not reset here.

`executeMovement` is empty. In-battle orders (move/use in the round) are comments only.

## Initiative (who shoots when)

All attackers and defenders are grouped by `ModuleStack.Initiative` in a `SortedList` and fire **lowest initiative first**, then higher.

`Initiative` = this stack’s `InitiativeBonus` + nested stacks’ `InitiativeBonus`.

`InitiativeBonus`:

- If **root:** `InitiativeManeuverabilityBonus` = `int(energyReserveRatio + massCapacityRatio) * 10` (energy produced/required and mass capacity/mass; 0 if a denominator is 0).
- Plus sum of **technologies** `initiative` on the stack.
- Plus sum of **people** `Initiative` (each person’s skills).

Catalog examples: military tactics `[miltac]` initiative 5 (held as a tech copy on a command stack); skills frigate pilot / armor platoon leader / infantry battalion commander initiative 5.

Same initiative value: those stacks fire in list order (not shuffled).

## Tactics (live)

`TACTIC` (`TacticOrder`) on a modulestack. Immobile stacks may only set **destroy**.

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

Only **operational** modules fire. Unarmed stacks skip `executeAttack`. Nested fighter drones and shuttles in a bay are skipped in `GetFiringModules` (the hull does not shoot them while docked).

**Armed:** formed, and module group **military**, or group **vehicle** / **infantry** with `attack > 0`, including nested stacks.

**Hangar launch:** at battle start, fighter drones (`alndrn`) and shuttles (`shuttl`) nested in a fighter drone bay (`drnbay`) detach to the location (`STACK OUT`). They join the parent’s side. They **do not fire in round 1**; from round 2 they fight as roots. They cannot move on their own.

Fighter drones: high module `initiative`, small `damage` and hit points, no cargo capacity, helium-3 fuel (1 per 13 weeks).

## Chance to hit

Each **operational firing module** on the shooter (and nested armed stacks) rolls once.

```
chance = (shooter.Attack + nested.Attack) / 2     // integer
if target.HasEvade:     chance = chance / 2
if target.IsImmobile:   chance = chance + chance / 2
dice   = shooter.Attack + nested.Attack + target.Defense + nested.Defense
roll   = uniform 1 … dice   (Sequence; if dice is 0, roll is forced to 1)
hit    if roll <= chance
```

The report prints `(chance: C/D)`.

`Attack` on a formed stack: `QuantityActive * moduleType.Attack` + technologies’ `attack` + people (skills’) `attack`. Nested stacks add through `ModuleStacks.Attack()`. Same shape for **Defense**.

Item `attack` / `damage` (e.g. rocket launchers `[rctlnc]`) are **not** added in `ModuleStack.Attack`. They do not change this formula.

If `Defense` is 0, chance is half of attack and dice is attack, so about **50%** before evade/immobile.

## Damage

On a hit, a module on the target is chosen by **hit weight**, then:

- **Destroy:** `hpDamage =` firing stack’s `ModuleType.Damage` (the weapon that fired). All of it is hit-point damage.
- **Capture:** `hpDamage = 25%` of that damage (integer); the rest is **capture** damage. `HpDamageFromShot` / `CaptureDamageFromShot`.

Both are capped by remaining pool `HitPoints - Damage - CaptureDamage`.

- `Damage >= HitPoints` → **wrecked**.
- Capture: `Damage + CaptureDamage >= HitPoints` and not wrecked → **capture complete** (module offline, peeled to the **battle initiator’s owner** — `this.attacker.Owner`, not necessarily the firing stack).

Hit weight for a stack: `DamageCapacity * intact module count`. Command or propulsion: **×2** if the shot is capture, **÷2** if the target is evading. Nested stacks of the **same owner** are included; other owners contribute 0 to this roll.

Disabling the last operational module can drop the stack from the battle (command/energy flavor lines). Losing the last military module marks unarmed; losing propulsion marks immobile.

## Equipment and officers

**Modules (weapons):** `attack` feeds to-hit; `damage` is the shot; `defense` feeds the defender’s dice; `hit-points` / `DamageCapacity` size the hit-location roll. Quantity active scales attack/defense. Nested military/vehicle/infantry fire as separate weapons on the parent’s shot sequence.

**Technologies** on the stack: `attack`, `defense`, `initiative` summed (e.g. `[miltac]` initiative 5). Battle techs are **held copies**, not `USE`d.

**Officers / people:** each person’s **skills** add `attack`, `defense`, `initiative` to the stack they are on (`People.Attack` / `Defense` / `Initiative`). Catalog: armor platoon leader and infantry battalion commander +5 attack and +5 initiative; frigate pilot +5 defense and +5 initiative; space station command +5 defense. Skill `produce effect="effective attack"` is catalog text; `Battle` does not read it.

**Items:** rocket launchers report attack 2 / damage 2 for infantry; they are **not** in `getChance` or shot damage. Use the **module** (infantry battalion `damage="1"`) until the engine wires equipment.

## Evade leave

After the round’s shots: an evading stack that was **not** hit this round increments a counter; at **2** consecutive unhit rounds it “evades and leaves combat” and is removed from both lists. A hit resets the counter.
