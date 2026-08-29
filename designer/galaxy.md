# Galaxy — 10 players, two starting systems

Ten corporations (factions `2`–`11`) plus NPC faction `1` (**United Star Nations**) and two planet-local militias (faction `12` **Arbor First**, faction `13` **HCS**). Seed is **not** one home system per player.

**Turn 1 occupancy:** two star systems only. Five players on starting system A, five on starting system B. Each player owns **one exclusive region** on that system’s habitable planet. The other **eight systems have no player HQ and no NPC cities**. They are exploration destinations.

Helios and Fomal are a **wide bound pair** (~200 AU), not light-years apart. No FTL. Inner-system hops are weeks; the pair hop is a season or more on chemical/fission stages; empty systems are farther still.

## Scale

| Object | Count | Notes |
|--------|-------|--------|
| Player factions | 10 (`name` 2–11) | Faction `1` = United Star Nations (cities, markets, later patrons) |
| Militia NPCs | 2 (`name` 12–13) | **Arbor First** (Arbor), **HCS** (Anvil). Neutral at t=1; flip hostile on first spaceship |
| Star systems | 10 | **2 occupied at t=1**, **8 empty** (no HQ, no UN `city`) |
| Alderson pair | 1 | Helios Gate ↔ Fomal Gate (`<alderson>`, `pair=`). Empty systems have **no** AP at t=1 |
| Planets + belts / system | 1–4 | Mix of `ocean`, `dust`, `gasgnt`, `abelt` |
| Initially habitable / system | 0–1 | `terair` + food + liquid water + settlement capacity on grassland/ocean |
| Initially exploitable / system | 0–2 | Ores/volatiles without a breathable mix; drills work, farms do not |
| Moons / planet | 0–4 | Gas giants carry most moons; belts have 0 |
| Regions / planet or moon | 10–50 | See size table |

**Habitable** means a human can run `city` / `farms` without imported air and food. **Exploitable** means extraction techs have a regional resource they produce.

Leave empty regions. A ~36-region ocean world with eight occupied cells still has a hinterland.

**XML star `type`:** live catalog currently has only `M4`. Emit `type="M4"` until `campaign/data.xml` adds more star entries. Spectral class in `name-en` / description is flavour.

## Seed vs exploration (retired layout)

The previous seed was **one player per system** (four habitable starts, six vacuum/ISRU starts). That table is **retired as the t=1 seed**. The eight empty systems below **reuse those briefs as exploration destinations** (including virgin habitable worlds with no cities).

## Turn-1 occupancy

| Role | System | Planet | Type | AU | `surface-size` | Regions | Who |
|------|--------|--------|------|----|----------------|---------|-----|
| Start A | **SS0001 Helios** | **P00001 Arbor** | `ocean` | 1.0 | 6×6 | 36 (`R00001`–`R00036`) | Factions **2–6**; UN capital + 2 towns |
| Start B | **SS0002 Fomal** | **P00005 Anvil** | `ocean` | 1.4 | 7×5 | 35 (`R00037`–`R00071`) | Factions **7–11**; 3 UN towns |

Both worlds are **initially habitable** (grassland/ocean, `terair`, food, water, settlement capacity). Both are **at least medium**: empty hinterland for uncontested growth. Neither planet holds the full industrial diet.

## Resource split (bootstrap diet)

Canonical ids, rarities, quantities, and empty-system deposits: **[resources.md](resources.md)**. Arbor/Anvil split is unchanged: Arbor has no surface `titani`/`copper`/`uraniu`; Anvil has no `oil` and poor `food`. Higher-level ores (`nickfe` `tungst` `deutrm` `ammoni` `methn` `volatl` `kerogn` `alumin` `platnm` `gold`) follow that file — none of them put forbidden metals on Arbor’s grid or petroleum on Anvil’s crust.

Corporations bootstrap on local organics **or** local metals, then **trade, contract, or fly** for the rest. Complementary pockets sit on **other planets/belts in the same system**. Do not stack the same `type` twice on one region.

## NPC polity — United Star Nations (faction `1`)

Trade-friendly charterer, not a conquering empire. Issues later `give-module` contracts. Faction `name-en`: **United Star Nations**.

