# Galaxy — 10 players, two starting systems

Ten corporations (factions `2`–`11`) plus NPC faction `1` (**United Star Nations**). Seed is **not** one home system per player.

**Turn 1 occupancy:** two star systems only. Five players on starting system A, five on starting system B. Each player owns **one exclusive region** on that system’s habitable planet. The other **eight systems have no player HQ and no NPC cities**. They are exploration destinations.

Helios and Fomal are a **wide bound pair** (~200 AU), not light-years apart. No FTL. Inner-system hops are weeks; the pair hop is a season or more on chemical/fission stages; empty systems are farther still.

## Scale

| Object | Count | Notes |
|--------|-------|--------|
| Player factions | 10 (`name` 2–11) | Faction `1` = United Star Nations (cities, markets, later patrons) |
| Star systems | 10 | **2 occupied at t=1**, **8 empty** (no HQ, no NPC `city`) |
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

### Assembly market (t=1, like SampleGame Berlin)

On stack **Assembly** (`city`):

- `buying item="food" quantity="500" price="1"`
- `buying module="farms" quantity="2" price="100"`
- `selling item="food" quantity="120" price="4"`
- `selling item="terran" quantity="50" price="50"`

Slagport: `buying item="food" quantity="200" price="2"` (premium); `selling item="terran" quantity="12" price="50"`; optional `selling` of `titani`/`copper` at modest quantity if the XML pass stocks them in the granary.

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

Shared HQ nest (all ten): `corphq` 1 + CEO `terran` officer, `cargob` 2, `farms` 2–3, `cdrill` 1, `factry` 2, crew tens not hundreds. Faction `balance` ~10000 as SampleGame. Settlement `capacity` 8–12 on player grassland.

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

Grid: unique `(X,Y)` on that body. 4-neighbour `exit` pairs, both directions. Ground `duration`: grassland/dust 2–3, barren 3, mountain 4–6, sea/ocean 5–8.

The 6×6 / 7×5 starting grids are the **playable continent** (settled land + coastal seas), not a 1:1 globe. Planet `type="ocean"` still means a water world; most pelagic area is off-map.

Settlement `capacity` only on habitable or planned colony sites (4 tiny, 8 town, 12–16 metro basin). Extraction worlds: `extraction` capacity 2–6, no `settlement` until habitats exist.

Live region types only: `orbit` `ocean` `sea` `grassl` `dust` `mountn` `barren` `smmast` `smcast` `lrmast` `lrcast`.

## Space travel times (until drive speed is wired)

Orbit-to-orbit and planet-orbit exits use `exitmode mode="space" duration="N"` weeks.

**Loader today:** `loadGalaxyExits` walks **planet regions only**. A planet-region may `exit` to another `region` or an `orbit`. **Orbit elements and moon regions do not get exits parsed.** Campaign workaround: **bidirectional space exits between planet regions** (spaceports ↔ local dust/belt ↔ the other starting world). Moon maps stay in the spec for a later XML pass after TDD lands the wishlist row.

| Hop | Chemical / fission (L0–2) | L10 fusion ark (design target) |
|-----|---------------------------|--------------------------------|
| Surface ↔ local orbit | 0–1 | 0 |
| Planet ↔ its moon | 1–2 | 1 |
| Inner system (0.5–2 AU) | 8–13 | 2–4 |
| Helios ↔ Fomal (~200 AU pair) | 26 | 4–8 |
| To gas giant (5 AU) | 13+ | 4–8 |
| Outer belt (20–40 AU) | many turns | 8–13 |
| Occupied pair → empty system | 52+ | 8–13 |

Until AU×drive is wired, put those durations on the spaceport exits.

**t=1 spaceports (empty of cities/HQ):**

| From | To | Duration |
|------|----|----------|
| Arbor `R00006` Cinder Flats (5,0) `barren` | Scoria landing `R00072` | 8 |
| Arbor `R00006` | Helios belt landing `R00096` | 13 |
| Arbor `R00006` | Anvil `R00039` Pad (2,0) `dust` | 26 |
| Anvil `R00039` | Pyre landing (first Pyre region) | 8 |
| Anvil `R00039` | Fomal belt landing (first carbonaceous cell) | 13 |
| Reverse of each | — | same |

## Id allocation

