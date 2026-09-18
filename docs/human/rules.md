# SpaceAge player rules

**Engine:** open beta **0.8.001**  
**Updated:** September 2026

Single source of truth for PBEM turn orders: file format, all live verbs, turn flow, market, movement, opening patterns, and combat.

Verbs are **case-insensitive**. Most arguments (stack ids, item ids) are **not**.

---

## Campaign setting — The Alderson Points

In a single catastrophic day on **18 September 2152**, the Points **shut down** for no apparent reasons. **Commerce died. Colonies starved.** After **four years**, most reopened, some new points appeared some pairs become unstable. Some systems remain isolated. And **two Points that led to Earth stayed dark**, leaving only mystery and **Fear**.

---

## Turn basics

- One **turn** = **13 weeks**.
- After each turn you receive a **faction report** (stacks, locations, contracts, battles, bank, orders template with ids to reuse).
- Submit a plain-text **order file** before the deadline. The host runs all factions, advances time, sends new reports.
- **Between-turn** submissions allow only **CONTRACT** and **PRESS**.

End of quarter (week 13): stack **upkeep**, wounded crew rolls, bank **interest**, regional **price drift**, UN city auto-listings.

---

## Order file format

Plain text, **Windows-1251** encoding (same as reports). Use Windows-1251 if names use non-ASCII letters.

### Headers


| Line | Meaning |
|------|---------|
| `#faction <id> "<password>"` | Your faction (password in quotes). First line. |
| `#modulestack <id\|newN>` | Next lines apply to this stack. |
| `#person <id\|newN>` | Next lines apply to this person. |
| `#end` | Optional end marker — put last. |


Wrong-owner headers: orders ignored until a valid subject.

```
#faction 2 "your-password"

#modulestack 200001
@produce cash

#modulestack 200003
withdraw 1500
@buy 25 terran at 50
```

### Comments

Empty lines skipped. `;` or `//` starts a comment (whichever comes first).

### Subjects

- `#modulestack` before stack verbs; `#person` only for person verbs (`TRAIN`, `ACTIVE`).
- Reuse **numeric ids** from your report. New builds: **`new1`**, **`new2`**, … only.

---

## Repeat (`N` and `@`)


| Form | Meaning |
|------|---------|
| *(none)* | Once (repeat 1). |
| `N verb …` | Up to N successes. |
| `@verb …` | Retry **every week**. |


**Immediate:** N = weeks the order may succeed. **Long:** N = full jobs to complete.

`-3 use …` — leading `-` is also a condition (see below).

---

## Conditions (`-` / `+`)

On the **same token** as the verb: `-use farmng`, not `-` alone.


| Prefix | Effect |
|--------|--------|
| `-child` | Runs after **parent finishes**. `move R00009` then `-use twnbld as new1`. |
| `+child` | **Skips parent**; child runs alone. `move R00001` then `+use ssassm` — move stays leftover. |


Nest: `--+-use`, `-+get`. `-` under a long parent waits until that job **completes**.

---

## Immediate vs long

Each week per subject:

1. **Immediate** loop until idle  
2. **One long** order (first ready)  
3. **Immediate** again  


| | Immediate | Long |
|---|-----------|------|
| Per week | Many if they succeed | **One** |
| Duration | Same week (or retry) | Multi-week job |
| Needs operational stack | No | Yes (crew, energy, fuel, repairs) |
| Examples | GET, STACK, ATTACK, BUY | MOVE, USE, PRODUCE, RESEARCH |

**CONTRACT** and **PRESS** = immediate + between-turn allowed. **JUMP** = long slot, one week, no operational check.

---

## Turn sequence (weekly)

Weeks **1–13**, each week:

1. **Orders** — factions, then stacks/people (immediate → long → immediate; effects tick)
2. **Sick bay heal** — medicines convert wounded to healthy
3. **Medical consume**
4. **Contracts** — triggers and rewards
5. **Buy clearing** — **BUY** matches **SELL** / market offers
6. **Battles** — enemy armed stacks at same location

Week 13 then: upkeep, wounded outcomes, bank interest, price update, NPC listings. Standing `@buy` / `@sell` retry weekly at step 5.

---

## Market and banking

**Bank:** quarterly interest; balance in whole credits. **`WITHDRAW`** / **`DEPOSIT`** move cash between bank and stack cargo.

**Trading:**

- **`SELL`** lists goods (`AT AVERAGE` or fixed price).
- **`BUY`** posts bid; matching at **end of week**, not when issued.
- Pay from **stack cash first**, then bank if `SET ALLOW BANK TRUE` (default). `ALLOW BANK FALSE` = local cash only.

**Buy price syntax:**


| | |
|---|---|
| *(omit)* | Any price |
| `AT <n>` | Max price (n ≥ 1) |
| `AT AVERAGE` | Regional list cap |
| `AT +N` | Average + N (outbids plain average) |