| City | Planet | Region | `city` qty | Nested (order of) | Why |
|------|--------|--------|------------|-------------------|-----|
| **Assembly** (capital) | Arbor | `R00015` (2,2) grassland | **6** | farms 24, `cplant` 10, `wnplnt` 5, `inftry` 4, granary `cargob` 2 (food ~4000), cash ~8000, terran ~80 | Calories and coal can actually feed a metro; diplomatic seat sits on the food-export world |
| **Tidewatch** | Arbor | `R00003` (2,0) grassland | 2 | farms 6, `cplant` 2, `inftry` 1, granary 1 | Coastal UN town; oil-adjacent |
| **Windgap** | Arbor | `R00028` (3,4) grassland | 1 | farms 4, `wnplnt` 6, `inftry` 1, granary 1 | Interior market |
| **Slagport** | Anvil | `R00040` (3,0) grassland | 2 | farms 6, `wnplnt` 10, `inftry` 1, granary 1 (food tight) | Hungry metal-export town; **buys food at a premium** |
| **Ridge** | Anvil | `R00057` (6,2) grassland | 1 | farms 4, `wnplnt` 6, `inftry` 1 | Eastern concession |
| **Isotope** | Anvil | `R00068` (3,4) grassland | 1 | farms 4, `wnplnt` 4, `inftry` 1 | Near uraninite mountains |

NPC cities occupy **their own regions**. Players never start nested inside a UN `city`.

## NPC militias — Arbor First and HCS

Planet-local anti-starflight polities. They exist **at t=1** as neutral `city` stacks (not spawned on flip). They have **no shuttles, hulls, or space exits in their cargo**. They never leave their home planet.

| Fac | `name-en` | Planet | Region | City stack | Nested (order of) | Why that cell |
|-----|-----------|--------|--------|------------|-------------------|---------------|
| **12** | Arbor First | Arbor | `R00014` (1,2) grassland **Farm Belt** | `120001` **Rootfast** `city` qty **3** | farms 16, `cplant` 2, `inftry` **3**, granary `cargob` 1 (food ~800, terair, water), cash ~4000, terran ~40. Nested idle raid `120010` `inftry` 3 (stays stacked under city until flip) | Inland agricultural heartland, away from Cinder Flats |
| **13** | Human Conservation Society (HCS) | Anvil | `R00060` (2,3) grassland **Vale** | `130001` **Crusthold** `city` qty **2** | `cdrill` 4, `factry` 2, `wnplnt` 4, `inftry` 2, `tanks` 1, granary 1 (iron/titani/silici, food tight ~120), cash ~5000, terran ~30. Nested idle raid `130010` `inftry` 3 | Inland vale next to the uraninite spine, away from Pad |

Faction XML: `password=""` (GM/NPC), `default-attitude="2"` (neutral), `text-report="True"`, bank as cash above. Stack id blocks: Arbor First `120001`–`120099`, HCS `130001`–`130099`.

### Hostility flip (wishlist until TDD)

**Trigger (per planet, once):** a stack whose location is a **region of that planet** completes production (`USE`/`PRODUCE` finishing) of:

- module type `shuttl`, **or**
- any module with group `frigate` or `spacecraft`.

Arbor → flip faction **12**. Anvil → flip faction **13**. The other militia is unchanged. Building a shuttle on Scoria/Pyre/orbit does **not** flip.

**On flip:**

1. Militia `DECLARE DEFAULT ENEMY` (and `DECLARE FACTION 1 ENEMY` so UN is not exempt).
2. UN issues the capture charter in [`contracts.md`](contracts.md) (live stand-in: `give-module` war-supply until `capture-stack` exists).
3. **Raids start** (below).

Until the Events pipeline lands, the GM/script writes faction 12/13 `order.*` files for the flip turn.

### Raids (ground only)

| Rule | Value |
|------|--------|
| Size | **3** combat modules (small raid in [`combat-balance.md`](combat-balance.md)): the nested `inftry` 3 stack |
| Cadence | **1 raid per game year** (turns 4, 8, 12, …) after flip, until the militia `city` is captured or destroyed |
| Path | `STACK OUT` from city → `MOVE` toward a **random player HQ on that planet** (not UN first). `ATTACK` / `CAPTURE` if they share a region |
| Limits | No ships, no AP, no other planet. If the raid stack dies, the next yearly raid forms a new `inftry` 3 from city garrison if `inftry` remain; otherwise raids stop |
| After city captured | Raids stop. Remnant hinterland `inftry` 1 optional, no new yearly spawn |

Neutral t=1: raid stacks stay nested; no `ATTACK`. Players may trade with or ignore them until someone builds a shuttle.

### UN markets (t=1)

Canonical prices and quantities: [`economy.md`](economy.md). Standing XML offers on the `city` stack (engine `GetPrice` is 0 until a trade; auto-list would skip).

**Assembly** (food-export capital):

- `buying item="food" quantity="500" price="1"`
- `buying item="carbon" quantity="80" price="2"`
- `buying module="farms" quantity="2" price="100"`
- `selling item="food" quantity="120" price="4"`
- `selling item="iron" quantity="30" price="3"`
- `selling item="silici" quantity="15" price="4"`
- `selling item="titani" quantity="10" price="6"`
- `selling item="copper" quantity="8" price="6"`
- `selling item="terair" quantity="50" price="1"`
- `selling item="terran" quantity="50" price="50"`

