# Player order syntax

Checked **9 Sep 2026** against engine **0.1.158** (`Game/Program.cs`).

Sources: `Game/orders/EOrderType.cs`, `Game/orders/OrderFactory.cs`, `Game/orders/OrdersReader.cs`, `Game/orders/Orders.cs`, `Game/orders/JumpOrder.cs`, `Game/orders/MoveOrder.cs`, `Game/orders/LongOrder.cs` (`CanOperate`, atmosphere and effective location), `Game/game/SpaceTransit.cs` (`f(ΔAU)`, mass factor, baked space-exit weeks), `Game/Game.cs` (week loop, `GenerateOffers`), `Game/Program.cs` (`/data`, `/turn-dir`, `/reports`, `/no-turn`, `/check`), `Game/Research.cs` (weekly output, breakthrough, preference, space-object proximity and reveal), `Game/SurveyReports.cs`, `Game/data structures/SurveyObjects.cs`, `Game/data structures/ModuleStack.Upkeep.cs` (sick bay, medical consume, quarterly maintenance, high-gravity bill), `Game/data structures/Galaxy.cs` (`LoadXml` / `LoadExits` / save of environment attrs, belt and alderson exits), `Game/data structures/Alderson.cs` (`PairName`, orbit only), `Game/data structures/Belt.cs` (`LocationType` space), `Game/data structures/Planet.cs` / `Moon.cs` (`HasEnvironmentAttrs`), `Game/data structures/ELocationType.cs` (`atmosphere`), `Game/data structures/BodyEnvironment.cs` (`EffectiveLocationType`, `HasAtmosphereResources`, `LaunchSurcharge`, `SurfaceOrbitSurcharge`, `BansNonShuttleSurfaceHop`, settlement temperature, gravity), `Game/data structures/Orbit.cs` (`HasAtmosphere`, orbit resources), `Game/data structures/ModuleType.cs` (`IsShipHullType` / `IsShuttleUnit`), `Game/data structures/Exits.cs` / `ExitMode.cs` / `Region.cs` (region **Exits:** lines), `Game/data structures/Faction.cs` (blank line before `Bank report:`), `Game/reports/ReportWriter.cs` (faction report sections and blank lines), `Game/battle/Battles.cs` (blank line between consecutive battles), `Game/game/DataFile.cs` (`LoadLocationType`, `LoadOrders` / `SaveOrders` delegate to `OrderXml`), `Game/game/OrderXml.cs` (XML switch, including `jump`), `Game/game/ModuleTypeGroupXml.cs` (`RESEARCH GROUP` tokens), `Game/game/CatalogLoader.cs` (`planet-atmosphere`, `location-type`), `Game/effects/Effects.cs` (`LoadXml` effect types), `Game/effects/Producing.cs` (omit empty `technology=`), each `Game/orders/*Order.Parse` / `Execute`. Sample prefix usage: `Tests/SampleGame/orders.*.txt`.

Not source of truth: `Game/documentation/Rules.txt`. Turn order files are **Windows-1251** (same as reports). Verbs are case-insensitive; most arguments are not.

