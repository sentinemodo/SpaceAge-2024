# Player order syntax

Checked **19 Aug 2026** against engine **0.1.141** (`Game/Program.cs`).

Sources: `Game/orders/EOrderType.cs`, `Game/orders/OrdersReader.cs`, `Game/orders/Orders.cs`, `Game/Game.cs` (week loop), `Game/data structures/ModuleStack.Upkeep.cs` (sick bay, medical consume, quarterly maintenance), `Game/game/DataFile.cs` (`LoadOrders` delegates to `OrderXml`), `Game/game/OrderXml.cs` (XML switch), `Game/effects/Effects.cs` (`LoadXml` effect types), each `Game/orders/*Order.Parse` / `Execute`. Sample prefix usage: `Tests/SampleGame/orders.*.txt`.

Not source of truth: `Game/documentation/Rules.txt`. Turn order files are **Windows-1251** (same as reports). Verbs are case-insensitive; most arguments are not.

**25** verbs parse from text (`OrdersReader` switch): **19 immediate**, **6 long**. See [Turn sequence](#turn-sequence), [Immediate vs long](#immediate-vs-long), and [Text vs XML](#text-vs-xml). `TRANSFER` loads from XML only.

## Prefixes and subjects

Order of a line after comments are stripped: **leading `-`/`+` conditions**, then **duration** (`N` on its own token, or `@` glued to the verb), then the **verb**. `OrdersReader` counts leading `+`/`-` on the first token as condition depth, then tries to read that token as a repeat count, then strips `@`/`+`/`-` from the verb.

### File headers


| Line                         | Meaning                                                                                                                             |
| ---------------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| `#faction <id> "<password>"` | Selects the faction and sets it as subject. Password must match (`GetQuotedToken`). Must appear before stacks/people.               |
| `#modulestack <id|newN>`     | Subject becomes that stack. Faction must already be set and must own it. Unknown ids may be created as unformed `newN` aliases.     |
| `#person <id|newN>`          | Subject becomes that person. Same ownership rules.                                                                                  |
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

1. Load catalog (`data.xml`) and the saved game.
2. Load requests and GM events; run `Events.Execute` (not player orders).
3. Load `order.*` files (`OrdersReader`).
4. `Game.Execute` (below).
5. Write faction reports, then save the game.

`/no-turn` skips the 13 weeks: load orders, run **between-turn** immediates only (`AllowedBetweenTurns` — live text verbs: `CONTRACT`, `PRESS`), write announcements (`announce.{turn}.{faction}.txt` for new contracts at that location and for press releases), save.

`/check` parses an order file and does not execute.

### Each turn (`Game.Execute`)

Clear last turn’s event reports, then `turn++`.

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
7. **Battles** — `Battle.StartAtLocations` then `Execute` (see `player/battle.md`).

After week 13: **quarterly maintenance**, then **quarterly wounded outcome**, then clear long/immediate flags; drop unformed stacks that should not report (`RemoveNonReporting`).

**Quarterly maintenance** (`ExecuteMaintenance`, once, week 13): cash upkeep, then food, then terran air `[terair]` if the stack needs canned air (orbit, space, or a moon region). Bills pull from this stack then parent nests; cash shortfall can also debit the faction bank. Unpaid food/air can wound healthy terrans (catalog 25%). Unpaid cash can damage the module or print race off-duty. `medici` is skipped here (already weekly).

**Quarterly wounded outcome** (once, week 13): each remaining `wndtrn` rolls independently — **25%** die of wounds, **25%** recover to terran, **50%** stay wounded. Not gated on medici. `madtrn` is not rolled here.

**End of turn (once):**

- Bank: quarterly interest (`AddQuarterlyInterest`).
- `UpdateRates` — empty (comments only).
- `GenerateOffers` — empty (no NPC auto-offers). Duration-0 leftover `receiving-items` on cities (old market delivery) **persist** after save but **never complete**; there is no player verb that clears them.

Standing `@buy` / `@sell` stay on the order list and retry each week at step 6. `ATTACK` / `TACTIC` / `DECLARE` during step 2 only set stance; shooting is step 7.

## Immediate vs long

A turn is **13 weeks**. Each week, for each faction / stack / person, the engine:

1. Runs **immediate** orders in a loop until a pass does nothing.
2. Runs **at most one long** order (the first ready one on that subject).
3. Runs **immediate** again, so `GET` / `STACK` / `SEE` can fire the same week after a move or a finished build.

The two kinds are independent except where you chain them with `-` / `+`. An immediate line after an unfinished long still runs if it has no pending conditions.


|                       | Immediate                                                                      | Long                                                                                     |
| --------------------- | ------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------- |
| How many per week     | As many as can succeed (loop until idle)                                       | **One** per subject                                                                      |
| Duration              | Finishes in the week it succeeds (or retries later)                            | Occupies the long slot for `Duration` weeks (`use-time`, produce duration, move legs, …) |
| Needs an active stack | No (probes and cargo still have their own checks)                              | Yes: formed, enough crew and energy (`CanOperate`)                                       |
| Repeat `N` / `@`      | Succeed up to N weeks, or every week forever                                   | Complete N full durations, or never stop                                                 |
| After a turn          | Dropped when `Executed` and repeat is used up; otherwise stays on the template | Same; in-progress work is kept as an effect (`Moving`, `Producing*`, `Training*`, `Receiving*`, `Fuelled`; `lightly-damaged` is a module marker). Those types **load again** after save (`Effects.LoadXml`). |


**Immediate** orders are setup, probes, cargo, stance, and stacking. They do not consume the week’s long slot. `FORM`, `GET`, and `GIVE` can all fire the same week as a `USE` or `MOVE`.

**Long** orders are the week’s work: move, produce, repair, research, train, use. A second long on the same subject waits until the first completes (or until its conditions clear). Accepting modules mid-week can mark the receiver as having already used its long slot.

`CONTRACT` and `PRESS` are immediate and also **allowed between turns** (`/no-turn`). No other live text verb is.

## Text vs XML

`DataFile.LoadOrders` calls `OrderXml.LoadAll`, which builds orders from `<order>` XML when loading a game. Divergences:


| Verb                                    | Text (`OrdersReader`)                                    | XML (`OrderXml.LoadAll`)                            |
| --------------------------------------- | -------------------------------------------------------- | --------------------------------------------------- |
| `TRANSFER`                              | **missing** — “Unknown order”                            | loads (`TransferOrder`)                             |
| `COPY` comments mention `COPY all TO …` | **not parsed** — technology id required                  | technology + receiver attributes                    |


Player turn files use **text**. XML matters for saved games, not for `order.*` drafts. Text `-` / `+` syntax is unchanged.

**Nested conditions:** `Order.SaveXml` writes leftover `-` / `+` children as nested `<order conditions="…">` under the parent (top-level save is `Level == 0` only). `saveXml_post` writes only the **next remaining condition level**, once (no duplicate nested siblings); recursion still persists leftover `+USE` trees. `LoadAll` walks those nested `<order>` elements and assigns the same `-` / `+` links as text (`AssignCondition`). Duplicate subject + conditions + verb XML is skipped. Frozen SampleGame `gamein.2_contract.xml` / `gamein.3_contract.xml` keep those leftovers; player-facing turn 2/3 reports are unchanged.

**Saved effects:** `Effects.LoadXml` accepts `fuelled`, `moving`, `producing-modules`, `producing-items`, `producing-energy`, `receiving-items`, `receiving-modules`, `receiving-technology`, `lightly-damaged`, and `training-officer`. `receiving-items` now writes a nested `<receiving>` cargo payload. Skill training still saves as `type="training-officer"` with a `skill` attribute (not a player-facing verb — issue `TRAIN SKILL`). `USE` leftover reconnects to a matching `Producing*` effect; leftover `PRODUCE` / `TRAIN` do **not** (`Producing` / `Training` on the order is null after load, so they start a new duration while the loaded effect stays frozen). Duration-0 `receiving-items` leftovers do not deliver cargo.

---

## Immediate orders

ACTIVE, ALIAS, ATTACK, BUY, CAPTURE, CONTRACT, COPY, DECLARE, FORM, GET, GIVE, HAS, NAME, PRESS, SEE, SELL, SET, STACK, TACTIC.

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

Posts a standing buy on the local market (`Offer.Process`). Quantity `ALL` sets `AllQuantity`. Omitted `AT` leaves price unrestricted (`Price = -1`). First token is treated as a **technology id** if it is in the catalog (do not write a trailing `technology` word — Parse would reject it). Sample: `@buy all terran`.

### CAPTURE

**Syntax:** `CAPTURE <unit-id>|ALL`

**Subject:** modulestack.

Sets tactic to **capture**. A specific id is the preferred target and is marked enemy if that stack exists. `ALL` prefers every enemy at the location. Immobile stacks cannot capture (destroy only).

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

Copies a technology the source holds onto a **pre-existing** receiver at the **same location**, if the receiver has remaining technology capacity. `COPY all` is not implemented.

### DECLARE

**Syntax:**

- `DECLARE FACTION <id> <attitude>`
- `DECLARE UNIT <id> <attitude>`
- `DECLARE DEFAULT <attitude>`
- `DECLARE UNKNOWN <attitude>`

**Subject:** any orderable (applies to its owner).

One-way stance. Attitudes (case-insensitive): `enemy`, `hostile`, `neutral`, `friendly`, `ally`. `enemy` is the combat stance (faction-wide when targeting a faction). Sample: `-declare faction 2 enemy`.

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

**Syntax:** `SEE <stack-id|newN>` or `SEE PERSON <person-id|newN>`

**Subject:** holder.

Succeeds if that stack or person is at the observer’s location. Template/report may print `see id person`; **Parse expects `SEE PERSON id`**.

### SELL

**Syntax:**

- `SELL <quantity\|ALL> <item-id\|module-id> [AT <price>|AVERAGE]`
- `SELL <technology-id> [AT <price>|AVERAGE]`

**Subject:** offerent.

Lists a standing sell (`Offer`). Matching is driven from the buy side; Execute does not complete the trade itself. `AT AVERAGE` uses the local market price. Sample: `-sell 1 wnplnt`. Same technology-id rule as BUY (no trailing `technology` word).

### SET

**Syntax:** `SET AVOID TRUE` or `SET AVOID FALSE`

**Subject:** modulestack.

Sets `IsAvoiding`. `**AVOID`, `TRUE`, and `FALSE` must be uppercase** (Parse does not fold case).

### STACK

**Syntax:** `STACK <parent-id|newN>` or `STACK TOP` or `STACK OUT`

**Subject:** stack (or person as item holder).

Nests the subject under another stack (same location, same faction, not self), under the root parent (`TOP`), or out into the location (`OUT`). `**top` and `out` must be lowercase.**

### TACTIC

**Syntax:** `TACTIC destroy|capture|evade` or `TACTIC prioritize armed|command`

**Subject:** modulestack.

Persists firing/evade/priority tactics. Destroy and capture are exclusive. Immobile stacks may only `destroy`. Sample: `-tactic prioritize armed`, `-tactic capture`.

---

## Long orders

MOVE, PRODUCE, REPAIR, RESEARCH, TRAIN, USE.

### MOVE

**Syntax:** `MOVE <dest> [<dest2> …]`

**Subject:** modulestack.

Walks a route. Each dest token is a **region**, **star**, **planet**, **moon**, **anomaly**, or **orbit** id (stars/planets/moons/anomalies resolve to their orbit). Starts a `Moving` effect, consumes fuel when required, changes parent on arrival.

### PRODUCE

**Syntax:** `PRODUCE ENERGY` or `PRODUCE <item-id>`

**Subject:** modulestack.

Starts energy or item production using the stack’s module type (`ProducingEnergy` / `ProducingItems`), duration from `ProduceDuration`. Those effects survive save/load; a leftover `@produce` does **not** reconnect to them (unlike leftover `USE`). Sample: `@produce cash`, `@produce energy`, `@produce terran`.

### REPAIR

**Syntax:** `REPAIR`

**Subject:** modulestack only.

Spends spare parts (`spare`) and restores hit points on the stack (or its parent scope). Engineering shop `[engshp]` restores **20 HP per active copy** and consumes **1 spare per copy**; otherwise **10 HP for 1 spare**; **1 HP** if unsupplied.

### RESEARCH

**Syntax:**

- `RESEARCH`
- `RESEARCH <stack-id|technology-id|tag|item-id|module-id|space-object>`
- `RESEARCH TECHNOLOGY <id>`
- `RESEARCH ITEM <id>`
- `RESEARCH MODULE <id>`
- `RESEARCH GROUP <group>`
- `RESEARCH TAG <tag>`

**Subject:** modulestack (must be group **research**).

Weekly research output; chance of a breakthrough, else points accumulate. Bare tokens resolve in this order: existing **stack id**, known **technology**, **tag** (such as `military`), **item**, **module**, then **map object** (moon/planet/region/orbit). `TAG` forces a tag preference even when the token is also a technology id. `RESEARCH TAG repair` prefers catalog techs whose `tags` include `repair`: medical services `[medtec]`, medicines refining `[medirf]`, preventive servicing `[servic]`, and engineering shop `[engshp]` (`engshp` also keeps `production`). `RESEARCH TAG research` prefers file indexing `[filidx]`, advanced computing `[advres]`, sick bay construction `[sckcns]`, and shipboard pharmacy `[pharms]`. Bare `research repair` still matches technology **repair and maintenance** `[repair]` (that id has no `repair` tag). `GROUP` accepts: `agricultural`, `command`, `spacecraft`, `energy`, `extraction`, `habitat`, `infantry`, `military`, `production`, `propulsion`, `research`, `vehicle`. Other group names (including `frigate`, `settlement`, `storage`) are stored as an untyped token.

### TRAIN

**Syntax:**

- On a **person:** `TRAIN SKILL <skill-id>`
- On a **modulestack:** `TRAIN <race-id> OFFICER AS "<alias>"|newN [FOR <parent-stack>]`

**Subject:** person (skill) or stack (officer).

Starts `TrainingSkill` or `TrainingOfficer` (officer requires matching crew of that race). `AS` is required for officer training. Both effects survive save/load as `type="training-officer"` (skill training is the `skill` attribute). A leftover `TRAIN` does **not** reconnect to the loaded effect (unlike leftover `USE`).

### USE

**Syntax:** `USE <technology-id> [AS <alias|newN>] [FOR <parent-id>]`

**Subject:** modulestack.

Uses a loaded (or level-0) technology: consumes catalog inputs and after `use-time` produces items or a module. `AS` names the new module stack; `FOR` is the nest parent. `AS` and `FOR` are independent (`use wndtrb for 000021` is valid). Level 0 techs do not need to be copied onto the stack. Duration scales with `UseTime`, efficiency, and active quantity. `use-allowed-in` can restrict both module **group** and a specific module type (`module="sckbay"` for shipboard pharmacy `[pharms]`).

In-progress work is a `Producing*` effect. It only ticks when a matching unconditioned `USE` runs that week (`Use()`). After save/load, the leftover order reconnects to that effect:

- **Same technology** — production **continues**; inputs are not consumed again. A new `AS` / `FOR` **retargets** the producing effect’s receiver/parent.
- **Different technology** — the leftover `USE` is dropped. The old producing effect **freezes** (stays on the stack, duration unchanged) while the new `USE` runs. Reissue the original tech to **resume** that frozen effect, still without consuming again.
- Conditioned lines (`-use` / `+use`) do not merge or drop leftovers this way.

Omit `FOR`: `ReceiverParent` defaults to the **producer**. `ProducingModule` treats that as “no extra nest”: the product is **formed as a sibling** (`produced.Parent = Producer.Parent`, same orbit/region). At complete it does **not** stack under the producer. `use spctrl as new102` therefore leaves a command bridge sitting next to the shuttle.

`use TECH as newX for 101` stacks the product under hull `101` when production **completes**, same location required (`STACK failed. Parent is in different location.` if the hull has already left). Alternative: `#modulestack new102` then `stack 101` (immediate, also same location). A nested factory can `USE` while the hull’s long slot is a `MOVE` (one long **per subject**). The shuttle itself may only `USE` in **orbit**.

Effect-producing techs (catalog `use-produce effect=…`) hit “Not implemented” in Execute — use `REPAIR` for repairs.