**Slagport** (hungry metal town): `buying item="food" quantity="200" price="2"` (premium vs Assembly); buy local `iron`/`titani`/`copper`/`silici` at 1–2; `selling` `titani` 25 @ 4, `copper` 25 @ 4, `uraniu` 10 @ 8, `food` 40 @ 6, `terran` 12 @ 50.

Tidewatch sells `oil`; Isotope sells `uraniu`; Windgap/Ridge are small food books. Stock granary `itemstack`s to cover the sell quantities.

## Faction → region (players 2–11)

Each player region: top-level `corphq` (not inside a UN city) + nested modest `cargob` / energy / `cdrill` / `factry` / `farms`. **Cargo is not a complete diet.**

| Fac | `name-en` (placeholder) | Planet | Region | XY | Terrain | HQ cargo (have) | HQ cargo (omit) | Energy |
|-----|-------------------------|--------|--------|----|---------|-----------------|-----------------|--------|
| 2 | Northwind | Arbor | `R00008` | 1,1 | `grassl` | food, terair, h2o2, iron, carbon | titani, copper, uraniu | `cplant` 2 |
| 3 | Greenwell | Arbor | `R00010` | 3,1 | `grassl` | same pattern | same | `cplant` 2 |
| 4 | Rivermark | Arbor | `R00017` | 4,2 | `grassl` | same pattern | same | `cplant` 2 |
| 5 | Sundock | Arbor | `R00022` | 3,3 | `grassl` | same pattern | same | `cplant` 2 |
| 6 | Copse | Arbor | `R00027` | 2,4 | `grassl` | same pattern | same | `cplant` 2 |
| 7 | Ironclad | Anvil | `R00045` | 1,1 | `grassl` | terair, modest food+water, titani, silici, copper, uraniu, some iron | carbon, oil, bulk food | `wnplnt` 8 |
| 8 | Oreline | Anvil | `R00049` | 5,1 | `grassl` | same pattern | same | `wnplnt` 8 |
| 9 | Basalt | Anvil | `R00054` | 3,2 | `grassl` | same pattern | same | `wnplnt` 8 |
| 10 | Silicate | Anvil | `R00059` | 1,3 | `grassl` | same pattern | same | `wnplnt` 8 |
| 11 | Fission | Anvil | `R00063` | 5,3 | `grassl` | same pattern | same | `wnplnt` 8 |

Shared HQ nest (all ten): `corphq` 1 + CEO `terran` officer, `cargob` 2, `cdrill` 1, `factry` 2, crew **30**. Arbor `farms` **3**; Anvil `farms` **2**. Faction `balance` 10000. Leftover `@produce cash` on every HQ (`campaign/data.xml` **50**/week — [`economy.md`](economy.md)). Settlement `capacity` 8–12 on player grassland.

Arbor HQ cargo quantities (order of): food 400, terair 200, h2o2 200, iron 40, carbon 40, silici 10.  
Anvil HQ cargo: food 80, terair 200, h2o2 80, iron 15, titani 40, silici 40, copper 30, uraniu 20.

## Region count by size

| Body | `surface-size` | Regions | Typical types |
|------|----------------|---------|----------------|
| Tiny moon / ring shepherd | 5×2 | 10 | `barren` `dust` `mountn` |
| Small moon | 5×3 or 4×4 | 12–16 | `dust` `barren` `mountn` (ice moons: same types + `h2o2`/`water`; **no** region type `ice`) |
| Large moon / small dusty planet | 5×4 or 6×4 | 20–24 | mix + 1–2 `mountn` |
| Earthlike / starting worlds | 6×6 to 7×6 | 30–42 | `grassl` `ocean` `sea` `mountn` `dust` |
| Large dusty world | 7×6 to 8×6 | 42–48 | `dust` `barren` `mountn` |
| Asteroid belt | 8×4 to 8×5 | 24–40 | `smmast` `smcast` `lrmast` `lrcast` |
| Gas giant | — | 0 surface | one `orbit` only; regions live on moons |

Grid: unique `(X,Y)` on that body. 4-neighbour `exit` pairs, both directions, plus east–west wrap (no north–south wrap). Ground `duration`: grassland/dust 2–3, barren 3, mountain 4–6. Sea/ocean (and any exit that touches a `sea`/`ocean` cell) use `exitmode mode="naval"` with those water durations. Landlocked cells stay `ground`. Ships can occupy the adjacent land tile as a port; they cannot walk inland. There is no separate coastal region type.

The 6×6 / 7×5 starting grids are the **playable continent** (settled land + coastal seas), not a 1:1 globe. Planet `type="ocean"` still means a water world; most pelagic area is off-map.

Settlement `capacity` only on habitable or planned colony sites (4 tiny, 8 town, 12–16 metro basin). Extraction worlds: `extraction` capacity 2–6, no `settlement` until habitats exist.