| Kind | Pattern | Range (10-player seed) |
|------|---------|-------------------------|
| System | `SS0001`–`SS0010` | 10 |
| Star | `S00001`–`S00012` | extras if a visual binary |
| Planet/belt | `P00001`–`P0030` | ~25 bodies |
| Moon | `M00001`–`M0080` | unique ids even if loader currently copies planet id |
| Orbit | `O00001`–`O0120` | one per planet/moon |
| Region | `R00001`–`R1500` | Arbor `R00001`–`R00036`, Anvil `R00037`–`R00071`, then satellites + empty systems |
| Contract | `CTnnnn` | 6 chars |
| Wreckage stacks | `W00001`–`W0020` | faction 1; **not** on Arbor/Anvil grids at t=1 |
| NPC city trees | `100001`+ | avoid SampleGame `000001`–`000030` |
| Player HQ trees | `200001`+ (fac 2), `210001`+ (fac 3), … `290001`+ (fac 11) | |
| Officers | same numeric space as HQ | |

## Occupied systems — bodies

### SS0001 Helios (start A) — G2 flavour, XML star `M4`

`X="0" Y="0" Z="0"`. Star `S00001` Helios.

| Id | Name | Type | AU | Size | Hab/exp | Notes |
|----|------|------|----|------|---------|-------|
| `P00001` | Arbor | `ocean` | 1.0 | 6×6 | **hab** | Player + UN start; organics/iron/carbon; no titani/uraniu/copper |
| `P00002` | Scoria | `dust` | 1.5 | 6×4 (24) | **exp** | Ilmenite/copper plains; no biosphere |
| `P00003` | (Helios belt) | `abelt` | 2.7 | 8×4 (32) | **exp** | Metal rocks = `uraniu`/`nickfe`; some `lrcast` carbon |
| `P00004` | Aeolus | `gasgnt` | 5.2 | — | — | Orbit only |

Moons: Arbor `M00001` **Selene** `rock` 5×3 (titani, silici). Aeolus: 4 moons (ice, ice, rock, vulcan) — `heliu3` / `h2o2` / later `tungst`. Moon region exits: wishlist.

**Arbor flavour:** ~1 bar N2/O2. Grasslands fix carbon; banded iron in old basins; peat and coastal oil. Crust is sediment and granite — little rutile or pitchblende at the surface.

### SS0002 Fomal (start B) — F5 flavour, XML star `M4`

`X="1" Y="0" Z="0"` (pair axis). Star `S00002` Fomal.

| Id | Name | Type | AU | Size | Hab/exp | Notes |
|----|------|------|----|------|---------|-------|
| `P00005` | Anvil | `ocean` | 1.4 | 7×5 | **hab** | Player + UN towns; metals/fissiles; poor food/oil |
| `P00006` | Pyre | `dust` | 0.6 | 5×4 (20) | **exp** | Hot iron/silica; not habitable |
| `P00007` | (Fomal belt) | `abelt` | 2.5 | 8×4 (32) | **exp** | Carbonaceous: `carbon`, `oil`/`kerogn`, `volatl` |
| `P00008` | Fomal giant | `gasgnt` | 6.0 | — | — | Orbit only |

Moons: Anvil 2 (`rock`, `ice` 5×3 / 5×2) — extra rock metals; ice = water. Giant: 2 ice moons (`heliu3`, `ammoni`).

**Anvil flavour:** Breathable mix over a younger, thinner biosphere. Shield volcanoes expose ilmenite, native copper, uraninite veins. Soils are mineral; wetlands scarce; no commercial petroleum. Seas exist but ice and aquifers are modest.

## Arbor region grid (P00001, 6×6)

Index: `R00001` + `Y*6+X`. 4-neighbour ground exits. Occupied cells in **bold**.

