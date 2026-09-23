# SpaceAge player rules

**Engine:** open beta **0.8.001**  
**Updated:** September 2026

Single source of truth for PBEM turn orders: file format, all live verbs, turn flow, market, movement, opening patterns, and combat.

This is a **complex ruleset** and difficult game in terms of orders. A significant level of conditioning and flexibility is possible in drafting orders. I have found out on myself that this lead to a significant number of errors and the **SpaceAge is unforgiving**. While combat is by design extended and weighted toward defender the environment is not. Triple check the orders. **always take more terair and food and fuel than you think is needed.**

Verbs are **case-insensitive**. Most arguments (stack ids, item ids) are **not**.

---

## Turn basics

- One **turn** = **13 weeks**.
- **On each** week each unit (modulestack or person) can execute any number of immediate orders and a single long order.
- **On each** week various week events may happen (a battle, a market sale, a contract completion, fuel consumption etc.) 
- **On end of quarter (week 13)** various turn events may happen (an **upkeep**/**maintenance**, wounded crew status change, a bank **interest**, regional **price drift** etc.).
- After each turn you receive a **faction report** (stacks, locations, contracts, battles, bank, orders template with ids to reuse).

- Submit a plain-text **order file** before the deadline. The host runs all factions, advances time, sends new reports.
- **Between-turn** submissions allow only **CONTRACT**, **RUMOR** and **PRESS**. They will be also issued immediately.

---

## Key words

Each keyword below has an in-game id (usually six letter/numbers):

- **Module type** - a class of modules (e.g. factory or vehicle)
- **Module** - an instance of module type, have size, mass, accumulate damage, need upkeep
- **Modulestack** - a logic grouping of modules of the same type, receive orders, may be placed at space objects (orbits, regions), may nest other modulestacks (cargo bay within hull, infantry within trucks), may be influenced by effects, store itemstacks and technologies
- **Item type** - a class of items (e.g. iron or infantry rockets)
- **Item** - an instance of item type, have size, mass
- **Itemstack** - a logical group of items
- **Person** - a special itemstack, consisting of one intelligent being (usually terran), have all characteristic of item, may have skills
- **Technology** - a usable technology stored in modulestacks 
- **Space object** - a physical location within star system (planet, moon, asteroid belt), may have an orbit or other space objects, have UA distace from nearest reference point (a star or planet) 
- **Region** - a physical location on planet, hold resources, have x,y position and exits to neighbouting regions
- **Effect** - a characteristic influencing or describing modulestack (moving, producing, burning)

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


Wrong-owner headers: orders **are ignored** until a valid subject.

```
#faction 2 "your-password"

#modulestack 200001
@produce cash

#modulestack 200003
withdraw 1500
buy 25 terran at 50
```

### Comments

Empty lines skipped. `;` or `//` starts a comment.

### Subjects

- `#modulestack` before stack verbs; `#person` only for person verbs (`TRAIN`, `ACTIVE`).
- **new unit** - a new modulestack or person id that can be referred in same turn orders. They must begin with word new followed by the number such as **`new1`**, **`new2`**, etc

---

## Repeat (`N` and `@`)


| Form | Meaning |
|------|---------|
| *(none)* | Once (repeat 1). |
| `N verb …` | Up to N successes. |
| `@verb …` | Retry **every week**. |


**Immediate:** N = weeks the order may succeed. **Long:** N = full jobs to complete.

---

## Conditions (`-` / `+`)

On the **same token** as the verb: `-use farmng`, not `-` alone.

**`-child`** prefix:
Runs after **parent finishes**, eg. MOVE **and then** USE technology and build a town.
``` 
move R00009
-use twnbld as new1
```

**`+child`** prefix:
**Skips parent**; parent can execute only after all child runs complete, eg. GET item **and then** MOVE somewhere else. 
```
move R00001
+get 20 iron from 200001
``` 

Nesting is possible: `--+-use`, `-+get`. `-` under a long parent waits until that job **completes**, but keep in mind the sequence and don't skip levels `use` followed by `--get` **is an error**.

---

## Immediate vs long

Each week per subject:

1. **Immediate** loop until idle  
2. **One long** order (first ready)  
3. **Immediate** loop until idle again  


| | Immediate | Long |
|---|-----------|------|
| Per week | Many if they succeed | **One** |
| Duration | Same week (or retry) | Multi-week job |
| Needs operational stack | No | Yes (crew, energy, fuel, repairs) |
| Examples | GET, STACK, ATTACK, BUY | MOVE, USE, PRODUCE, RESEARCH |

**CONTRACT** **RUMOR** and **PRESS** = immediate + between-turn allowed. 

---

## Turn sequence (weekly)

Weeks **1–13**, each week:

1. **Orders** — factions, then stacks/people (immediate → long → immediate; effects tick)
3. **Contracts** — triggers and rewards
4. **Buy clearing** — **BUY** matches **SELL** / market offers
5. **Battles** — enemy armed stacks at same location

Week 13 then: upkeep, wounded outcomes, bank interest, price update, NPC listings. Standing `@buy` / `@sell` retry weekly at step 5.

---

## Market trading and banking

**Trading:**

- **`SELL`** lists goods (`AT AVERAGE` or fixed price).
- **`BUY`** posts bid; matching at **end of week**, not when issued.
- Pay from **stack cash first**, then bank if `SET ALLOW BANK TRUE` (default).
- Same bid cap in a region → **pro-rata** split; higher bid wins tier.
- Tech buys match immediately.
- The outcome of the sale influences the market locally and on the galactic scale.

**Banking:** 

Bank credits/debits the interest quarterly; balance is in whole credits. 
- **`WITHDRAW`** / **`DEPOSIT`** move cash between bank and stack cargo.
- `SET ALLOW BANK TRUE` (default). `ALLOW BANK FALSE` = local cash only.

---

## Maintenance, upkeep and sharing

- Each unit have upkeep section. Upkeep is paid quarterly.
- Lack of resources lead to wounds/death of the crew and disabling or other detrimental effects (riots, damage) of the modulestacks.
- Resources are by default shared between same faction modulestacks within region. Cash is being pulled from bank for upkeep if allowed. 
- Breathing air (terair for terrans) is not needed when on regions with breathable atmosphere.
**Sharing:** default stacks share cargo for production/upkeep. `SET SHARING FALSE` isolates a reserve.

---

## MOVE, space travel and JUMP

### MOVE and space travel

Movement is possible between neighbouring regions (identified in exits). Duration and mode of travel is defined in **Exits** (ground or naval).

For space capable units orbit is reachable from any region and **any region** is reachable from orbit. The time to travel to orbit and region takes 1 week. Atmospheric launch may **consume fuel** both ways.

Move **between asteroids** within asteroid belts take 1 week.

True spaceships (frigate hulls and above) **cannot land** on regions if there is an atmosphere.

Move between space destination is calculated depending on the **relative distance** measured in UA and takes into account **thrust and mass**. The time is **rounded up** to the nearest week and slightly simpler formula for less than 5 weeks travel durations.

```
base travel time = 
    orbital movement time = 6
    travel time = 33 × ln((1 + ΔAU) / 2.7) / ln(80 / 2.7)
    base time = orbital movement time + travel time

effectiv speed 
    load           = thrust / max(mass, 1)
    reference Load = 40000 / 4150    // ≈ 9.6386
    mass Factor    = clamp(load / referenceLoad, 0.67, 1.50)

space travel duration = max(1, ceil(base travel time / effective Speed))
```

this roughly gives the following in weeks
| Hop | Scout | Default | Cargo |
|-------|-----|-----|-----|
|Planet - Moon <0.1 UA |2|2|3|
|Planet - Planet ca 2 UA|4|6|9|
|Inner Planet - Outer planet ca 5 UA|9|13|19|
|Inner Planet to Alderson point 50+ UA|26|39|60|

**Disabled stacks cannot move** — stage crew, fuel, repairs first; `move` fails until operational.

**Spaceships need a complete setup before they can move** - a minimum viable spaceship must include hull, command bridge, powerplant and engine. It should also have crew quarters and cargo bay. All of those modulestacks need to have enough crew, fuel, energy to operate at least single module. 

**Immobile stacks cannot move on its own** - stacks without move capability HQ/cargo/factory need to STACK within modulestack that can move as a whole.

**Person cannot move on its own** - person need to STACK within modulestack that can move as a whole. 

### JUMP

`JUMP <gate-id>` — one-week Alderson Gate hop; ship hull at paired gate orbit only. Does not consume fuel. 

---

## Full orders dictionary - All 31 verbs

**24 immediate** + **7 long**.

### Immediate

**ACTIVE** — `ACTIVE <stack|newN>` — succeeds if stack is active (condition parent for `-get`).

**ACTIVATE** — `ACTIVATE [N|ALL] [MODULES]` — turn inactive module copies on. Increase upkeep.

**ALIAS** — `ALIAS "<name>"` — stack display alias.

**ATTACK** — `ATTACK <unit-id>|REGION <region-id>` — mark unit as **enemy**. equivalent of DECLARE UNIT <unit-id> ENEMY. Combat may be resolved later depending on units visibility.
ATTACK REGION forces move into region and declare unit preventing entry ENEMY.

**BUY** — `BUY <qty|ALL> <item|module> [AT price|AVERAGE|+N] or `BUY <tech-id> [AT price]`. 

| | |
|---|---|
| *(omit)* | Any price |
| `AT <n>` | Max price (n ≥ 1) |
| `AT AVERAGE` | Regional list cap |
| `AT +N` | Average + N (outbids plain average) |

Same bid cap in a region → **pro-rata** split. Higher bid wins tier. Tech buys match immediately.

**CAPTURE** — `CAPTURE <unit-id>|ALL|REGION <region-id>` — set unit to capture tactics and declare unit ENEMY.
CAPTURE REGION forces move into region and declare unit preventing entry ENEMY.

**CONTRACT** — `CONTRACT <loc> GIVE … REWARD …` | `CONTRACT <loc> RESEARCH … REWARD … UNIT` | `CONTRACT <id> WITHDRAW`. Issue new contract that can be executed by any faction. The contract reward must be present at the moment of issuing. Reward is trasnfered to faction executing the contract. OK to issue between turns. Will be announced until completed or withdrawn.

**DECLARE** — `DECLARE FACTION|UNIT <id> <attitude>` | `DECLARE DEFAULT|UNKNOWN <attitude>`. Attitudes: enemy, hostile, neutral, friendly, ally. Can be issued as **#Faction** order

|Attitude|Behaviour|
|--------|----------------|
|Ally|Joins battles when ally attacks. Allow movement. Allow name change when patrolling.|
|Friendly|Joins battles when friendly faction is attacked. Allow movement.|
|Neutral|Do not join battles unless attacked. Allow movement.|
|Hostile|Do not attack. Prevent movement if patrolling. Does not accept GIVE.|
|Enemy|Attack when seen. Prevent movement if patrolling. Does not accept GIVE.|

**DEACTIVATE** — `DEACTIVATE [N|ALL] [MODULES]` — mothball module copies. Reduces upkeep. do not change the mass.

**DEPOSIT** — `DEPOSIT <qty|ALL>` — move cash to bank.

**FORM** — `FORM NEW [WITH n] AS [newN|"alias"]` — form new stack; empty or filled with modules from the modulestack. 

**GET** — `GET <qty|ALL> <item> FROM <holder>` | `GET ALL FROM <holder>` | `GET <qty|ALL> <item>` | `GET ALL`. Get itemstack from modulestack. Negative qty leaves all but remainder. Both units must be present in the region. You can only GET from your own units.

|Get value|Holder amount before get|Holder amount after get|
|--------|-------|--------|
|10|30|20|
|10|5|5 - ERR: insufficient amount to get|
|ALL|30|0|
|-10|30|10|
|-40|30|30  - ERR: insufficient amount to get|

**GIVE** — `GIVE <qty|ALL> <item> TO <holder|newN>` | `GIVE ALL TO …` Same as GET but from giver perspective. You can only GIVE to units with whom you have a Nurtal or better attitude.

**HAS** — `HAS <qty> <item|module-type>` | `HAS PERSON <id>` | `HAS MODULES [qty]` — condition probe. Executed when modulestack has an item or module.

**NAME** — `NAME "<stack name>"` | `NAME <region-object>|<space-object-id> "<name>"` — rename (map rename requires presence). Cannot change the <region-id>|<space-object-id> name if there is a patrolling modulestack of non-ally different faction.

**PRESS** — `PRESS [planet|moon] TITLE "<t>" [FLAVOUR "<f>"]`. Subject: **#faction**. Between-turn OK. Issue a press release that will be added to the report (and immediatelly issued if submitted between turns). The press release will be visible to all present on the planet/moon. The sender of the press release **will be indicated.**

**RUMOR** — `RUMOR [planet|moon] TITLE "<t>" [FLAVOUR "<f>"]`. Subject: **#faction**. Between-turn OK. Issue a rumor message that will be added to the report (and immediatelly issued if submitted between turns). The rumore will be visible to all present on the planet/moon. The sender of the rumor **will not indicated.**

**SEE** — `SEE <stack|newN>` | `SEE PERSON <id>` | `SEE <id> PERSON` — a conditional order. execute when modulestack can see the target unit. May be limited by stealth technologies. You can always see your own units in the region regardless of visibility factors.

```
#modulestack <producer>
SEE <transporter>
-GIVE ALL <item-type>
```

**SELL** — `SELL <qty|ALL> <item|module> [AT price|AVERAGE]` | `SELL <tech-id> …`. Place an offer to sell the item, module or technology on the region's market.

**SET** — `SET AVOID|ALLOW BANK|SHARING|PATROL TRUE|FALSE`

|Flag|Behaviour|
|-----|----------|
|Avoid|If true do not join battles unless attacked.|
|Allow Bank|if true unit can draw cash from bank when needed - upkeep, or market purchase.|
|Sharing|If true unit will share resources when other units need them for fuel, upkeep, production|
|Patrol|If true unit will prevent hostile units entry and will prevent region, space object names changes by non-allies|

**STACK** — `STACK <parent|newN>` | `STACK top` | `STACK out` — **`top`/`out` lowercase**. Nest the modulestack under different parent unit or eject them to the region. Unit must have sufficient capacity to accept the stacked unit size. Some units may only be stacked under specific type of modules, e.g. figther drones may only be stacked under drone bay if they are to participate in combat.

**SYNCHRO** — `SYNCHRO <tag>` — rendezvous. Every live `SYNCHRO` with the same tag (any faction or unit; case-insensitive) must be ready in the same week before any of them execute. A single copy never fires. A further copy of the same tag holds the signal until it is ready too. When the last one is ready, all of them execute together and release orders waiting on them. `-synchro <tag>` signals after the parent finishes (after a `MOVE` arrives). `+synchro <tag>` makes the parent wait until the signal fires.

```
#modulestack 000001
synchro move_signal
-move R00001

#modulestack 000002
has 2 tanks
-synchro move_signal
```

**TACTIC** — `TACTIC destroy|capture|evade` | `TACTIC prioritize armed|command|storage` sets unit behaviour during combat. The default tactics are destroy and prioritize armed.

|Tactic|Behaviour|
|----|--------|
|Destroy|Shoot to destroy. If combat is won any disabled unitstacks are destroyed by this unit.|
|Scavenge|Shoot to destroy. Tries to destroy the unit (if succesful, the target module will be unstacked, converted to itemstacks and looted by scavenging unit). If combat is won and no unit has destroy tactic any disabled unitstacks are scavenged by this unit.|
|Capture|Shoot to capture. Tries to capture the unit (if succesful, the target module will be unstacked and change the ownership). If combat is won and no unit has scavenge tactic any disabled unitstacks are captured by this unit.|
|Evade|Do not shoot. Tries to move ot from combat. Will escape if not hit for two consectuive combat rounds.|
|Prioritize armed|When selecting the module to be hit armed modules will be given preference.|
|Prioritize command|When selecting the module to be hit command modules will be given preference.|
|Prioritize storage|When selecting the module to be hit storage modules will be given preference.|

**TRANSFER** — `TRANSFER n TO <stack>` | `TRANSFER ALL [DAMAGED] [MODULES] TO <stack|FACTION id>` | `TRANSFER MODULE index TO …`
Stack specific module, damage modules or selected number of modules to unit or faction.

**WITHDRAW** — `WITHDRAW <qty|ALL>` — transfer cash from bank account and place it as cash (positive balance only).

### Long

**JUMP** — `JUMP <gate-id>` — Usable by moveable spaceships only. Alderson hop between paired Alderson points, 1 week, ships only.

**MOVE** — `MOVE <dest> [dest2 …]` — usable by move-capable modules - multi-hop travel; disabled stacks fail; person cannot move on it's own. Destination must be reachable.

`MOVE <dest> [<dest2> …]` — **module stack only** (not a person alone).

One line, multiple hops: `move R00014 R00009 O00003`. Valid destinations: region, orbit, planet, moon, belt, star, anomaly, or alderson point id from report.

**PRODUCE** — `PRODUCE ENERGY` | `PRODUCE <item-id>` — e.g. `@produce cash`, `@produce energy`. Usable in producing modules. Continues use of the modulestack. some modulestack may operate only in specific location types or within specific atmosphere.

**REPAIR** — `REPAIR` — Usable by all modules. weekly repair of the stack. repairs the number of hitpoints equivalent to number of damaged modules. More effcient repair require advanced technologies.

**RESEARCH** — `RESEARCH` | `RESEARCH <target>` | `RESEARCH TECHNOLOGY|ITEM|MODULE|GROUP|REGION| TAG <id>`. Usable in Research modules. Generate Research Points that can trigger random technology breakthrough. Different tags may be used to prioritize specific group of technologies. Some tags require physical presence to be valid. Research regions to investigate anomalies.

**TRAIN** — `TRAIN SKILL <skill>` (person) | `TRAIN <race> OFFICER AS "name"|newN [FOR stack]`. Usable in Research modules. Train a person in new skill or train a anonymous crewmate into skill-capable officer.

**USE** — `USE <tech> [AS newN] [FOR parent]` — Usable in production modules. Build or produce items or modules or effect out of local resources, or stored resources. Places outputs in unit indicated by `FOR`. 

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

### Tactics 


| Order | Effect |
|-------|--------|
| TACTIC destroy | Kill; Post-win routed/disabled units are destroyed with cargo |
| TACTIC scavenge | Kill; Post-win routed/disabled units are converted to loot and transferred to scavenger units|
| TACTIC capture | Seize; Post-win routed/disabled units are converted to new ownership |
| TACTIC evade | Harder to hit; exit after 2 unhit rounds |
| TACTIC prioritize armed\|command\|storage | Target preference |

Immobile units: cannot capture and can only **destroy** only; 
Avoid: is not a battle tactic, but preference in joining combat.

### Battle flow

Battle executes in up to **10 rounds**.

In each round in order of increasing **initiative** modulestack fire against target modulestack. Initiative may be influenced by thurst/mass ratio, skills, technologies, effects and equipped modules.

Drones/figther carried in hangars launch after round 1 roster and fight from round 2. 

The **hit chance** depend on attack value of the shooter and defence value of the defender, modified by skills, technologies, modules, effects.

If hit the **damage** is applied to specific module in the modulestack. The damage may result in defending crew loss.  

When destroy tactic is used shots carry full damage. In capture tactic, the damage splits module damage and capture damage. 

If sum of module damage and capture damage exceeds the module hitpoints the module is wrecked/captured along with portion of the itemstacks held by the entire stack.

Capture damage is cleared between combats.

If battle ends with one side completely disabled the battle is won. Otherwise, after up to **10 rounds**, the battle ends **indecisively** that week; combat may **re-trigger** the following week if enemy stacks remain co-located.

On a clear win an explicit **destroy**/**capture**/**scavenge** tactics decides the fate of **disabled** survivors — default firing alone does not auto-kill routed units.

**Tips:** 
-`declare faction N enemy` before engagement; -`TACTIC destroy` to mop fauna; 
-`TACTIC capture` to grab valuable modules and technology. board officers on combat stacks; 

---

## Common playbooks

### Turn-1 bootstrap on HQ land

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
use twnbld as new1
+get 30 iron from <cargob>
+get 2 titani from <cargob>

#modulestack new1
transfer 1 to faction 1
```

### Buy crew & farm

```
#modulestack <cargob-id>
withdraw 1500
buy 25 terran at 50

#modulestack <farm-id>
get 15 terran from <cargob-id>
@use farmng
```

### Move person

```
#person 200010
active 100
-stack 100

#modulestack 100
move R00014 R00009
```

### activating disabled stacks

**Disabled** = missing crew, energy, fuel, or repairs. Long orders and MOVE fail until fixed.

Factory built units (`use grndtr`, `use armcbt`) start **empty** - have new ID but no modules so cannot be active

```
#modulestack new1
move R00014
+get 6 terran from <hq>
+get 10 oil from <hq>
```

Before `move`: crew, fuel, 
`@repair` if damaged, 
`@produce energy` upstream, 
`get h2o2` for atmospheric hops.

`active newN` then `-get` when unit forms mid-quarter. **`USE farmng`** only on farms; **`USE hcdril`** only on surface drills.

---

*SpaceAge open beta 0.8.001*