Live region types only: `orbit` `ocean` `sea` `grassl` `dust` `mountn` `barren` `smmast` `smcast` `lrmast` `lrcast`.

## Space travel times (until drive speed is wired)

Orbit-to-orbit and planet-orbit exits use `exitmode mode="space" duration="N"` weeks.

**Loader today:** `LoadExits` walks planet regions, moon regions, belts, and `<alderson>`. A region may `exit` to another `region` or a `belt`. Same-body surface↔orbit is implicit (1 week) — do not emit `exit orbit=` or `exit alderson=`. Moon maps can wait for the XML pass.

| Hop | Chemical / fission (L0–2) | L10 fusion ark (design target) |
|-----|---------------------------|--------------------------------|
| Surface ↔ local orbit | 0–1 | 0 |
| Planet ↔ its moon | 1–2 | 1 |
| Inner system (0.5–2 AU) | 8–13 | 2–4 |
| Helios ↔ Fomal (~200 AU pair) | 26 | 4–8 |
| To gas giant (5 AU) | 13+ | 4–8 |
| Outer belt (20–40 AU) | many turns | 8–13 |
| Occupied pair → empty system | 52+ | 8–13 |
| Planet → local Gate (80 AU) | **13** (AU×drive, wishlist) | 2–4 |
| Helios Gate ↔ Fomal Gate | **1** (`JUMP`) | 1 |

Until AU×drive is wired, put chemical durations on **region↔region** and **region↔belt** exits only. Do not bake hops from planetary regions to Gates.

**Inter-homeworld:** the **Alderson pair** is the intended crossing (1 week `JUMP` at the Gate). The 26-week Cinder Flats ↔ Pad hop is the **chemical long way** (no Gate). Keep both.

**t=1 baked chemical hops:**

| From | To | Duration |
|------|----|----------|
| Arbor `R00006` Cinder Flats (5,0) `barren` | Scoria landing `R00072` | 8 |
| Arbor `R00006` | Helios Belt `P00003` | 13 |
| Arbor `R00006` | Anvil `R00039` Pad (2,0) `dust` | 26 |
| Anvil `R00039` | Pyre landing (first Pyre region) | 8 |
| Anvil `R00039` | Fomal Belt `P00007` | 13 |
| Helios Gate `P00009` | Fomal Gate `P00010` | **1** (`JUMP`, not a MOVE exit) |
| Reverse of each | — | same |

## Id allocation

| Kind | Pattern | Range (10-player seed) |
|------|---------|-------------------------|
| System | `SS0001`–`SS0010` | 10 |
| Star | `S00001`–`S00012` | extras if a visual binary |
| Planet/belt | `P00001`–`P0030` | Helios/Fomal bodies `P00001`–`P00008`; **APs `P00009`–`P00010`**; empty systems `P00011`+ |
| Moon | `M00001`–`M0080` | unique ids even if loader currently copies planet id |
| Orbit | `O00001`–`O0120` | one per planet/moon; Gate orbits `O00110` Helios, `O00111` Fomal |
| Region | `R00001`–`R1500` | Arbor `R00001`–`R00036`, Anvil `R00037`–`R00071`, satellites next. Gates have **no** corona region |
| Contract | `CTnnnn` | 6 chars |
| Wreckage stacks | `W00001`–`W0020` | faction 1; **not** on Arbor/Anvil grids at t=1 |
| UN city trees | `100001`+ | avoid SampleGame `000001`–`000030` |
| Arbor First trees | `120001`–`120099` | Rootfast + raid |
| HCS trees | `130001`–`130099` | Crusthold + raid |
| Player HQ trees | `200001`+ (fac 2), `210001`+ (fac 3), … `290001`+ (fac 11) | |
| Officers | same numeric space as HQ | |

## Occupied systems — bodies

### SS0001 Helios (start A) — G2 flavour, XML star `M4`

`X="0" Y="0" Z="0"`. Star `S00001` Helios.

| Id | Name | Type | AU | Size | Hab/exp | Notes |
|----|------|------|----|------|---------|-------|
| `P00001` | Arbor | `ocean` | 1.0 | 6×6 | **hab** | Player + UN start; organics/iron/carbon; no titani/uraniu/copper |
| `P00002` | Scoria | `dust` | 1.5 | 6×4 (24) | **exp** | Ilmenite/copper plains; no biosphere |
| `P00003` | Helios Belt | `<belt>` | 2.7 | — | **exp** | Composition (uraniu/nickfe/iron/carbon); no regions; wreck `W00001` |
| `P00004` | Aeolus | `gasgnt` | 5.2 | — | — | Orbit only |
| `P00009` | Helios Gate | `<alderson>` | 80 | — | — | Pair of Fomal Gate. Orbit `O00110`, no regions |