| Id | XY | Type | Name-en | Occupant | Resources (qty order of) |
|----|----|------|---------|----------|---------------------------|
| R00001 | 0,0 | ocean | West Pelagic | empty | terair 100, water 600, food 40 |
| R00002 | 1,0 | sea | Shelf | empty | terair 100, water 400, food 80, h2o2 100 |
| **R00003** | 2,0 | grassl | Tidewatch Coast | **UN Tidewatch** | terair 100, food 500, oil 30, iron 15, water 150 |
| R00004 | 3,0 | grassl | South Vale | empty | terair 100, food 450, carbon 25, iron 20, water 120 |
| R00005 | 4,0 | dust | Launch Steppe | empty | iron 30, silici 25, carbon 10 |
| **R00006** | 5,0 | barren | Cinder Flats | **spaceport** (no city) | iron 20, silici 15 |
| R00007 | 0,1 | ocean | West Deep | empty | terair 100, water 600, food 40 |
| **R00008** | 1,1 | grassl | Northwind Grant | **fac 2 HQ** | terair 100, food 600, carbon 30, iron 20, water 150 |
| R00009 | 2,1 | grassl | Mid Vale | empty | terair 100, food 500, carbon 20, iron 15, water 140 |
| **R00010** | 3,1 | grassl | Greenwell Grant | **fac 3 HQ** | terair 100, food 600, carbon 30, iron 20, water 150 |
| R00011 | 4,1 | mountn | South Ridge | empty | iron 70, silici 20, carbon 5 |
| R00012 | 5,1 | dust | East Dune | empty | iron 30, silici 25, carbon 10 |
| R00013 | 0,2 | sea | West Coast | empty | terair 100, water 400, food 80, h2o2 80 |
| R00014 | 1,2 | grassl | Farm Belt | empty | terair 100, food 700, carbon 35, iron 20, water 160 |
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

No `titani`, `copper`, `uraniu`, or `heliu3` on this grid. Hinterland: 28 empty of 36.

## Anvil region grid (P00005, 7×5)

Index: `R00037` + `Y*7+X`. Occupied cells in **bold**.

| Id | XY | Type | Name-en | Occupant | Resources (qty order of) |
|----|----|------|---------|----------|---------------------------|
| R00037 | 0,0 | ocean | West Sea | empty | terair 100, water 350, food 10 |
| R00038 | 1,0 | sea | West Shelf | empty | terair 100, water 200, food 30 |
| **R00039** | 2,0 | dust | Pad | **spaceport** | copper 25, silici 40, titani 20, iron 25 |
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
| R00060 | 2,3 | grassl | Vale | empty | terair 100, food 90, water 70, silici 25, iron 15 |
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

No `oil`. No commercial `carbon` (belt holds kerogen/coal analogues). `heliu3` not on this grid. Hinterland: 27 empty of 35.

## Same-system pockets (summary; full grids on XML pass)

Region ids after `R00071`. Name landings used by spaceport exits.

| Body | First region | Size | Theme |
|------|----------------|------|--------|
| Scoria `P00002` | `R00072` landing | 6×4 | `dust`/`mountn`/`barren`; titani, copper, silici, iron; no food, no ground `terair`; `extraction` 4 |
| Helios belt `P00003` | `R00096` metal landing | 8×4 | mix `lrmast` (uraniu 50–150, nickfe) and `lrcast` (carbon); no settlement |
| Selene `M00001` | `R00128` | 5×3 | titani, silici; **moon exits not loaded today** |
| Pyre `P00006` | after Helios block | 5×4 | hot dust; iron, silici |
| Fomal belt `P00007` | carbonaceous landing | 8×4 | `smcast`/`lrcast`: carbon, oil, volatl |
| Anvil moons / Fomal ices | later ids | 10–15 each | water, h2o2, heliu3 |

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

Place empty systems on the map farther than the Helios–Fomal pair (`X` 4+). XML star `type` remains `M4` until the catalog grows.

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

Planet ids continue `P00009`+. Do not pre-place wreck stacks on Arbor/Anvil.

## Resource placement (general)

Use **[resources.md](resources.md)** (bands, environments, Arbor/Anvil, empty systems). Do not keep a second placement list here.

## Evolution after turn 1

Do not pre-build the whole inner-system industry. Add: depleted resource quantities after heavy extraction (edit `gamein` between GM runs), new NPC stacks (pirates, UN outposts), new contracts, and — rarely — a new moon/region only if a survey “reveals” it (prefer contracts + existing empty regions). First UN outposts on empty systems should be **new** `city`/`smhabi` stacks, not retroactive t=1 cities.

## XML pass status

Spec-complete here. **`campaign/gamein.xml` is not written yet.** Next XML skeleton (when scheduled): factions 1–11, Helios+Fomal stars/planets, Arbor+Anvil full grids + stacks + spaceport exits, Scoria/Pyre/belts as landing-region stubs (not necessarily every belt cell). Empty systems: star + 1–4 bodies with condensed region lists. Skip moon region exits until TDD. Encoding Windows-1251. Ids ≤ 6 characters.
