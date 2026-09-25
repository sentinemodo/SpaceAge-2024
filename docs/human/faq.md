# Frequently asked questions

Draft answers for the open PBEM lobby. Edit this file; the website renders it at build time.

## How do I submit orders?

Use the **Game Client** (hosted visual tool): log in with your faction id and the credentials from your invitation, edit orders (on the star map or faction page, click parse to review the output) and press submit.

For full syntax see the **Rules** page on this site (source: `docs/human/rules.md`).

---

## Scout — what to build for scouting initially? why scout at all?



You need to scout to have a better view on neighbouring regions. You need a reliable source of oil to move around, and coal to produce energy efficiently and titani to build weapons and spacecrafts and silici to build research complexes.



Scouts let you investigate the anomalies, find neighbouring factions, United Nations cities and warn you about hostile fauna.



The earliest scout is a truck [trucks]. unarmed, low fuel and usable for carrying good around.

To build truck:

```
#modulstack factory
get 1 iron from <cargo bay stack id>
use grndtr as new1
```

don't forget to top of fuel and food before you move it: 
```
#modulestack new1
move <neighbouring-region-id>
+get 2 terran from <HQ stack id>
+get 4 food from <cargo bay stack id>
+get 2 oil from <cargo bay stack id>
```

Your HQ stack `[corphq]` is a producing command module: under `#modulestack <hq-id>` you can run **`@produce cash`** (corporate treasury) or **`@produce terran`** (recruitment) — one long produce verb at a time, not both in the same week block.

If scouts **`GET`** terrans off HQ, stock can fall below your reserve; then the sim refills terrans first and **`@produce cash` pauses** until **`set hold`** is satisfied again. Typical HQ orders:

```
#modulestack <hq-id>
set hold 20 terran
4 produce terran ; or as many as you need
@produce cash
```

## Getting a bit of advanced stuff 

The easiest way to get some of the level 1 technologies is to grant them to your faction at startup. 

```
GRANT technology msrvtm to <factory-id>
```

at the cost of 1000 credits from your bank account will give you a tech to build a mobile research unit.

---

## Anomaly investigation — what to build for anomaly investigation initially? why investigate it at all?

They easiest way to research technology is to build a computer library [cmplib]. But the more effective one is to investigate anomalies and get a much higher amount of research points out of it.

Build a mobile laboratory `[moblab]` using mobile survey team `[msrvtm]` on the factory.

To investigate the anomaly move to the region and research the anomaly:

```
#modulestack <factory>
use msrvtm as newSurveyTeam

#modulestack newSurveyTeam
move <anomaly-holding-region-id>
+get 2 terran
+get 4 oil
+get 8 food 
+get 8 food
-@research <anomaly-holding-region-id>
```

---
## Grab a land from ative — what to build for land grab initially? why fight the fauna at all?

First killing of natives is promoted by UN. they're usually hostile and threaten the terran reign. So let's be gone with them. UN usally make it profitable but also clearing them bring the benefit on its own.

So what to build? best choice is a squad of tanks.

```
#modulestack <factory>
grant technology armcbt to <factory>
use armcbt as newFaunaCleaningTeam

#modulestack newFaunaCleaningTeam
has 1 tanks
-grant item 16 terran to newFaunaCleaningTeam
-grant item 32 oil to newFaunaCleaningTeam
-grant item 32 food to newFaunaCleaningTeam
-attack <fauna-holding-region-id>

```

---
## Is this all we can see on the surface? 

not really researching or granting a core drill technology [mcored] will let you see a deep resource pocket (maybe that so much needed oil and titani)

once you get it load up your truck with resoruces and head toward region where you spot the resoource pockets and drill them with use of `**iminng`** / `**hcdril**` / etc. on the drill 

---

## How do I build whatever? (Iron, trucks, factory chain)

This is the question most new players stall on. The chain is always **materials → factory module → crew/fuel/upkeep reserve → MOVE**.

### 1. Get **iron** (and friends)

Iron is item type `**iron`**. Common sources:

- `**iminng` (iron mining)** — run on a **surface drill `[sdrill]`** (or core drill) sitting on **solid ground** in a region that actually has iron in the report. One week → **3 iron** per successful use.
- `**@buy all iron`** (or priced buys) — if the regional market lists it; costs **cash** from bank/`withdraw`.
- `**GET`** from your `**cargob**` — if you already stockpiled iron there (turn‑1 grants often include some).
- `**GRANT`** from your United Nations reserves — paid in cash (or rather directly from bank account).
- **Do not** put `**use hcdril`** on the **factory** — hydrocarbons drilling runs on a **drill**, not on `[factry]`. 