Moons: Arbor `M00001` **Selene** `rock` 5×3 (titani, silici, **polar `water` ice 40–80 on ≥2 regions**). Aeolus: 4 moons (ice, ice, rock, vulcan) — ice moons **rich `water`** + `heliu3` / later `tungst`. Moon region exits: wishlist.

**Environment attrs (see [`environments.md`](environments.md)):** Arbor `gravity=normal` `temperature=habitable` `atmosphere=terair`, plus `<race type="terran"/>` on the planet (not on orbit). Selene `gravity=low` `temperature=cold` `atmosphere=none`. Scoria/Pyre `gravity=low` `temperature=hot` `atmosphere=thin`. Aeolus has a ring belt `P00091`.

**Arbor flavour:** ~1 bar N2/O2. Grasslands fix carbon; banded iron in old basins; peat and coastal oil. Crust is sediment and granite — little rutile or pitchblende at the surface.

### SS0002 Fomal (start B) — F5 flavour, XML star `M4`

`X="1" Y="0" Z="0"` (pair axis). Star `S00002` Fomal.

| Id | Name | Type | AU | Size | Hab/exp | Notes |
|----|------|------|----|------|---------|-------|
| `P00005` | Anvil | `ocean` | 1.4 | 7×5 | **hab** | Player + UN towns; metals/fissiles; poor food/oil |
| `P00006` | Pyre | `dust` | 0.6 | 5×4 (20) | **exp** | Hot iron/silica; not habitable |
| `P00007` | Fomal Belt | `<belt>` | 2.5 | — | **exp** | Composition (carbon/oil); no regions; wreck `W00002` |
| `P00008` | Fomal giant | `gasgnt` | 6.0 | — | — | Orbit only |
| `P00010` | Fomal Gate | `<alderson>` | 80 | — | — | Pair of Helios Gate. Orbit `O00111`, no regions |

Moons: Anvil 2 (`rock`, `ice` 5×3 / 5×2) — rock moon: metals + **`water` ice 30–60** on ≥2 regions; ice moon: **rich `water`**. Giant: 2 ice moons (`water`, `heliu3`, `ammoni`).

**Environment attrs:** Anvil `gravity=normal` `temperature=habitable` `atmosphere=terair`, plus `<race type="terran"/>` on the planet (not on orbit). Moons `gravity=low` `temperature=cold` `atmosphere=none`. Fomal Giant has a ring belt `P00092`.

**Anvil flavour:** Breathable mix over a younger, thinner biosphere. Shield volcanoes expose ilmenite, native copper, uraninite veins. Soils are mineral; wetlands scarce; no commercial petroleum. Seas exist but ice and aquifers are modest.

## Alderson Points (homeworld pair)

First-class `<alderson>` (not a planet). Catalog type `adpnt` remains unused in gamein. Each Gate has **one orbit and no regions**. No solid surface, no settlement capacity, no resources. Environment: `temperature="cold"`, `atmosphere="none"`. Spaceships occupy the orbit.

| Id | Name | System | AU | Orbit | Pair |
|----|------|--------|----|-------|------|
| `P00009` | Helios Gate | SS0001 | 80 | `O00110` | `P00010` |
| `P00010` | Fomal Gate | SS0002 | 80 | `O00111` | `P00009` |

**Play loop:** Reach a Gate orbit via AU×drive (wishlist — not a region exit), then `JUMP P00010` while at Helios Gate (1 week, ships only: `frigate` / `spacecraft` / `shuttl`; no `city`/`inftry` top-level). Reverse the same. Do not emit region↔Gate MOVE exits.

Empty systems: **no** AP at t=1. Do not add Cinder↔Shards or other pairs until a later XML pass.

**XML sketch:**

```
<alderson name="P00009" name-en="Helios Gate" AU="80" pair="P00010" temperature="cold" atmosphere="none">
  <orbit name="O00110"/>
</alderson>
```

## Arbor region grid (P00001, 6×6)

Index: `R00001` + `Y*6+X`. 4-neighbour ground exits. Occupied cells in **bold**.