**27** verbs register in `OrderFactory.ByVerb` (text via `OrdersReader`, saved games via `OrderXml`): **20 immediate**, **7 long**. See [Turn sequence](#turn-sequence), [Immediate vs long](#immediate-vs-long), and [Text vs XML](#text-vs-xml).

## Prefixes and subjects

Order of a line after comments are stripped: **leading `-`/`+` conditions**, then **duration** (`N` on its own token, or `@` glued to the verb), then the **verb**. `OrdersReader` counts leading `+`/`-` on the first token as condition depth, then tries to read that token as a repeat count, then strips `@`/`+`/`-` from the verb.

### File headers


| Line                         | Meaning                                                                                                                             |
| ---------------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| `#faction <id> "<password>"` | Selects the faction and sets it as subject. Password must match (`GetQuotedToken`). Must appear before stacks/people.               |
| `#modulestack <id            | newN>`                                                                                                                              |
| `#person <id                 | newN>`                                                                                                                              |
| `#end`                       | Recognized header, not an order. `finished` is reset every line, so later lines are still read — put `#end` last, as in SampleGame. |


Orders after a failed `#modulestack` / `#person` (wrong owner) are ignored until a valid subject is set.

### Comments and blanks

Empty lines are dropped. `;` and `//` start a comment; **whichever appears first** wins (`LineParser.CommentIndex`). The rest of the line is discarded.

```
#modulestack 000012
; Caste Prime Headquarters [000012]
@produce cash   // also a comment
```

### Repeat

- Default repeat is **1**.
- `N verb …` — first token is an integer: repeat **N** times (`5 use shtlas as new1`).
- `@verb …` — unlimited (`Repeat = -1`), e.g. `@produce cash`, `@use farmng`.
- A leading minus on a numeric first token is both a **condition dash** and a repeat: `-3 use armcbt as new4 for 000025` means “if the parent failed, USE three times”.

On **immediate** orders, `N` / `@` is how many **weeks** the order may succeed (once per week while it stays on the list). On **long** orders, `N` / `@` is how many **durations** to complete (`5 use` is five builds; `@produce cash` never finishes).

### Conditions (`-` / `+`)

Leading `-` and `+` on the first token nest against **earlier orders on the same subject**. Depth is the count of those characters. Condition characters must sit on the **same token** as the verb (`-stack new1`, not `-` on its own line).

`ExecuteList` only starts an order whose `ConditionalOrders` list is **empty**. That makes the two signs opposite in practice:


| Prefix   | Stored on                                            | Effect                                                                                                                                                                                                                      |
| -------- | ---------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `-child` | parent goes into the **child’s** `ConditionalOrders` | Child is skipped until the parent **finishes** (`Executed`). Then `RemoveConditions` clears the wait. Use this to hop then build: `move O00001` then `-use filidx as new108 for 101`.                                       |
| `+child` | child goes into the **parent’s** `ConditionalOrders` | **Parent is skipped** (its list is no longer empty). The child runs on its own if it has no further `+` grandchildren. `move R00001` then `+use ssassm` does **not** move — the USE chain runs and the MOVE stays leftover. |


Immediate and long orders share one list. A `-` under a long parent waits until that long **finishes** (`Executed`), not merely starts (`Executing`). Stack them: `--+-use`, `-+get`, `--+--give`.

## Turn sequence

From `Game.exe` (`Program.Main`) and `Game.Execute`. A turn is **13 weeks**. Combat details: `player/battle.md`.

### Host pipeline

`Program.Main` is public. Engine version **0.1.158** (`Program.EngineVersion`). Flags share one parse loop: `/reports` and `/no-turn` are bare switches (no following argument); `/data`, `/turn-dir`, and `/check` take the next token.


| Flag        | Argument  | Effect                                                                                     |
| ----------- | --------- | ------------------------------------------------------------------------------------------ |
| `/data`     | directory | Catalog and saved-game directory (default: cwd).                                           |
| `/turn-dir` | directory | Orders, reports, and announcements directory (default: cwd).                               |
| `/reports`  | none      | After load, write faction reports only (below).                                            |
| `/no-turn`  | none      | After load, between-turn orders only (below).                                              |
| `/check`    | filename  | Stub. Stores the argument, then `OrdersReader.Check` returns 0. Does not parse or execute. |


Always: load catalog (`data.xml`) and the saved game. Then one branch (`/check` wins if a filename was stored):

`**/check`:** stub only. No orders, no Execute, no reports, no save.

`**/reports`:** `ReportWriter.GenerateReports(turn_dir)` only. No order load, no events, no `Game.Execute`, no `SaveGame`. Filenames use the **saved** `turn` (seed `turn="1"` writes `report.1.{faction}.txt` into `/turn-dir`). Faction XML (`report.{turn}.{faction}.xml`) is written when that faction’s `xml-report` option is true (default). Does not increment the turn and does not write `gameout`.

`**/no-turn`:** load orders, run **between-turn** immediates only (`AllowedBetweenTurns` — live text verbs: `CONTRACT`, `PRESS`), write announcements (`announce.{turn}.{faction}.txt` for new contracts at that location and for press releases), save.

**Full run** (none of the above):

1. `Request.Load` and `EventsReaders.Load` are **stubs** (return 0 / null). `Game.Events.Execute` is also a stub (returns 0). No GM events run.
2. Load `order.*` files (`OrdersReader`).
3. `Game.Execute` (below) — `turn++` first, so seed `turn="1"` becomes turn 2.
4. Write faction reports (`ReportWriter.GenerateReports`), then save the game (`gameout.{turn}.xml` into `/data`). After that increment the files are `report.2.{faction}.txt` (and `.xml` when `xml-report` is true) plus `gameout.2.xml`.

Text reports insert **blank lines** between major sections: after the engine-version line, after the stub events block, between declared stances and `Bank report:` when declarations exist (`Faction.Report`), before/after `Technology reports:` when that section is present, before/after `**Survey reports:`** when that section is present (between technology reports and battles), before `Battles report:`, between consecutive battles (`Battles.Report`), after each space system, and before each visible region. The galaxy block ends with a blank line.

### Each turn (`Game.Execute`)

Clear last turn’s event reports, then `turn++`. Drop stale per-unit stances (`DropStaleUnitAttitudes`: missing, empty, or now-own stacks).

**Weeks 1–13**, in order:

1. Clear each subject’s “already did a long order” flag and each immediate’s `Executed` flag.
2. **Orders** (`ExecuteOrders`):
  - Every **faction** (e.g. `CONTRACT`, `PRESS`).
  - **Module stacks that still have orders** (`HasOrders`), looping until a pass does nothing. Each stack: immediate loop → one long → immediate loop, drop finished non-repeating orders, then tick **effects** (`Moving`, `Producing*`, `Training*`).
  - **People that still have orders**, same loop (person effects are commented out and do not tick here).
3. **Sick-bay heal** — stacks whose module type heals `wndtrn` (catalog: sick bay `[sckbay]`). With medicines `[medici]` on the stack, convert up to **4 wounded per bay** into terran `[terran]` this week and consume 1 medici each. Without medicines, count unmedicated weeks and every **4 weeks** convert **2 wounded per bay**. Medical facility `[medfac]` heal is catalog-only (`target="stacked"`) and does not run here.
4. **Medical consume** — `medici` for remaining wounded/mad crew on each stack (`wndtrn` / `madtrn`). Food and breathing gas are not deducted here.
5. **Contracts** — evaluate triggers and pay rewards.
6. **Buy offers** — each standing `BUY` tries to match a sell (`Offer.Process`). `SELL` only lists; matching is from the buy side.
7. **Battles** — `Battle.StartAtLocations` then `Execute` (see `player/battle.md`). Then drop stale per-unit stances again.

After week 13: **quarterly maintenance**, then **quarterly wounded outcome**, then clear long/immediate flags; drop unformed stacks that should not report (`RemoveNonReporting`).

**Quarterly maintenance** (`ExecuteMaintenance`, once, week 13): cash upkeep, then food, then terran air `[terair]` if the stack needs canned air (orbit, space, or a moon region). Bills auto-GET from this stack’s own nest (self, then nested children), then the parent chain, then other same-owner stacks at the same location. Cash shortfall can also debit the faction bank. When cash (or any other upkeep item) is actually deducted (`taken > 0`), the stack logs `week 13: paid N cash [cash] upkeep.` (`ItemType.ReportName` is `cash [cash]`; same `paid N {item} upkeep.` shape for food/air). Each formed stack pays its own `UpkeepNetto` (`localUpkeep` does not roll nested children into the parent bill). Nested hangar craft still pay if they remain nested through week 13; after hangar launch they pay as roots (4 alien fighter drones `[alndrn]` at 20 cash each = `paid 80 cash [cash] upkeep.`). Unpaid food/air can wound healthy terrans (catalog 25%). Unpaid cash can damage the module or print race off-duty. `medici` is skipped here (already weekly).

**High gravity** (`BodyEnvironment.GravityAt` == `high`): that stack’s local cash bill is multiplied by **1.5** (ceiling). Stacks with people or `population-maximum` > 0 also add **2 food**. Planets with no `gravity` attribute load as `normal`; moons with no attribute load as `low`. SampleGame maps have no `gravity=` attrs, so this surcharge does not fire there.

**Quarterly wounded outcome** (once, week 13): each remaining `wndtrn` rolls independently — **25%** die of wounds (`roll < 25`), **25%** recover to terran (`roll ≥ 75`), **50%** stay wounded. An officer anywhere on the stack (including nested modules) with trained `**hmedic`** skill lowers the recover threshold by that skill’s catalog `cure-chance` (e.g. `25` → recover at `roll ≥ 50`). Death threshold is unchanged. Not gated on medici. `madtrn` is not rolled here.

**End of turn (once):**

- Bank: quarterly interest (`AddQuarterlyInterest`). Balance is stored and reported as **whole credits** (rounded half away from zero).
- `UpdateRates` — each quarter after bank interest: (1) for each region that already posted an item price, set that price to the **galaxy-wide average** of all regions that posted that item (integer, min 1 when average ≥ 0.5); standing offer **objects** keep their saved price — only regional price lists drift; (2) player factions 2–11: if balance > 5000, deposit rate −0.005 (floor 0.01); if balance < 0, credit rate +0.005 (ceiling 0.25).
- `GenerateOffers` — NPC faction `[1]` stacks whose module type is `city` auto-list on-hand inventory as `SellItems` `Offer`s (same objects as XML `<selling>`; not leftover player `SELL` orders). Skips cash. Skips an item type if that city already has a **buy or sell** offer for it (no simultaneous buy+sell of the same type; standing offers are not rewritten, so existing NPC city sells **remain** at their saved quantity and price). Quantity for a **new** listing is on-hand; price is `Market.GetPrice` (regional average if any region posted a price, else catalog nominal `value`, else 0); skip if price ≤ 0. Farms and other non-city stacks are not auto-listed. Listings appear on this turn’s reports and save; weekly buy matching (step 6) can hit them from **next** turn. Duration-0 leftover `receiving-items` on cities (old market delivery) **persist** after save but **never complete**; there is no player verb that clears them.

Standing `@buy` / `@sell` stay on the order list and retry each week at step 6. NPC city auto-listings have no leftover `SELL` and persist as market `Offer`s. `ATTACK` / `TACTIC` / `DECLARE` during step 2 only set stance; shooting is step 7.

## Immediate vs long

A turn is **13 weeks**. Each week, for each faction / stack / person, the engine:

1. Runs **immediate** orders in a loop until a pass does nothing.
2. Runs **at most one long** order (the first ready one on that subject).
3. Runs **immediate** again, so `GET` / `STACK` / `SEE` can fire the same week after a move or a finished build.

The two kinds are independent except where you chain them with `-` / `+`. An immediate line after an unfinished long still runs if it has no pending conditions.


|                       | Immediate                                                                      | Long                                                                                                                                                                                                                                                    |
| --------------------- | ------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| How many per week     | As many as can succeed (loop until idle)                                       | **One** per subject                                                                                                                                                                                                                                     |
| Duration              | Finishes in the week it succeeds (or retries later)                            | Occupies the long slot for `Duration` weeks (`use-time`, produce duration, move legs, …)                                                                                                                                                                |
| Needs an active stack | No (probes and cargo still have their own checks)                              | Yes: formed, enough crew and energy (`CanOperate`)                                                                                                                                                                                                      |
| Repeat `N` / `@`      | Succeed up to N weeks, or every week forever                                   | Complete N full durations, or never stop                                                                                                                                                                                                                |
| After a turn          | Dropped when `Executed` and repeat is used up; otherwise stays on the template | Same; in-progress work is kept as an effect (`Moving`, `Producing*`, `Training*`, `Receiving*`, `Fuelled`; `lightly-damaged` is a module marker). Those types **load again** after save (`Effects.LoadXml`). `JUMP` has no effect type (same-week hop). |


**Immediate** orders are setup, probes, cargo, module transfers, stance, and stacking. They do not consume the week’s long slot. `FORM`, `GET`, `GIVE`, and `TRANSFER` can all fire the same week as a `USE` or `MOVE`.

**Long** orders are the week’s work: jump, move, produce, repair, research, train, use. A second long on the same subject waits until the first completes (or until its conditions clear). Accepting modules mid-week can mark the receiver as having already used its long slot. `JUMP` occupies the long slot but does **not** call `CanOperate` (see [JUMP](#jump)).

`CONTRACT` and `PRESS` are immediate and also **allowed between turns** (`/no-turn`). No other live text verb is.

## Order playbooks (templates)

Hard-science campaign drafts should follow these patterns. Ground every id in the **text report** and orders template at the bottom — do not invent stack ids.

### Activating disabled module stacks

A stack marked **disabled** in the report is not yet operational (`Online=false`, or `Online=true` but missing crew, energy, fuel, or repairs). `SET ONLINE TRUE` is the only activate verb. Long orders (`USE`, `PRODUCE`, `REPAIR`, …) then require `CanOperate`:

- sufficient **crew** (`terran` items on the stack),
- sufficient **energy** from the root production tree,
- sufficient **fuel** when the module consumes fuel,
- **repaired** damage when modules are damaged,
- other catalog **operate-in** conditions as applicable.

To activate a module stack it must receive required inputs — **`GET`** them from other stacks at the same location, or **`BUY`** at a UN market after **`WITHDRAW`** enough cash into a trading stack.

Typical bootstrap at headquarters (SampleGame and campaign turn 1):

1. **`SET ONLINE TRUE`** on each production stack you intend to run this quarter.
2. **Staff crew** — if the report shows `crew: N/0`, either **`GET` `terran`** from headquarters/cargo, or **buy crew at market** (see below).
3. **`GET` fuel and inputs** — e.g. `@get all carbon from <cdrill-id>` into the cargo bay, then coal plant `@produce energy`.
4. **`@produce energy`** on coal or wind plants so nested stacks meet energy requirements.
5. **`@use farmng` / `@use hcdril`** once the stack is operational.
6. **`@get`** surplus into the cargo bay; **`SELL … AT AVERAGE`** for exports; local **`@buy`** for metals (see [BUY](#buy) — **`AT AVERAGE` is SELL-only**).

#### Buy crew when short (campaign turn 1 example)

When headquarters lacks enough `terran` to staff every nested module, withdraw bank cash into the cargo bay and buy colonists at a UN city (Assembly on Arbor, Slagport on Anvil):

```
#modulestack 200003
; cargo bay — trading stack at HQ
withdraw 1500
@buy 25 terran at 50

#modulestack 200006
set online true
get 15 terran from 200003
@use farmng

#modulestack 200004
set online true
get 6 terran from 200003
@use hcdril
```

Adjust quantities to match catalog crew per module (`cdrill` 6, `farms` 5×qty, `factry` 10×qty, `cplant` 2×qty). **`GIVE`/`GET`** distributes bought `terran` to nested stacks. Campaign seed games now pre-place crew on each nested stack so turn 1 can skip the market buy when the report already shows crew aboard.

Order stacks on the **same subject**: HQ `@produce cash`, cargo bay `@get` / `@sell` / `@buy`, each module stack its own `#modulestack` block.

### Multi-stop MOVE (preferred)

**Syntax:** `MOVE <dest> [<dest2> …]` on one line — e.g. `move R00014 R00009` walks Grant → Farm Belt → Mid Vale using report exit durations.

**Subject:** a **mobile `#modulestack`** only (shuttle, infantry stack, ship hull, etc.). Immobile stacks (`corphq`, `cargob`, …) cannot move.

**People travel with vehicles, not alone.** A `#person` cannot take `MOVE` — `MoveOrder` casts the subject to `ModuleStack` and throws `InvalidCastException`. Board the CEO on a vehicle first:

```
#person 200010
active 100
-stack 100

#modulestack 100
; space shuttle [shuttl] or other ground-capable stack
move R00014 R00009
```

Use `ACTIVE` + `STACK` (or start the turn already nested under a mover). Conditional `-move` chains are optional; for reconnaissance prefer a **single** `MOVE` with multiple region ids on the vehicle stack.

### Turn-1 economic bootstrap (pattern)

```
#modulestack <hq-id>
@produce cash

#modulestack <cargob-id>
@get all food from <farms-id>
@get all carbon from <cdrill-id>
sell <N> food at average

#modulestack <cdrill-id>
set online true
@use hcdril

#modulestack <farms-id>
set online true
@use farmng

#modulestack <cplant-id>
set online true
@produce energy
```

Replace ids from the report template. When nested stacks already show crew in turn-1 reports, skip the market-buy block above. Defer ground **`MOVE`** until a shuttle or other mobile stack exists; people ride on that stack.

## Text vs XML

`DataFile.LoadOrders` calls `OrderXml.LoadAll`; `DataFile.SaveOrders` calls `OrderXml.SaveAll`. XML builds leftover orders when loading a saved game. Divergences:


| Verb                                    | Text (`OrdersReader` → `OrderFactory`)  | XML (`OrderXml` → `OrderFactory`) |
| --------------------------------------- | --------------------------------------- | --------------------------------- |
| `COPY` comments mention `COPY all TO …` | **not parsed** — technology id required | technology + receiver attributes  |


Player turn files use **text**. XML matters for saved games, not for `order.*` drafts. Text `-` / `+` syntax is unchanged.

**Nested conditions:** `Order.SaveXml` writes leftover `-` / `+` children as nested `<order conditions="…">` under the parent (top-level save is `Level == 0` only). `saveXml_post` writes only the **next remaining condition level**, once (no duplicate nested siblings); recursion still persists leftover `+USE` trees. `LoadAll` walks those nested `<order>` elements and assigns the same `-` / `+` links as text (`AssignCondition`). Duplicate subject + conditions + verb XML is skipped. Frozen SampleGame `gamein.2_contract.xml` / `gamein.3_contract.xml` keep those leftovers; player-facing turn 2/3 reports are unchanged.

**Saved effects:** `Effects.LoadXml` accepts `fuelled`, `moving`, `producing-modules`, `producing-items`, `producing-energy`, `receiving-items`, `receiving-modules`, `receiving-technology`, `lightly-damaged`, and `training-officer`. `Producing.SaveXml` writes `technology=` only when Technology is not null; `LoadXml` skips a missing or empty attribute, so `@produce energy` leftovers round-trip as `producing-energy` with no tech id. `producing-modules` still writes `technology=` (`ProducingModule.SaveXml`), so leftover `USE` can match after load. `receiving-items` now writes a nested `<receiving>` cargo payload. Skill training still saves as `type="training-officer"` with a `skill` attribute (not a player-facing verb — issue `TRAIN SKILL`). Leftover `TRAIN` XML uses a child `<officer>` element (`name` / `race` / optional `officer-parent`), not an attribute; `LoadXml` sets `TrainingOfficer` from that child. `USE` leftover reconnects to a matching `Producing*` effect; leftover `TRAIN` reconnects to a matching `TrainingOfficer` / `TrainingSkill` and continues `DurationLeft` from the effect. Leftover `PRODUCE` does **not** (`Producing` on the order is null after load, so it starts a new duration while the loaded effect stays frozen). Duration-0 `receiving-items` leftovers do not deliver cargo.

---

## Immediate orders

ACTIVE, ALIAS, ATTACK, BUY, CAPTURE, CONTRACT, COPY, DECLARE, DEPOSIT, FORM, GET, GIVE, HAS, NAME, PRESS, SEE, SELL, SET, STACK, TACTIC, TRANSFER, WITHDRAW.

### ACTIVE

**Syntax:** `ACTIVE <modulestack-id|newN>`

**Subject:** any orderable (observer).

Succeeds if the named stack is already active (`IsActive`). Used as a condition parent (SampleGame: `active new2` then `--stack new3`).

### ALIAS

**Syntax:** `ALIAS "<alias>"`

**Subject:** modulestack.

Sets the stack’s `Alias` string.

### ATTACK

**Syntax:** `ATTACK <unit-id>`

**Subject:** a holder (stack).

Sets the owner’s attitude toward that unit id to **enemy**. Combat itself is resolved later from tactics (`destroy` / `capture`).

### BUY

**Syntax:**

- `BUY <quantity\|ALL> <item-id\|module-id> [AT <price>] [EVERYWHERE]`
- `BUY <technology-id> [AT <price>] [EVERYWHERE]`

**Subject:** an offerent (trading stack).

Posts a standing buy on the local market (`Offer.Process`). Matching can complete against leftover player `SELL` and against market `Offer`s (XML `<selling>`, including NPC city auto-listings from end of turn). Quantity `ALL` sets `AllQuantity`. Omitted `AT` leaves price unrestricted (`Price = -1`). **`AT` takes a numeric price only** — unlike [SELL](#sell), **`AT AVERAGE` is not parsed on BUY** (throws `bad syntax price expected`). Optional trailing **`EVERYWHERE`** is parsed but **campaign execution currently throws** (`Regions.DistanceBetween` not implemented) when matching distant markets — omit `EVERYWHERE` on campaign maps until fixed, or use local `@buy` without it. First token is treated as a **technology id** if it is in the catalog (do not write a trailing `technology` word — Parse would reject it). Sample: `@buy all terran`, `@buy all iron` (local market).

### CAPTURE

**Syntax:** `CAPTURE <unit-id>|ALL`

**Subject:** modulestack.

Sets tactic to **capture**. A specific id is the preferred target and is marked enemy if that stack exists. `ALL` prefers every enemy at the location. Successful battle peels go onto new stacks `c00001`, `c00002`, … (see `player/battle.md`).

Immobile stacks cannot capture. Execute reports `CAPTURE failed. Immobile units may only use destroy.` **once per week**, marks the order **executed** (default one-shot is consumed that week; `@capture` retries next week), and does not set the tactic.

### CONTRACT

**Syntax:**

- `CONTRACT <location> GIVE <quantity> <module-id> TO <stack-id> REWARD <technology-id> [TITLE "<title>"] [FLAVOUR|FLAVOR "<text>"]`
- `CONTRACT <location> RESEARCH <stack-id> [POINTS <n>] REWARD <stack-id> UNIT [TITLE "<title>"] [FLAVOUR|FLAVOR "<text>"]`
- `CONTRACT <contract-id> WITHDRAW`

**Subject:** **faction** (`#faction` as subject). Also allowed **between turns**.

Publishes a location contract, or withdraws one by id. GIVE pays the technology when a non-issuer delivers that module quantity to the receiver. RESEARCH pays the **unit** when a same-location lab accumulates `POINTS` (default **5**) on the target wreckage stack. Execute requires a technology reward on GIVE and a unit reward on RESEARCH. Optional TITLE/FLAVOUR store on the contract. If the subject is not a faction, Execute does nothing.

### COPY

**Syntax:** `COPY <technology-id> TO <stack-id>`

**Subject:** modulestack (source).

Copies the named catalog technology onto a receiver at the **same location**, if that receiver has remaining technology capacity. The leftover template prints `COPY <technology-id> TO <stack-id>` (alias while the receiver is unformed). Execute does **not** check that the source already holds it. `COPY all` is not implemented. Condition `COPY` on the `USE` that forms the receiver (`--copy … to newN` under `-use … as newN`) so it does not retry against an unformed stack.

### DECLARE

**Syntax:**

- `DECLARE FACTION <id> <attitude>`
- `DECLARE UNIT <id> <attitude>`
- `DECLARE DEFAULT <attitude>`
- `DECLARE UNKNOWN <attitude>`

**Subject:** any orderable (applies to its owner).

One-way stance. Attitudes (case-insensitive): `enemy`, `hostile`, `neutral`, `friendly`, `ally`. `enemy` is the combat stance (faction-wide when targeting a faction). Sample: `-declare faction 2 enemy`.

### DEPOSIT

**Syntax:** `DEPOSIT <quantity|ALL>`

**Subject:** item holder (stack or person).

Moves cash [cash] from the subject into the owner’s bank account. `ALL` deposits every cash unit the holder carries. Fails if the holder has less cash than requested. Credits the bank and logs `deposited N cash [cash] to bank account.` on the subject.

### FORM

**Syntax:** `FORM NEW [WITH <n>] [AS "<alias>"|newN]`

**Subject:** modulestack.

Creates an empty stack as a sibling of the former. `WITH n` immediately transfers `n` modules into it (internal `TransferOrder`). `AS` names/aliases the new stack via `GetOrCreateNewModuleStack`. Execute always uses the formed stack — **include `AS`** so the new unit exists.

### GET

**Syntax:**

- `GET <quantity\|ALL> <item-id> FROM <stack-or-person>`
- `GET ALL FROM <stack-or-person>` — all item types from one holder
- `GET <quantity\|ALL> <item-id>` — that item from all friendly holders here
- `GET ALL` — all items from all friendly holders here
- Negative quantity: leave that many behind (`get -15 carbon from 000015`)

**Subject:** item holder (stack or person).

Moves cargo from a same-location holder into the subject if capacity allows. `newN` transferers are created if needed.

### GIVE

**Syntax:**

- `GIVE <quantity\|ALL> <item-id> TO <stack-or-person|newN>`
- `GIVE ALL TO <stack-or-person|newN>`
- Negative quantity: leave that many behind (`give -20 terran to new6`)

**Subject:** item holder.

Moves cargo to a receiver (same-location capacity check). Receiver may be an unformed `newN`.

### HAS

**Syntax:**

- `HAS <quantity> <item-id\|module-type-id>`
- `HAS PERSON <person-id|newN>`
- `HAS MODULES [<quantity>]` — omit quantity to mean “any modules” (`quantity = -1`)

**Subject:** holder / stack (module counts require a stack).

Condition probe: succeeds if recursive cargo / nested module count / person presence meets the threshold. Sample: `-+has person 000001`.

### NAME

**Syntax:**

- `NAME "<new name>"` — rename this stack
- `NAME <planet|moon|orbit|region> "<new name>"` — rename that map object; the stack must be **in that location**

**Subject:** modulestack.

### PRESS

**Syntax:**

- `PRESS TITLE "<title>" [FLAVOUR|FLAVOR "<text>"]`
- Bare tokens: first unused token is the title, the next is flavour. Title or flavour is required.

**Subject:** **faction** (`#faction` as subject). Also allowed **between turns**.

Creates a `PressRelease` and reports `issued press release {title}.` on the issuer. If the subject is not a faction, Execute does nothing. `/no-turn` writes title and flavour into `announce.{turn}.{faction}.txt` for every faction (`Contract.All.WriteAnnouncements`).

### SEE

**Syntax:**

- `SEE <stack-id|newN>`
- `SEE PERSON <person-id|newN>`
- `SEE <person-id|newN> PERSON`

**Subject:** holder.

Succeeds if that stack or person is at the observer’s location. Both person word orders parse (`person` must be lowercase). Leftover/template (`SeeOrder.Report`) prints `see <id> person` (or `see newN person` if unformed). XML is unchanged: `see-type="person"` plus `person="<id>"` on the `<see>` element (`LoadXml` uses `GetOrCreateNewPerson`).

### SELL

**Syntax:**

- `SELL <quantity\|ALL> <item-id\|module-id> [AT <price>|AVERAGE]`
- `SELL <technology-id> [AT <price>|AVERAGE]`

**Subject:** offerent.

Lists a standing sell (`Offer`) and keeps a leftover `SELL` on the template. Matching is driven from the buy side; Execute does not complete the trade itself. `AT AVERAGE` uses `Market.GetPrice` (regional average if any region posted a price, else catalog nominal `value`, else 0). Sample: `-sell 1 wnplnt`. Same technology-id rule as BUY (no trailing `technology` word). NPC city auto-listings are `Offer`s only (no leftover `SELL`); see [Turn sequence](#turn-sequence).

### SET

**Syntax:** `SET AVOID|ONLINE TRUE|FALSE`

**Subject:** modulestack.

Flag name and `TRUE`/`FALSE` are **case-insensitive**; Parse stores the flag as uppercase `AVOID` or `ONLINE`.

- `SET AVOID TRUE|FALSE` — sets `IsAvoiding`. Not a battle tactic (see `player/battle.md`).
- `SET ONLINE TRUE|FALSE` — `ModuleStack.SetOnline`: stack `Online` and every `module.Online`. Captured modules are left `Online=false` (report: deactivated); there is no other activate verb. Sample: `set online true`.

### STACK

**Syntax:** `STACK <parent-id|newN>` or `STACK TOP` or `STACK OUT`

**Subject:** stack (or person as item holder).

Nests the subject under another stack (same location, same faction, not self), under the root parent (`TOP`), or out into the location (`OUT`). `**top` and `out` must be lowercase.** Fighter drones may only `STACK OUT` (location) or stack under a fighter drone bay; stacking under a hull fails. Shuttles may also nest under a frigate hull.

### TACTIC

**Syntax:** `TACTIC destroy|capture|evade` or `TACTIC prioritize armed|command|storage`

**Subject:** modulestack.

Persists firing/evade/priority tactics. Destroy and capture are exclusive. The three `prioritize` kinds are exclusive with each other (`ApplyPrioritizeTactic` removes the others) and may coexist with destroy/capture/evade. `prioritize storage` prefers storage-group stacks (`IsCargoStack()`, e.g. `cargob`). Sample: `-tactic prioritize armed`, `tactic prioritize storage`, `-tactic capture`.

Immobile stacks may only `destroy`. `TACTIC capture` and `TACTIC evade` report `TACTIC failed. Immobile units may only use destroy.` **once per week**, mark the order **executed** (default one-shot consumed; `@tactic` retries next week), and do not change tactics. `prioritize armed|command|storage` still applies on immobile stacks.

### TRANSFER

**Syntax:** `TRANSFER <n> TO <id>`

**Subject:** modulestack (source).

Moves `n` modules of this stack’s type onto an **existing** receiver. `n` must be a positive integer. The receiver id must already exist (no `newN` create). Module type is the transferer’s, same as XML load. Instantaneous: same type **merges** into the receiver; a different type **nests** under it. Fighter drones (`alndrn`) may only nest in a **location** (`STACK OUT`) or a **fighter drone bay** (`drnbay`); `TRANSFER` them onto a hull fails — target the bay. Shuttles (`shuttl`) may also nest under a frigate hull. Copies the source’s long-order-used flag onto the package so the receiver cannot take a second long this week. Emptying the last module removes the source stack. Notifies GIVE contracts. Fails with `TRANSFER failed. tried to transfer more modules than having.` Execute does not check same location. `ALL`, damaged-only, and `MODULE <index>` are not parsed.

### WITHDRAW

**Syntax:** `WITHDRAW <quantity|ALL>`

**Subject:** item holder (stack or person).

Moves cash [cash] from the owner’s bank account into the subject as cargo. Only **positive** bank balance counts (credit line is not withdrawable). `ALL` withdraws the full positive balance. Fails if the account has less than requested or the subject lacks cargo capacity. Debits the bank and logs `withdrew N cash [cash] from bank account.` on the subject. Not the same as `CONTRACT … WITHDRAW`.

---

## Long orders

JUMP, MOVE, PRODUCE, REPAIR, RESEARCH, TRAIN, USE.

### JUMP

**Syntax:** `JUMP <alderson-id>`

**Subject:** modulestack.

Alderson-gate hop. Parse takes **one** token; it must be an **Alderson Gate** id in `Alderson.All` (not a planet, region, orbit, moon, belt, or star). Throws `Bad syntax or unknown JUMP destination` otherwise. Text and XML both parse (`OrderXml` case `jump`). Leftover XML is `<jump destination="…"/>` (the gate id) plus optional `duration-left`. There is no `Report` override: a leftover that is still `NotExecuted` prints `jump` with no destination.

**Who:** `ModuleType.IsShipHullType` — groups `frigate`, `corvette`, `destroyer`, `cruiser`, `capital`, `ark`, `shuttle`, `spacecraft`, or a shuttle unit (`shuttl`, `alndrn`). Else `JUMP failed. Only ships can jump.` Unformed (null module type) fails with no event line.

**Where:** the stack must be in an **orbit whose holder is an Alderson Gate** (`orbit.OrbitHolder as Alderson`). Planet/moon orbits and all regions fail (`GateAt` returns null). That gate must have XML `pair` set (`Alderson.PairName`). Else `JUMP failed. Unit is not at an Alderson Gate.` The destination gate’s **name** must equal that `pair` value. Else `JUMP failed. Destination is not the paired Gate.` Pairing is one-way (only the origin’s `pair` is checked). Gates have **orbit only, no regions** — frigates occupy the gate orbit; they do not land. SampleGame maps have no `<alderson>` objects, so JUMP parse fails there. Campaign maps pair gates (e.g. Helios Gate `[P00009]` ↔ Fomal Gate `[P00010]`). Planet `pair` is unused by JUMP.

**Execute:** occupies the long slot (`base.Execute`). Does **not** call `CanOperate` (crew, energy, `operate-in`, and disabled stacks do not block). Does not consume fuel and does not start a `Moving` effect. Duration is 1 and completes in the **same** week: logs `jumping to {gate ReportName}, ETA 1.` then sets `Parent` to the arrival and logs `arrived at {arrival ReportName} via JUMP.` Arrival is the destination gate’s `Orbit`. Nested children stay nested on the jumper; a nested shuttle issued JUMP unnests to the arrival. Failure and success both set `Executed` (long `Executed` is not cleared each week, so `@jump` does not retry). Repeat `N` is not decremented on arrival.

### MOVE

**Syntax:** `MOVE <dest> [<dest2> …]`

**Subject:** modulestack only (not `#person`). People move only when boarded on a vehicle (`ACTIVE` + `STACK` onto a shuttle, infantry stack, or hull) — see [Order playbooks](#order-playbooks-templates).

Walks a route in one order — e.g. `move R00014 R00009` (Grant → Farm Belt → Mid Vale). Each dest token is a **region**, **star**, **planet**, **moon**, **belt**, **alderson**, **anomaly**, or **orbit** id. Conditional `-move` / `+move` chains are optional; prefer listing all ground hops on one `MOVE` line for reconnaissance. Immobile stacks (e.g. `corphq`) cannot move. Stars, planets, moons, anomalies, and Alderson Gates resolve to their **orbit**. A **belt** token is the belt itself (location-type **space**, not a landing). Starts a `Moving` effect, consumes fuel when required, changes parent on arrival.

**Exits on the report:** a **region** block includes `Exits:` (`Region.Report` → `Exits.Report`). A region destination prints `{name} [id] (x,y), {region type}, {ground|naval|space} travel duration N week(s).` A non-region destination prints that location’s `ReportName` plus the mode duration — `orbit [id], space travel duration N week(s).` for an orbit, or `{name} [id] at AU N, belt, space travel duration N week(s).` for a belt. Orbit reports and `Belt.Report` do not list exits (belt exits still exist in the save and are used by MOVE). Maps without `orbit=` / `belt=` / `alderson=` exits (SampleGame) never show those lines.

**Orbit atmosphere line** (`Orbit.HasAtmosphere`): each orbit header ends with `, has atmosphere` or `, has no atmosphere`. True when the orbit has `<resource>` or `<race>` entries, or the parent planet/moon has races, or parent `atmosphere` ≠ `none` (any non-none band — thin, terair, hostile — counts). Optional `suitable for {race}` lists orbit and inherited body races.

**Duration** (`movementDuration`) is not always the printed exit duration. Space hops use `SpaceTransit` (`Game/game/SpaceTransit.cs`). `JUMP` is a separate 1-week hop and does **not** use AU.

- Same-planet **region → region**: ground or naval (`tryCompatibleSurfaceMode`); weeks = ceil(exit duration / mover Speed). Needs a compatible surface exit from here. If that surface match fails, a listed space exit uses baked space-exit weeks (below); otherwise AU weeks.
- Same-parent **region ↔ orbit** (e.g. Arbor surface → Arbor orbit): space; **1 week**, even if the exit lists more. Needs a space-capable mover (or nested space stack). Then the surface↔orbit environment rules below run before departure.
- **Belt** hops (current or dest is a belt): space. A listed space exit uses baked space-exit weeks; otherwise AU weeks. Occupying a belt is **space**, not a landing — there is no solid surface.
- Region ↔ orbit with a **listed space exit** (different parent — typically a surface region to a Gate orbit): baked space-exit weeks. No listed exit: AU weeks. Alderson Gates have **orbit only, no regions**. Frigates occupy the Gate orbit; they do not land.
- **Orbit ↔ orbit** (planet, moon, Gate, or other): space; AU weeks. ΔAU is `|BodyAu(here) − BodyAu(dest)|` (a moon is `planet.AU + moon.AU`).
- Any other hop: listed space exit → baked space-exit weeks; else AU weeks.

A stack with a **space** move mode may attempt a hop even when the current location lists no matching exit (`isWay` returns true). Ground- or naval-only stacks need an exit from here to the dest. Unformed stacks fail with `MOVE failed. unformed units cannot move.`

#### Space weeks (`SpaceTransit`)

Catalog space `speed` omitted defaults to **1**. Effective speed is that catalog speed (max of this stack and nested space movers) times a mass factor. Thrust is the sum of nested space `mass-capacity` (this stack plus children). Zero thrust leaves the mass factor at 1.

```
load = (sum nested space mass-capacity) / max(Mass, 1)
massFactor = clamp(load / (40000 / 4150), 0.67, 1.50)
effectiveSpeed = catalogSpaceSpeed × massFactor
```

**AU hops** (`DurationWeeks` → `DurationWeeksFromRaw` → `RoundUpWeeks`): ΔAU ≈ 0 is 1 week. Speed ≤ 0 treated as 1.

```
if ΔAU < 0.1:
    f = 50 × ΔAU                    // moon-scale; no premature floor
else:
    f = 6 + 33 × ln((1 + ΔAU) / 2.7) / ln(80 / 2.7)

if f ≈ 1.0: weeks = 1               // bypass mass/speed (same-body orbit hops)
else: weeks = RoundUpWeeks(f / effectiveSpeed)
```

`RoundUpWeeks` = `max(1, ceil(weeks − ε))` — one ceil after dividing by `effectiveSpeed`. When `f` is exactly 1 week at speed 1, duration stays **1** even if mass factor < 1 (cargo hull).

The log is the two-point fit through (1.7, 6) and (79, 39). Default workshop frigate (one `[fustor]`, mass **4150**, mass factor **1.00**, speed **1**):


| Hop                    | ΔAU  | Weeks  |
| ---------------------- | ---- | ------ |
| Planet → moon          | 0.04 | **2**  |
| Planet → belt          | 1.7  | **6**  |
| Planet → gas giant     | 4.2  | **13** |
| Planet → Alderson Gate | 79   | **39** |


Scout (factor 1.50) / cargo (factor 0.67) on the same hops: moon 2 / 3, belt 4 / 9, gas giant 9 / 19, Gate 26 / 59. SampleGame `rctdrv` / `autdrv` list thrust **10000** (omitted speed → 1); a default-mass hull then hits the **0.67** clamp.

**Baked space exits** (`ExitDurationWeeks`): `RoundUpWeeks(exitDuration / effectiveSpeed)`. Same-body surface↔orbit stays **1 week** and does not use this formula. `JUMP` stays **1 week**, not AU.

#### Surface↔orbit environment

Same-body region↔orbit only (`BodyEnvironment.IsSurfaceOrbitHop`). Runs once when the hop starts (`MovingTo` is still empty). Failures set `Executed` (the leftover is consumed; `@move` does not retry).

**Ship landing ban** (`BansNonShuttleSurfaceHop`): a non-shuttle **ship hull** (`IsShipHullType` and not `IsShuttleUnit` — frigates, corvettes, …) cannot hop region↔orbit when the surface body **emitted** environment attrs and `atmosphere` is not `none`. Fail: `MOVE failed. Spaceships cannot use solid-surface exits on atmospheres.` Shuttles (`shuttl`, `alndrn`, group `shuttle`) still ferry. Vacuum moons (`atmosphere="none"`, e.g. Selene) stay landable for frigates. Bodies that **omit** `gravity` / `temperature` / `atmosphere` (SampleGame Earth) stay landable. Save writes those three attrs only when they were emitted (`HasEnvironmentAttrs`).

**Launch surcharge** (`LaunchSurcharge` / `SurfaceOrbitSurcharge`): same hop, **both ways**, consumes oxyhydro `[h2o2]` from the mover or nested cargo when the body emitted environment attrs. Quantity is a table of gravity × atmosphere (hostile adds 4):


| Gravity | Atmosphere `none` | Other atmosphere (`thin` / `terair` / `hostile`) |
| ------- | ----------------- | ------------------------------------------------ |
| high    | 16                | 16 (+4 if hostile → 20)                          |
| normal  | 4                 | 8 (+4 if hostile → 12)                           |
| low     | 0                 | 2 (+4 if hostile → 6)                            |


Arbor / Anvil (`normal` + `terair`) = **8** `h2o2` both ways. Selene (`low` + `none`) = **0**. Omitted attrs (SampleGame Earth) = **0**. Short cargo: `MOVE failed. Not enough unit of oxyhydro [h2o2] for launch.` Success logs `consumed {N units of oxyhydro [h2o2]} for launch.` (or `for launch for {mover}.` if a nested holder paid). This is separate from drive fuel (`needFuel`).

### PRODUCE

**Syntax:** `PRODUCE ENERGY` or `PRODUCE <item-id>`

**Subject:** modulestack.

Starts energy or item production using the stack’s module type (`ProducingEnergy` / `ProducingItems`), duration from `ProduceDuration`. Energy production has no catalog technology (null Technology); save omits empty `technology=` so the leftover `producing-energy` effect loads. Those effects survive save/load; a leftover `@produce` does **not** reconnect to them (unlike leftover `USE`). Sample: `@produce cash`, `@produce energy`, `@produce terran`.

**Location and atmosphere** (`LongOrder.CanOperate`): module `operation-allowed-in location-type` is checked against `BodyEnvironment.EffectiveLocationType` (not raw `Location.LocationType`). Catalog values: `solid-surface`, `liquid-surface`, `orbit`, `atmosphere`, `space`. A **gas-giant orbit** whose parent planet emitted `atmosphere` ≠ `none` counts as `**atmosphere`** (e.g. ram scoop `[ramsco]` with `location-type="atmosphere"` — fixture catalog `Tests/fixtures/gas-atmosphere/data-ramsco.xml`, not main SampleGame `Tests/data.xml`). Habitable or vacuum orbits stay `orbit`.

Module `operation-allowed-in planet-atmosphere="…"` names an atmosphere **band token** (`terair`, `thin`, `hostile`, `none` — same strings as map `atmosphere=` attrs). `BodyEnvironment.HasAtmosphereResources` passes when the location’s orbit `<resource>` list includes that item type, or the parent body’s atmosphere band matches, or the parent **did not** emit explicit environment attrs (`HasEnvironmentAttrs`: any of `gravity`, `temperature`, `atmosphere` on planet/moon XML). When attrs were emitted and the band does not match, the week logs `PRODUCE failed: {module} cannot operate in {location}.` SampleGame Earth omits those attrs, so wind plants and other `terair` modules still produce there. Explicit `atmosphere="none"` on a sea body blocks them (unit test fixture).

Module `operation-allowed-in planet-type="…"` restricts passive operations to the parent planet or moon’s catalog type (e.g. `gasgnt` for ram scoops). Checked via `BodyEnvironment.MatchesPlanetType`; same `cannot operate in {location}` failure line as the other gates.

### REPAIR

**Syntax:** `REPAIR`

**Subject:** modulestack only.

Spends spare parts (`spare`) and restores damage on the stack (or its parent scope). Engineering shop `[engshp]` restores **20 damage per active copy** and consumes **1 spare per copy**; otherwise **10 damage for 1 spare**; **1 damage** if unsupplied. Event: `repaired N damage.` For a slower tech path that consumes spare at job start, use `**USE repair`** (see [USE](#use)).

### RESEARCH

**Syntax:**

- `RESEARCH`
- `RESEARCH <stack-id|technology-id|tag|item-id|module-id|space-object>`
- `RESEARCH TECHNOLOGY <id>`
- `RESEARCH ITEM <id>`
- `RESEARCH MODULE <id>`
- `RESEARCH GROUP <group>` — quote `"space station"` (two words)
- `RESEARCH TAG <tag>`

**Subject:** modulestack (must be group **research**).

Weekly output is catalog `research-output` × module count plus `**produce effect="research output"`** from trained officers anywhere on the lab stack whose `usable-in` matches the lab root (`Research.WeeklyOutput`; computer library `[cmplib]` is 1; sensor officer `[snsroff]` adds +1 per officer in Tests catalog). Breakthrough is a weekly hazard against the cheapest available tech cost (`Research.RollBreakthrough`; default cost 8, 16, 32… by level, catalog `cost` overrides). Else points accumulate. Preference (`RESEARCH TECHNOLOGY` / `GROUP` / `TAG` / …) is a ~50% pick from the matching subset (`PreferredTechnologies`). Bare tokens resolve in this order: existing **stack id**, known **technology**, **tag** (such as `military`), **item**, **module**, then **space object** (star, planet, moon, belt, region, or orbit id). `TAG` forces a tag preference even when the token is also a technology id. `RESEARCH TAG repair` prefers catalog techs whose `tags` include `repair`: medical services `[medtec]`, medicines refining `[medirf]`, preventive servicing `[servic]`, and engineering shop `[engshp]` (`engshp` also keeps `production`). `RESEARCH TAG research` prefers file indexing `[filidx]`, advanced computing `[advres]`, sick bay construction `[sckcns]`, and shipboard pharmacy `[pharms]`. Bare `research repair` still matches technology **repair and maintenance** `[repair]` (that id has no `repair` tag). Bare `research military` is a **tag**; `research group military` is a **group**.

**Space-object survey reveal:** when the target is a space-object id (not a wreckage stack id), each week that passes the proximity gate queues that body’s catalog `**description=`** for the faction report under `**Survey reports:**` (`SurveyReports.QueueIfNew` → `ObjectsToShow`; after the report, `AllShown` moves it to `ObjectsSeen`). Each body is shown **once per faction**, like technology reports; objects with no `description` are skipped. Turn **1** reports auto-seed the home **star** and home **planet** blurbs for player factions **2–11** (from the faction’s `corphq` location; `SurveyReports.SeedTurnOneHomeBlurbs`).

**Proximity** (`Research.IsResearcherAtSpaceObject`): the lab must be at the target **orbit**, **region**, or **belt**, or on the target **planet** or **moon**’s orbit or one of its surface regions. **Stars** have no proximity gate (any star id passes). If proximity fails, the week logs `RESEARCH failed: {lab} is not at {object}.` and **no research points accrue** that week. When proximity passes (or the target is a star), reveal is queued if new and the week proceeds normally — RP accrue or a breakthrough roll as for any other `RESEARCH`. Space-object preference still biases breakthroughs toward technologies that produce or consume resources found on that body (`Research.ResourcesOf`).

`GROUP` uses `ModuleTypeGroupXml` tokens: `agricultural`, `ark`, `capital`, `command`, `corvette`, `cruiser`, `destroyer`, `energy`, `extraction`, `frigate`, `habitat`, `infantry`, `military`, `production`, `propulsion`, `research`, `settlement`, `shuttle`, `spacecraft`, `space station`, `storage`, `vehicle`. Quote `"space station"` (`GetQuotedToken`); load also accepts aliases `spacestation` and `spaceStation`. Save writes `group="space station"`, not enum `spaceStation`. Unknown names fall back to untyped `Any` (`parseModuleTypeGroup` catch). GROUP prefers techs whose `usable-in` module group or produced module group matches the stored token: `research group settlement` / `frigate` / `storage` prefer those catalog groups. The stored `space station` token does not equal the engine name `spaceStation`, so that preference currently matches nothing. SampleGame `Tests/data.xml` has no `corvette` / `destroyer` / `cruiser` / `capital` / `ark` module groups. Campaign `campaign/data.xml` emits `group="frigate"` on patrol hull `[sshull]` and distinct groups on `corhul` (`corvette`), `deshul` (`destroyer`), `cruhul` (`cruiser`), and `arkhul` (`ark`).

### TRAIN

**Syntax:**

- On a **person:** `TRAIN SKILL <skill-id>`
- On a **modulestack:** `TRAIN <race-id> OFFICER AS "<alias>"|newN [FOR <parent-stack>]`

**Subject:** person (skill) or stack (officer).

Starts `TrainingSkill` or `TrainingOfficer` (officer requires matching crew of that race). `AS` is required for officer training. Duration is catalog `officer-training-duration` / `training-duration`. Both effects survive save/load as `type="training-officer"` (skill training is the `skill` attribute). After save/load, leftover `TRAIN` reconnects to that effect and **continues** `DurationLeft` (does not restart), same idea as leftover `USE`:

- **Same skill**, or **same officer race and person** — leftover `TRAIN` is kept (not duplicated). Training **continues**.
- **Otherwise** — leftover `TRAIN` is dropped. The old training effect **freezes** while the new `TRAIN` runs. Reissue the original skill or officer to **resume**.
- Conditioned lines (`-train` / `+train`) do not merge or drop leftovers this way.

### USE

**Syntax:** `USE <technology-id> [AS <alias|newN>] [FOR <parent-id>]`

**Subject:** modulestack.

Uses a loaded (or level-0) technology: consumes catalog inputs and after `use-time` produces items or a module. `AS` names the new module stack; `FOR` is the nest parent. `AS` and `FOR` are independent (`use wndtrb for 000021` is valid). Level 0 techs do not need to be copied onto the stack. Duration scales with `UseTime`, efficiency, and active quantity. `use-allowed-in` can restrict module **group**, a specific module type (`module="sckbay"` for shipboard pharmacy `[pharms]`), **location-type** (against `BodyEnvironment.EffectiveLocationType` — gas-giant orbits with atmosphere ≠ `none` count as `atmosphere`), **planet-type**, and **planet-atmosphere** (same band-token gate as `PRODUCE`; failure line `USE failed: {tech} cannot operate in {location}.`).

**Settlement temperature:** if the tech produces a **settlement**-group module, `BodyEnvironment.AllowsSettlement` must pass at the producer’s location. **Habitable** (planet default when XML omits `temperature`) always allows. **Cold** allows only module types `clddom` and `cryhab`. **Hot** allows only `hotdom`. Otherwise `USE failed: {module} cannot settle a {cold|hot} world.` SampleGame `Tests/data.xml` has none of those exception types. Moons with no `temperature` attribute load as **cold** (`ParseTemperature` of an empty string), so `USE popcnt` / `ctypln` / `dmecns` on a SampleGame moon fails this gate. Planets with no attribute load as **habitable**.

In-progress work is a `Producing*` effect. It only ticks when a matching unconditioned `USE` runs that week (`Use()`). After save/load, the leftover order reconnects to that effect (`producing-modules` persists `technology=`):

- **Same technology** — production **continues**; inputs are not consumed again. A new `AS` / `FOR` **retargets** the producing effect’s receiver/parent.
- **Different technology** — the leftover `USE` is dropped. The old producing effect **freezes** (stays on the stack, duration unchanged) while the new `USE` runs. Reissue the original tech to **resume** that frozen effect, still without consuming again.
- Conditioned lines (`-use` / `+use`) do not merge or drop leftovers this way.

Omit `FOR`: `ReceiverParent` defaults to the **producer**. `ProducingModule` treats that as “no extra nest”: the product is **formed as a sibling** (`produced.Parent = Producer.Parent`, same orbit/region). At complete it does **not** stack under the producer. `use spctrl as new102` therefore leaves a command bridge sitting next to the shuttle.

`use TECH as newX for 101` stacks the product under hull `101` when production **completes**, same location required (`STACK failed. Parent is in different location.` if the hull has already left). Alternative: `#modulestack new102` then `stack 101` (immediate, also same location). A nested factory can `USE` while the hull’s long slot is a `MOVE` (one long **per subject**). The shuttle itself may only `USE` in **orbit**.

Effect-producing techs (`use-produce effect=…`) run through `ProducingEffect`. **Repair and maintenance** `[repair]` (`use-time` 2, production-group module): consumes **1 spare** at start, then after duration repairs **1 HP** on the producer’s parent scope (nested factory repairs its parent stack and nested children — same walk as `REPAIR`). Fails with `USE failed: no damage to repair.` if scope has no damage (spare not consumed). Event: `repaired 1 damage.` For faster weekly repairs use `**REPAIR`** instead (10/20 HP per week).