Same bid cap in a region → **pro-rata** split. Higher bid wins tier. Tech buys match immediately.

**Sharing:** default stacks share cargo for production/upkeep. `SET SHARING FALSE` isolates a reserve.

---

## MOVE, JUMP, space travel

### MOVE

`MOVE <dest> [<dest2> …]` — **module stack only** (not a person alone).

One line, multiple hops: `move R00014 R00009 R00003`. Destinations: region, orbit, planet, moon, belt, star, anomaly, or gate id from report.

**Disabled stacks cannot move** — stage crew, fuel, repairs first; `@move` fails until operational.

People board vehicles first:

```
#person 200010
active 100
-stack 100
#modulestack 100
move R00014 R00009
```

Space hops use distance (AU), ship speed, and cargo mass. Same-body surface↔orbit ≈ **1 week**. Atmospheric launch may consume **oxyhydro** both ways. Report **Exits:** lines show ground/naval durations.

### JUMP

`JUMP <gate-id>` — one-week Alderson Gate hop; ship hull at paired gate orbit only. No fuel. Campaign maps only.

---

## Common playbooks

### Turn-1 bootstrap

```
#modulestack <hq>
@produce cash

#modulestack <cargob>
@get all food from <farms>
@get all carbon from <cdrill>
sell 50 food at average

#modulestack <farms>
@use farmng

#modulestack <cdrill>
@use hcdril

#modulestack <cplant>
@produce energy

#modulestack <factry>
get 30 iron from <cargob>
get 2 titani from <cargob>
use twnbld as new1

#modulestack new1
transfer 1 to faction 1
```

Military personas may `@produce terran` instead of cash. Stage iron/titani before factory builds.

### Buy crew

```
#modulestack <cargob>
withdraw 1500
@buy 25 terran at 50

#modulestack <farm>
get 15 terran from <cargob>
@use farmng
```

### Disabled stacks

**Disabled** = missing crew, energy, fuel, or repairs. Long orders and MOVE fail until fixed.

Factory units (`use grndtr`, `use armcbt`) start **empty**:

```
#modulestack new1
@get 6 terran from <hq>
@get 10 oil from <hq>
@move R00014
```

Before `@move`: crew, fuel, `@repair` if damaged, `@produce energy` upstream, ship command bridge + `@get h2o2` for atmospheric hops.

`@active newN` then `-+@get` when unit forms mid-quarter. **`USE farmng`** only on farms; **`USE hcdril`** only on surface drills.

**ACTIVATE/DEACTIVATE** = per-module copies. **`SET ONLINE`** = whole stack — for captured/offline stacks, not normal bootstrap.

### Multi-stop MOVE

Prefer one line: `move R00014 R00009 R00003`. Immobile HQ/cargo/factory cannot move.

---

## All 29 verbs

**22 immediate** + **7 long**.

### Immediate

**ACTIVE** — `ACTIVE <stack|newN>` — succeeds if stack is active (condition parent for `-+@get`).

**ACTIVATE** — `ACTIVATE [N|ALL] [MODULES]` — turn inactive module copies on.

**ALIAS** — `ALIAS "<name>"` — stack display alias.

**ATTACK** — `ATTACK <unit-id>` — mark **enemy** (combat resolves later).

**BUY** — `BUY <qty|ALL> <item|module> [AT price|AVERAGE|+N] [EVERYWHERE]` or `BUY <tech-id> [AT price]`. Subject: trading stack. Omit EVERYWHERE unless GM confirms.

**CAPTURE** — `CAPTURE <unit-id>|ALL` — capture tactic; immobile fails weekly.

**CONTRACT** — `CONTRACT <loc> GIVE … REWARD …` | `CONTRACT <loc> RESEARCH … REWARD … UNIT` | `CONTRACT <id> WITHDRAW`. Subject: **#faction**. Between-turn OK.

**DECLARE** — `DECLARE FACTION|UNIT <id> <attitude>` | `DECLARE DEFAULT|UNKNOWN <attitude>`. Attitudes: enemy, hostile, neutral, friendly, ally.

**DEACTIVATE** — `DEACTIVATE [N|ALL] [MODULES]` — mothball module copies.

**DEPOSIT** — `DEPOSIT <qty|ALL>` — cargo cash → bank.

**FORM** — `FORM NEW [WITH n] [AS newN|"alias"]` — new empty stack; always use **AS**.

**GET** — `GET <qty|ALL> <item> FROM <holder>` | `GET ALL FROM <holder>` | `GET <qty|ALL> <item>` | `GET ALL`. Negative qty leaves remainder.

**GIVE** — `GIVE <qty|ALL> <item> TO <holder|newN>` | `GIVE ALL TO …`

**HAS** — `HAS <qty> <item|module-type>` | `HAS PERSON <id>` | `HAS MODULES [qty]` — condition probe.