| Id | XY | Type | Name-en | Occupant | Resources (qty order of) |
|----|----|------|---------|----------|---------------------------|
| R00001 | 0,0 | ocean | West Pelagic | empty | terair 100, water 600, food 40 |
| R00002 | 1,0 | sea | Shelf | empty | terair 100, water 400, food 80, h2o2 100 |
| **R00003** | 2,0 | grassl | Tidewatch Coast | **UN Tidewatch** | terair 100, food 500, oil 30, iron 15, water 150 |
| R00004 | 3,0 | grassl | South Vale | empty | terair 100, food 450, carbon 25, iron 20, water 120 |
| R00005 | 4,0 | dust | Launch Steppe | empty | iron 30, silici 25, carbon 10 |
| R00006 | 5,0 | barren | Cinder Flats | empty | iron 20, silici 15 |
| R00007 | 0,1 | ocean | West Deep | empty | terair 100, water 600, food 40 |
| **R00008** | 1,1 | grassl | Northwind Grant | **fac 2 HQ** | terair 100, food 600, carbon 30, iron 20, water 150 |
| R00009 | 2,1 | grassl | Mid Vale | empty | terair 100, food 500, carbon 20, iron 15, water 140 |
| **R00010** | 3,1 | grassl | Greenwell Grant | **fac 3 HQ** | terair 100, food 600, carbon 30, iron 20, water 150 |
| R00011 | 4,1 | mountn | South Ridge | empty | iron 70, silici 20, carbon 5 |
| R00012 | 5,1 | dust | East Dune | empty | iron 30, silici 25, carbon 10 |
| R00013 | 0,2 | sea | West Coast | empty | terair 100, water 400, food 80, h2o2 80 |
| R00014 | 1,2 | grassl | Farm Belt | **Arbor First Rootfast** | terair 100, food 700, carbon 35, iron 20, water 160 ; settlement 8 |
| **R00015** | 2,2 | grassl | Assembly Basin | **UN Assembly** | terair 100, food 800, carbon 40, iron 25, water 180 ; settlement 16 |
| R00016 | 3,2 | grassl | Central Basin | empty | terair 100, food 550, carbon 25, iron 20, water 150 |
| **R00017** | 4,2 | grassl | Rivermark Grant | **fac 4 HQ** | terair 100, food 600, carbon 30, iron 20, water 150 |
| R00018 | 5,2 | mountn | East Peak | empty | iron 80, silici 25 |
| R00019 | 0,3 | ocean | Mid Pelagic | empty | terair 100, water 600, food 30 |
| R00020 | 1,3 | ocean | Inner Pelagic | empty | terair 100, water 600, food 30 |
| R00021 | 2,3 | grassl | Prairie | empty | terair 100, food 500, carbon 20, iron 15, water 140 |
| **R00022** | 3,3 | grassl | Sundock Grant | **fac 5 HQ** | terair 100, food 600, oil 20, iron 20, water 150 |
| R00023 | 4,3 | grassl | East Steppe | empty | terair 100, food 480, carbon 20, iron 18, water 130 |
| R00024 | 5,3 | mountn | East Crag | empty | iron 75, silici 20 |
| R00025 | 0,4 | ocean | North Pelagic | empty | terair 100, water 600, food 30 |
| R00026 | 1,4 | sea | North Sound | empty | terair 100, water 400, food 70, h2o2 90 |
| **R00027** | 2,4 | grassl | Copse Grant | **fac 6 HQ** | terair 100, food 600, carbon 30, iron 20, water 150 |
| **R00028** | 3,4 | grassl | Windgap | **UN Windgap** | terair 100, food 450, carbon 20, iron 15, water 140 |
| R00029 | 4,4 | dust | Loess | empty | iron 35, silici 20, carbon 15 |
| R00030 | 5,4 | barren | East Flat | empty | iron 20, silici 15 |
| R00031 | 0,5 | ocean | Polar Ocean | empty | terair 100, water 700, food 20 |
| R00032 | 1,5 | ocean | Polar Ocean E | empty | terair 100, water 700, food 20 |
| R00033 | 2,5 | sea | Polar Sea | empty | terair 100, water 500, food 40, h2o2 120 |
| R00034 | 3,5 | grassl | Tundra | empty | terair 100, food 250, carbon 10, iron 10, water 200 |
| R00035 | 4,5 | mountn | North Spine | empty | iron 60, silici 20 |
| R00036 | 5,5 | dust | Polar Dust | empty | iron 25, silici 20 |

No `titani`, `copper`, `uraniu`, or `heliu3` on this grid. Hinterland: 27 empty of 36.

## Anvil region grid (P00005, 7×5)

Index: `R00037` + `Y*7+X`. Occupied cells in **bold**.