### 2. Build **trucks** (ground transport)

Technology `**grndtr`** (ground transport), used on a **factory `[factry]`**:

- **Needs:** 2 **iron**
- **Builds:** `**trucks`** module (the vehicle stack)
- **Time:** 2 weeks

Order shape (ids from **your** report):

```text
#modulestack <factry-id>
use grndtr as new1 for <hq-id>
+get 2 iron from <cargob-id>
```

The `**+get**` lines **stage iron before** the long `**USE`** finishes — same idea as town builds in the **Common playbooks** section of Player Rules. Factory `**USE`** also pulls from shared cargo on the grant if bays are sharing (see `**USE**` consume rules there).

`**use grndtr as new1 for <hq-id>**` nests the new truck under headquarters (campaign pattern). Always name outputs `**new1**`, `**new2**`, … — not `scout1`.

### 3. Activate the truck (easy to forget)

Factory-built `**trucks**` start **disabled**: they exist but have **no crew** (and no fuel). `**@move` fails** until fixed.

On the `**#modulestack new1`** block (same `new1` as in the `USE` line):

```text
#modulestack new1
@get 1 terran from <hq-or-cargob>
@get 1 oil from <cargob>
@move R00009
```

- **Crew** — usually `**terran`** (buy at market or `**GET**` from HQ or `**GRANT**`).
- **Fuel** — `**oil`** for ground vehicles (catalog: 1 oil per 13 weeks of ground movement for trucks).
- **Air/food** — terrans consume `**terair`** off-world and `**food**` upkeep every quarter; take extra on long trips (rules warn the sim is unforgiving).

## How do I get more terrans or cash or energy

Your **Headquarters office** can produce cash or terrans
`**@PRODUCE cash**`
`**@PRODUCE terran**`

**Branch office** and similar are separate mid-game patterns (local cash/crew, not a full HQ clone).

Your powerplants produce energy
`**@PRODUCE energy**`

## Do I need a truck to reach the neighboring region?

**Yes**, if you want to move there to scout or to move **equipment** (drill, lab, tanks) into that region.

- `**MOVE`** follows **exit** lines in the report (orthogonal hops; duration from the exit, often 1 week on land).
- **People** can `**STACK`** onto a vehicle or shuttle; the **vehicle** (or shuttle for orbit) does the `**MOVE`**.
- A **shuttle** is for **surface ↔ orbit** and orbital work — not for replacing trucks on the regional grid.

---

## How do I expand production into the neighboring region?

1. **Recon** — move a truck (or tank) along exits; read the new region’s **resources** and terrain in the report.
2. **Deploy extraction** — `**MOVE`** a `**sdrill**` (or build one in-place if you already have factory + iron there) into the region.
3. **Run the right tech on the drill** — e.g. `**@use iminng`** for iron, `**@use hcdril**` for carbon, `**@use oildwe**` for oil — only if the region supports that tech (`operation-allowed-in` in `docs/human/basic_technologies.md`).
4. **Haul output home** — `**GET`** iron (etc.) **onto the truck** in that region, `**MOVE`** back to your grant, `**GET**` from truck into `**cargob**`. Same region only for each `**GET**`; multi-hop = move the carrier.

**Settlements** (town/city) and many **production** modules are tied to **terran atmosphere** solid ground — you cannot drop a full industrial clone on every biome.

---

## How do I transport loot to the “warehouse”?

1. **Same region:** `**GET 30 iron FROM <truck-id>`** on the `**cargob**` (or `**GIVE**` from truck to bay).
2. **Different region:** truck (or other carrier) must `**MOVE`** with the items **on board** (nested cargo or itemstacks on the vehicle stack), then unload with `**GET`** when both stacks share the region.
3. **Allied handoff:** `**GIVE`** to a friendly stack in the same region (attitude rules apply).

There is no automatic “teleport to HQ”. Plan round-trip **weeks**, **fuel**, and **terair** for ground hauls.

After combat, `**TACTIC scavenge`** and capture flows move loot according to battle rules — still via normal stack locations and `**GET**`, not a special warehouse building.
