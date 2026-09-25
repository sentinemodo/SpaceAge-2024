# Frequently asked questions

Draft answers for the open PBEM lobby. Edit this file; the website renders it at build time.

## How do I submit orders?

Use the **Game Client** (hosted visual tool): log in with your faction id and the credentials from your invitation, edit orders (on the star map or faction page, click parse to review the output) and press submit.

For full syntax see the **Rules** page on this site (source: `docs/human/rules.md`).

---

## Scout — what to build for scouting initially? why scout at all?



You need to scout to have a better view on neighbouring regions. You need a reliable source of oil to move around, and coal to produce energy efficiently and titani to build weapons and spacecrafts and silici to build research complexes.



Scouts let you investigate the anomalies, find neighbouring factions, United Nations cities and warn you about hostile fauna.



The earliest scout is a truck. unarmed, low fuel and usable for carrying good around.

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
+get 2 terran from <cargo bay stack id>
+get 4 food from <cargo bay stack id>
+get 2 oil from <cargo bay stack id>
```


| Goal                           | What you are really building                                                                                  | Why                                                                                                                                                                       |
| ------------------------------ | ------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **See the next region over**   | Ground **vehicle** (`trucks` module) with crew + **oil** fuel                                                 | Only mobile **modulestacks** (or people stacked on them) can `MOVE` along region **exits**. HQs and drills do not walk.                                                   |
| **Investigate an anomaly**     | `**moblab`** (mobile laboratory) on a truck, plus time on `RESEARCH <region-id>`                              | Anomalies are resolved on-site after you **move onto the cell**. Typical loop: detect hint in report → `MOVE` → `RESEARCH`. See `play/designer/anomaly-investigation.md`. |
| **Fight or grab land**         | `**grndtr`** on a **factory** → `**trucks`** or `**armcbt**` → tank squads                                    | Combat stacks are separate military modules; they still need crew, fuel, and often a slow factory pipeline.                                                               |
| **Mine the neighbor’s ground** | `**sdrill`** (surface drill) **moved** into that region, then `**iminng`** / `**hcdril**` / etc. on the drill | Extraction techs run on an **extraction** module **in that region**. Your home grant does not auto-mine distant cells.                                                    |


Early campaign personas differ: some HQs start with `**sdrill`** and hydrocarbons tech; researchers may get `**moblib**` on the factory for a faster `**moblab**` path. Read **your** turn‑1 report (modules, tech copies, region exits) before copying someone else’s build order.

**Warehouse:** the game has no module named “warehouse”. Treat your HQ `**cargob`** (small cargo bay) as the central stash in that region — `**GET**` / `**GIVE**` move items between your stacks **in the same region** only.

---

## How do I build a scout? (Iron, trucks, factory chain)

This is the question most new players stall on. The chain is always **materials → module → crew/fuel → MOVE**.

### 1. Get **iron** (and friends)

Iron is item type `**iron`**. Common sources:

- `**iminng` (iron mining)** — run on a **surface drill `[sdrill]`** (or core drill) sitting on **solid ground** in a region that actually has iron in the report. One week → **3 iron** per successful use.
- `**@buy all iron`** (or priced buys) — if the regional market lists it; costs **cash** from bank/`withdraw`.
- `**GET`** from your `**cargob**` — if you already stockpiled iron there (turn‑1 grants often include some).
- **Do not** put `**use hcdril`** on the **factory** — hydrocarbons drilling runs on a **drill**, not on `[factry]`. Sample game turn 1 uses `@use hcdril` under the **core drill** stack (`000016` in the Caste Prime example).

Your HQ `**@produce cash`** (and `**@produce energy**` on plants) pays for buys and upkeep while you bootstrap.

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

- **Crew** — usually `**terran`** (buy at market or `**GET**` from HQ).
- **Fuel** — `**oil`** for ground vehicles (catalog: 1 oil per 13 weeks of ground movement for trucks).
- **Air/food** — trucks consume `**terair`** and `**food**` upkeep off-world; take extra on long trips (rules warn the sim is unforgiving).

See **“activating disabled stacks”** in Player Rules.

### 4. Optional — **mobile lab scout** (anomalies)

After `**moblib`** is researched/copied onto the factory:

- `**use moblib as new2 for <hq-id>**` (or nest on truck — follow your report’s nesting rules)
- Same **crew + oil** story as trucks
- On the anomaly region: `**RESEARCH Rxxxxx`** each week until investigation completes

Researcher personas may start with `**moblib**` already on the factory; others research it normally (L1, RP cost in catalog).

---

## Do I need a truck to reach the neighboring region?

**Yes**, if you want to move **equipment** (drill, lab, tanks) into that region.

- `**MOVE`** follows **exit** lines in the report (orthogonal hops; duration from the exit, often 1 week on land).
- **People** can `**STACK`** onto a vehicle or shuttle; the **vehicle** (or shuttle for orbit) does the `**MOVE`**.
- A **shuttle** is for **surface ↔ orbit** and orbital work — not for replacing trucks on the regional grid.

Turn‑1 Sample Game orders defer ground `**MOVE`** until shuttles exist because that scenario is building **orbital** industry first — that is not the same as “neighbors never need trucks.”

---

## How do I expand production into the neighboring region?

1. **Recon** — move a truck (or tank) along exits; read the new region’s **resources** and terrain in the report.
2. **Deploy extraction** — `**MOVE`** a `**sdrill**` (or build one in-place if you already have factory + iron there) into the region.
3. **Run the right tech on the drill** — e.g. `**@use iminng`** for iron, `**@use hcdril**` for carbon, `**@use oildwe**` for oil — only if the region supports that tech (`operation-allowed-in` in `docs/human/basic_technologies.md`).
4. **Haul output home** — `**GET`** iron (etc.) **onto the truck** in that region, `**MOVE`** back to your grant, `**GET**` from truck into `**cargob**`. Same region only for each `**GET**`; multi-hop = move the carrier.

**Settlements** (town/city) and many **production** modules are tied to **terran atmosphere** solid ground — you cannot drop a full industrial clone on every biome.

**Branch office** and similar are separate mid-game patterns (local cash/crew, not a full HQ clone).

---

## How do I transport loot to the “warehouse”?

1. **Same region:** `**GET 30 iron FROM <truck-id>`** on the `**cargob**` (or `**GIVE**` from truck to bay).
2. **Different region:** truck (or other carrier) must `**MOVE`** with the items **on board** (nested cargo or itemstacks on the vehicle stack), then unload with `**GET`** when both stacks share the region.
3. **Allied handoff:** `**GIVE`** to a friendly stack in the same region (attitude rules apply).

There is no automatic “teleport to HQ”. Plan round-trip **weeks**, **fuel**, and **terair** for ground hauls.

After combat, `**TACTIC scavenge`** and capture flows move loot according to battle rules — still via normal stack locations and `**GET**`, not a special warehouse building.

---

## I still do not see what builds what — where is the map?

Use three layers together:

1. `**docs/human/basic_technologies.md**` — every `**use …**` tech: **where** it runs, **inputs**, **output module or items**, **time**.
2. **Player Rules → Common playbooks** — minimal turn‑1 loops (HQ cash, drill + hydrocarbons, factory town, disabled `**new1`** activation).
3. **Sample game** — real order files under **Sample game** on this site; cross-check ids against the matching **report** in the same turn.

Known gap (called out by players): Sample Game examples are **dense** and not every line is commented. Prefer the **playbooks** in Rules for narrated patterns, then open Sample Game to see a full faction turn. When you learn a working scout chain, annotate your own order file with `;` comments — the engine ignores them.

**Quick reference — scout bootstrap minimum:**

```text
HQ:     @produce cash
cargob: withdraw / buy iron if needed
factry: use grndtr as new1 for <hq>  +  +get 2 iron from <cargob>
new1:   @get crew + @get oil  →  @move <neighbor-region-id>
```

Replace ids from the report template block at the bottom of your faction report.