**NAME** — `NAME "<stack name>"` | `NAME <map-object> "<name>"` — rename (map rename requires presence).

**PRESS** — `PRESS [planet|moon] TITLE "<t>" [FLAVOUR "<f>"]`. Subject: **#faction**. Between-turn OK.

**SEE** — `SEE <stack|newN>` | `SEE PERSON <id>` | `SEE <id> PERSON` — visibility at location.

**SELL** — `SELL <qty|ALL> <item|module> [AT price|AVERAGE]` | `SELL <tech-id> …`

**SET** — `SET AVOID|ONLINE|ALLOW BANK|SHARING TRUE|FALSE`

**STACK** — `STACK <parent|newN>` | `STACK top` | `STACK out` — **`top`/`out` lowercase**. Drones: out or drone bay only.

**TACTIC** — `TACTIC destroy|capture|evade` | `TACTIC prioritize armed|command|storage`

**TRANSFER** — `TRANSFER n TO <stack>` | `TRANSFER ALL [DAMAGED] [MODULES] TO <stack|FACTION id>` | `TRANSFER MODULE index TO …`

**WITHDRAW** — `WITHDRAW <qty|ALL>` — bank → cargo (positive balance only).

### Long

**JUMP** — `JUMP <gate-id>` — Alderson hop, 1 week, ships only.

**MOVE** — `MOVE <dest> [dest2 …]` — multi-hop travel; disabled stacks fail.

**PRODUCE** — `PRODUCE ENERGY` | `PRODUCE <item-id>` — e.g. `@produce cash`, `@produce energy`. Location/atmosphere gates apply.

**REPAIR** — `REPAIR` — weekly spare-part repair.

**RESEARCH** — `RESEARCH` | `RESEARCH <target>` | `RESEARCH TECHNOLOGY|ITEM|MODULE|GROUP|TAG <id>`. Lab stack; `"space station"` group needs quotes. Proximity required for space-object targets.

**TRAIN** — `TRAIN SKILL <skill>` (person) | `TRAIN <race> OFFICER AS "name"|newN [FOR stack]` (stack).

**USE** — `USE <tech> [AS newN] [FOR parent]` — build/produce; inputs from factory + sharing allies; `FOR` nests product under parent.

---

## Combat overview

After orders and market each week: battles where **armed operational root** stacks have **enemy** attitude toward co-located enemies.

### Diplomacy


| Attitude | Role |
|----------|------|
| enemy | Starts/joins combat |
| hostile | No join |
| neutral | No combat |
| friendly | Defends |
| ally | Attacks and defends with ally |

**DECLARE** sets stance; **ATTACK** / **CAPTURE** mark enemy. One battle per location per faction pair per week.

### Tactics (TACTIC + CAPTURE)


| Order | Effect |
|-------|--------|
| ATTACK | Mark enemy; targeting bias |
| DECLARE | Stance only |
| CAPTURE | Capture tactic; peel disabled foes after win |
| TACTIC destroy | Kill; explicit post-win cleanup of routed units |
| TACTIC capture | Seize modules → stacks `c00001`, … |
| TACTIC evade | Harder to hit; exit after 2 unhit rounds |
| TACTIC prioritize armed\|command\|storage | Target preference |

Immobile units: **destroy** only; capture/evade fail once/week. **SET AVOID** is not a battle tactic.

### Battle flow

Up to **10 rounds**; lower **initiative** fires first. Hangar drones launch after round 1 roster; fight from round 2. Destroy shots full damage; capture splits hull/capture damage. Wreck at 0 HP. Clear win + explicit **destroy**/**capture**/**scavenge** decides fate of **disabled** survivors — default firing alone does not auto-kill routed units.

**Tips:** `-declare faction N enemy` before engagement; `TACTIC destroy` to mop fauna; board officers on combat stacks; `STACK OUT` drones for round-1 fire.

---

## Quick reference


| Immediate (22) | Long (7) |
|----------------|----------|
| ACTIVE, ACTIVATE, ALIAS, ATTACK, BUY, CAPTURE, CONTRACT, DECLARE, DEACTIVATE, DEPOSIT, FORM, GET, GIVE, HAS, NAME, PRESS, SEE, SELL, SET, STACK, TACTIC, TRANSFER, WITHDRAW | JUMP, MOVE, PRODUCE, REPAIR, RESEARCH, TRAIN, USE |


| Pattern | Meaning |
|---------|---------|
| `@verb` | Every week |
| `5 verb` | Up to 5 times |
| `-verb` | After previous finishes |
| `+verb` | Skip previous |
| `-+@get` | Wait for active, retry GET |

Read before ordering: stack status (crew/fuel/disabled), **Exits:** durations, **Bank report**, **Contracts**, **Battles**, footer template ids.

---

*SpaceAge open beta 0.8.001*