| Id | XY | Type | Name-en | Occupant | Resources (qty order of) |
|----|----|------|---------|----------|---------------------------|
| R00037 | 0,0 | ocean | West Sea | empty | terair 100, water 350, food 10 |
| R00038 | 1,0 | sea | West Shelf | empty | terair 100, water 200, food 30 |
| R00039 | 2,0 | dust | Pad | empty | copper 25, silici 40, titani 20, iron 25 |
| **R00040** | 3,0 | grassl | Slagport | **UN Slagport** | terair 100, food 120, water 80, iron 20, silici 30, titani 20 |
| R00041 | 4,0 | mountn | South Ore | empty | titani 60, silici 40, copper 30, iron 30 |
| R00042 | 5,0 | dust | South Dune | empty | copper 20, silici 40, titani 15, iron 25 |
| R00043 | 6,0 | barren | South Scarp | empty | silici 50, titani 20 |
| R00044 | 0,1 | sea | Northwest Sea | empty | terair 100, water 200, food 25 |
| **R00045** | 1,1 | grassl | Ironclad Grant | **fac 7 HQ** | terair 100, food 140, water 80, iron 20, titani 25, silici 30 |
| R00046 | 2,1 | grassl | Slope | empty | terair 100, food 100, water 70, silici 25, titani 15 |
| R00047 | 3,1 | mountn | West Spine | empty | titani 70, copper 35, silici 40, iron 30 |
| R00048 | 4,1 | mountn | Mid Spine | empty | titani 50, **uraniu 80**, silici 30, copper 20 |
| **R00049** | 5,1 | grassl | Oreline Grant | **fac 8 HQ** | terair 100, food 140, water 80, iron 20, titani 25, silici 30 |
| R00050 | 6,1 | dust | East Talus | empty | copper 25, silici 40, titani 20 |
| R00051 | 0,2 | grassl | Marsh | empty | terair 100, food 150, water 100, iron 15, silici 20 |
| R00052 | 1,2 | grassl | Bench | empty | terair 100, food 110, water 70, silici 25, titani 15 |
| R00053 | 2,2 | mountn | Crag | empty | titani 80, copper 40, silici 45, iron 35 |
| **R00054** | 3,2 | grassl | Basalt Grant | **fac 9 HQ** | terair 100, food 140, water 80, iron 20, copper 20, silici 30 |
| R00055 | 4,2 | dust | Scree | empty | copper 30, silici 40, titani 25, iron 20 |
| R00056 | 5,2 | mountn | East Peak | empty | titani 55, **uraniu 120**, silici 35, copper 25 |
| **R00057** | 6,2 | grassl | Ridge | **UN Ridge** | terair 100, food 100, water 70, silici 30, titani 20, iron 15 |
| R00058 | 0,3 | ocean | Gulf | empty | terair 100, water 350, food 10 |
| **R00059** | 1,3 | grassl | Silicate Grant | **fac 10 HQ** | terair 100, food 140, water 80, silici 40, titani 20, iron 15 |
| **R00060** | 2,3 | grassl | Vale | **HCS Crusthold** | terair 100, food 90, water 70, silici 25, iron 15 ; settlement 8 |
| R00061 | 3,3 | mountn | Uraninite | empty | **uraniu 150**, titani 40, silici 30, copper 20 |
| R00062 | 4,3 | grassl | Thin Soil | empty | terair 100, food 80, water 60, silici 20, titani 10 |
| **R00063** | 5,3 | grassl | Fission Grant | **fac 11 HQ** | terair 100, food 140, water 80, uraniu 15, titani 20, silici 30 |
| R00064 | 6,3 | dust | Fan | empty | copper 20, silici 45, titani 15 |
| R00065 | 0,4 | ocean | North Sea | empty | terair 100, water 350, food 8 |
| R00066 | 1,4 | sea | North Shelf | empty | terair 100, water 200, food 20 |
| R00067 | 2,4 | dust | Ash | empty | copper 15, silici 35, titani 15, iron 20 |
| **R00068** | 3,4 | grassl | Isotope | **UN Isotope** | terair 100, food 90, water 70, silici 25, titani 15, iron 15 |
| R00069 | 4,4 | mountn | Shield | empty | titani 65, silici 40, copper 30, iron 30 |
| R00070 | 5,4 | barren | Glass | empty | silici 50, titani 15 |
| R00071 | 6,4 | dust | North Reg | empty | copper 20, silici 40, iron 20 |

No `oil`. No commercial `carbon` (belt holds kerogen/coal analogues). `heliu3` not on this grid. Hinterland: 26 empty of 35.

## Same-system pockets (summary; full grids on XML pass)

Region ids after `R00071`. Name landings used by chemical hops.

| Body | First region | Size | Theme |
|------|----------------|------|--------|
| Scoria `P00002` | `R00072` landing | 6×4 | `dust`/`mountn`/`barren`; titani, copper, silici, iron; no food, no ground `terair`; `extraction` 4 |
| Helios belt `P00003` | composition on the belt | — | uraniu/nickfe/iron/carbon; wreck `W00001`; no settlement |
| Selene `M00001` | `R00128` | 5×3 | titani, silici; **polar `water` ice required** (≥2 regions, 40–80); moon exits not loaded today |
| Pyre `P00006` | after Helios block | 5×4 | hot dust; iron, silici |
| Fomal belt `P00007` | composition on the belt | — | carbon, oil; wreck `W00002` |
| Anvil moons / Fomal ices | later ids | 10–15 each | **water** ice mandatory; h2o2, heliu3 on ice moons |

Aeolus / Fomal giant: 0 surface regions.

## Empty systems (exploration briefs)

No player HQ, no NPC `city`, no t=1 contracts on these grids. Sparse resources allowed. Optional **hidden** wreckage in a later XML pass (outer moons/belts), not on the two starting continents. Virgin **habitable** worlds (Graph, Deep’s ice moon) are mid-game land grabs.

| SS | Star flavour | Bodies (type, AU, hab/exp) | Moons | Signature resources | Later alien seed (not t=1 on start worlds) |
|----|--------------|----------------------------|-------|---------------------|--------------------------------------------|
| SS0003 Ember | K2 | dust 0.7 (hot, not hab); ice-giant 4.1 **exp** (as `gasgnt`); abelt 2.2 **exp** | ice-giant 3 ice | titani, silici, heliu3 on ices; **`lithia` signature** (brines) | He3 plant wreck → production+energy |
| SS0004 Gleam | M1 | barren-dust 0.12; abelt 0.4 **exp**; ice-dust 0.8 **exp** | 0 | nickfe, uraniu, copper; **`reeox` signature** | Drone hangar wreck → military |
| SS0005 Cinder | K5 | dust 1.2 **exp**; abelt 2.8; gasgnt 8 | gasgnt 3 (vulcan, rock, ice) | carbon, tungst, uraniu; **`boron` + `grphit` signature** (vulcan / baked carbon) | Foundry wreck → production |
| SS0006 Ash | M0 giant | abelt 3 **exp**; gasgnt 6; gasgnt 18 | inner giant 4 mixed | heliu3, volatiles, ammoni; **`xenon` signature** (outer ices) | Fusion core wreck → propulsion+production |
| SS0007 Shards | G8 young | abelt 2.0 **exp**; abelt 3.1 **exp**; dust 0.9 | 0 | carbon, organics, ice; **`nitrat` signature** (evaporites) | Fauna/anomaly later |
| SS0008 Deep | K7 | gasgnt 4.5; gasgnt 9.2; dust 0.5 | 4+3 moons; one ice moon **hab** (thin `sea`+`grassl`, 12 regions) | water, methn, food (tight) | Spinal-weapon wreck → military |
| SS0009 Graph | G4 | ocean 0.95 **hab**; dust 1.6; abelt 2.5 **exp** | ocean 0; dust 1 rock | carbon, silici, iron, gold | Habitat wheel wreck → production/habitat |
| SS0010 Spare | K3 | dust 1.1 **exp**; abelt 2.4 | dust 1 ice | titani, copper, ice; **`berylm` signature** | Life-support wreck → research+habitat |

Place empty systems on the map farther than the Helios–Fomal pair (`X` 4+). XML star `type` remains `M4` until the catalog grows. Planet ids **`P00011`+** (Gates took `P00009`–`P00010`).

**L3+ signature ores** (not on Arbor/Anvil basins; see `resources.md`):

| Ore | Empty system | Why that world |
|-----|--------------|----------------|
| `lithia` | SS0003 Ember | Hot dust + ice-giant brine chemistry |
| `reeox` | SS0004 Gleam | Metal-belt REE with `nickfe` |
| `boron` | SS0005 Cinder | Vulcan fumaroles |
| `grphit` | SS0005 Cinder | High-T baked carbon, not peat |
| `xenon` | SS0006 Ash | Outer-ice adsorbed nobles |
| `nitrat` | SS0007 Shards | Young-system evaporites |
| `berylm` | SS0010 Spare | Light-metal barren crust |

SS0008 Deep keeps `methn` as its L3 volatile (already in the diet dictionary). SS0009 Graph stays a habitable carbon/silica prize, not an L3+ industrial signature.

Planet ids continue `P00011`+. Do not pre-place wreck stacks on Arbor/Anvil.

## Resource placement (general)

Use **[resources.md](resources.md)** (bands, environments, Arbor/Anvil, empty systems). Do not keep a second placement list here.

## Evolution after turn 1

Do not pre-build the whole inner-system industry. Add: depleted resource quantities after heavy extraction (edit `gamein` between GM runs), new NPC stacks (pirates, UN outposts), new contracts, and — rarely — a new moon/region only if a survey “reveals” it (prefer contracts + existing empty regions). First UN outposts on empty systems should be **new** `city`/`smhabi` stacks, not retroactive t=1 cities.

## XML pass status

Spec-complete here (militias + homeworld AP pair included). **`campaign/gamein.1.xml` is generated by `campaign/_gen_gamein.py`.** Regenerator: `python campaign/_gen_gamein.py`. Seed includes factions **1–13**, Helios+Fomal stars/planets **including Gates** (`pair=`), Arbor+Anvil full grids + UN + militia + player stacks + chemical hops and Gates, Scoria/Pyre/belts as landing grids, empty systems SS0003–SS0010 condensed, **no** empty-system APs. Encoding Windows-1251. Ids ≤ 6 characters. L3+ signature ores wait on catalog item rows.
