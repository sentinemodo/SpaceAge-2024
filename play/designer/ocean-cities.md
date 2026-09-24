# Ocean / underwater cities and support tech

Canonical design for grokbot wishlist: **Technologies for building underwater and surface (above-water) cities**. Live catalog tokens go in `play/campaign/data.xml`. Engine gaps stay in [`engine-wishlist.md`](engine-wishlist.md). Tech/module numbers also appear in [`technology.md`](technology.md) and [`catalog.md`](catalog.md). Undersea resource seeds: [`resources.md`](resources.md).

## Design decisions

### Three settlement tracks

| Track | Tech | Module | Where (today) | Air | Role |
|-------|------|--------|---------------|-----|------|
| **Above-water pontoon city** | removed from the live catalog | — | — | — | Not buildable. Under-surface city and the dome remain |
| **Under-surface city** | `usctyc` L2 | `uscty` | `liquid-surface` (proxy for seafloor) | Surface port + elevator shaft pumps ambient air down — **no** `terair` upkeep (flavour); omit upkeep line | Ownable corp colony |
| **Underwater dome** | existing `dmecns` L3 | `dmdcty` | `solid-surface` **and** `liquid-surface` | Closed dome — **still** `food`+`terair` upkeep | Same airless-rock dome, also seats on seafloor |

**Dome underwater is an extension of `dmecns` / `dmdcty`, not a new tech.** One pressure-shell craft; on seafloor it still needs canned air because there is no surface shaft.

### Solid underwater without crashing the loader

There is **no** `seafloor` / `solid-underwater` location-type today (`LoadLocationType` crashes on unknowns). Campaign XML uses **`liquid-surface`** for all undersea placement (ocean/sea cells already map to it). Descriptions and this file state the physics: seafloor bedrock / pressure hull on the bottom. Wishlist: `location-type="seafloor"` (alias solid underwater) and region typing for deep cells.

### No `terair` on under-surface city

Express by **omitting** the `<upkeep type="terair"/>` line on `uscty`. Food + cash remain. If the engine later forces canned air by location, wishlist the exemption (shaft-linked ambient air).

### Underwater ships gate reach and build

`uwtruk` (truck-class) and `uwtank` (tank-class) are **more costly** than `trucks`/`tanks` and naval `coastr`/`gunbot`. They are required to **build and reach** under-surface cities and underwater domes:

- Live MOVE mode: **`naval`** (loads today; unknown modes throw). Ships operate on `liquid-surface` only (no land port — unlike `coastr`).
- Intended play: factory `USE` on land, then `USE … FOR <uwtruk-in-ocean>` to seat the settlement under an underwater tender, or move tenders into the cell first.
- Wishlist: enforce that underwater settlements cannot be seated/reached by surface-only naval (`coastr`/`gunbot`); optional dedicated `underwater` MOVE mode later.

### Stealth

Underwater-placed settlements (`uscty`, `dmdcty` on liquid) and underwater ships/drills (`uwtruk`, `uwtank`, `udrill`) are **stealthy** unless the observer has **own underwater units in the region** or a **spaceship on the body's orbit**. **Live (2026-09-22):** `ModuleType.Underwater` + `ModuleStack.Visible` / `IsUnderwaterStealthy`. Surface presence alone does not reveal them.

---

## Tech / module numbers

Research cost default `8 * 2^(level-1)` unless noted. Single `requires=` edge (engine limit).

| Tech | Lv | RP | Requires | Use-time | Consume | Produce | Notes |
|------|----|----|----------|----------|---------|---------|-------|
| `ptncns` | 1 | 8 | `nvltrs` | 12 | 40 `iron`, 4 `titani` | `ptncty` | Pontoon / floating metro. Tag production. USE at `production` (no location — seat via FOR tender) |
| `tdlpwr` | 1 | 8 | `wndtrb` | 4 | 8 `iron`, 2 `copper` | `tdlpln` | Tidal / current turbines. Tag production |
| `udrill` | 1 | 8 | `sdrill` | 4 | 35 `iron`, 15 `titani` | `udrill` | Seafloor rotary drill. Tag production |
| `uwtrs` | 1 | 8 | `nvltrs` | 4 | 6 `iron`, 4 `titani` | `uwtruk` | Pressure-hull cargo sub. Tag production |
| `usctyc` | 2 | 16 | `uwtrs` | 12 | 60 `iron`, 20 `titani`, 10 `copper` | `uscty` | Shaft metro. Tag production. Requires underwater transport tech before research |
| `uwcbt` | 2 | 16 | `uwtrs` | 12 | 14 `iron`, 6 `titani` | `uwtank` | Combat sub. Tag military. Soft peer: naval `nvlcbt` |
| `dmecns` | 3 | 32 | — | 10 | 40 `iron`, 20 `titani` | `dmdcty` | **Extended:** module also operates on liquid-surface |

### Module stats vs peers

Cash upkeep from [`economy.md`](economy.md) formula.

| Module | Group | Size/mass | Crew | Cap | HP | Combat | Move | Upkeep / consume | Operate | Peer |
|--------|-------|-----------|------|-----|-----|--------|------|------------------|---------|------|
| `ptncty` | settlement | 6000/— | — | 4000 | 350 | — | — | food 30; cash **0**; produce 200 cash + 2 terran / 13 wk; `cannot-be-owned` | liquid + terair | `town` (solid) |
| `uscty` | settlement | 8000/— | — | 5000 | 300 | — | — | food 40; cash **90**; produce 150 cash / 13 wk; **no terair** | liquid only | between `town` and `dmdcty` |
| `dmdcty` | settlement | 5000/— | — | 3500 | 250 | — | — | food 20; terair 20; cash 75; produce 200 cash / 13 wk | solid **+ liquid** | unchanged bill |
| `tdlpln` | energy | 80/80 | 1 | — | 8 | — | — | cash **15**; produce energy **12** / 13 wk | liquid + terair | `wnplnt` (4 energy, cash 1) |
| `udrill` | extraction | 750/750 | 8 | 400 | 60 | — | — | cash **60** | liquid only | `sdrill` (500, cash 35, solid) |
| `uwtruk` | vehicle | 350/200 | 2 | 120 | 25 | — | naval 1 | cash **25**; food 8; terair 8; oil 2 / 13 wk | liquid only | `trucks`/`coastr` (cash 5, 2 iron build) |
| `uwtank` | vehicle | 300/320 | 14 | 200 | 95 | atk 7 def 6 dmg 8 kinetic | naval 1 | cash **85**; food 14; terair 14; oil 5 / 13 wk | liquid only | `tanks` (cash 80) / `gunbot` (weaker, cash 80) |

Build cost summary (more than surface/naval peers):

| Module | Tech bill | Peer bill |
|--------|-----------|-----------|
| `ptncty` | 40 iron + 4 titani | `town` 30 iron + 2 titani |
| `uwtruk` | 6 iron + 4 titani | `coastr`/`trucks` 2 iron |
| `uwtank` | 14 iron + 6 titani | `tanks`/`gunbot` 8 iron + 2 titani |
| `udrill` | 35 iron + 15 titani | `sdrill` 25 iron |
| `tdlpln` | 8 iron + 2 copper | `wnplnt` 1 iron |

---

## Undersea / ocean resource seeds

**No new resource ids.** Closed-set ids only ([`resources.md`](resources.md)). Photic `food` + hydrosphere `water` stay. Underwater drill (`udrill`) and undersea play add **seafloor / shelf** ores on `ocean`/`sea` cells:

| Id | Ocean/sea role | Arbor | Anvil | Graph / empty ocean | Notes |
|----|----------------|-------|-------|---------------------|-------|
| `water` | hydrosphere | rich | modest | present | unchanged |
| `food` | photic biomass | present | poor | present | fisheries; unchanged |
| `iron` | ferromanganese crust / banded seafloor | common on deep ocean | modest | common | `udrill` / `iminng` path |
| `silici` | biogenic ooze / diatom silica | uncommon | common | common | |
| `oil` | shelf seeps | **shelf seas only** | **absent** | absent | keeps Arbor/Anvil split |
| `carbon` | marine organics / soft sediment | deep pockets | absent | pocket | not graphite |
| `copper` | hydrothermal vents | **absent** | shelf/ocean | absent | Anvil smokers |
| `titani` | submerged ilmenite sands | **absent** | shelf pockets | absent | |
| `gold` | vent chimneys | absent | **trace** 1–2 cells | absent | |
| `nickfe` | Fe-Ni-rich nodule fields | **deep ocean pockets** (few cells) | absent | optional deep | expands prior asteroid-only seed |
| `methn` | clathrate hydrates | **deep ocean pockets** (few cells) | absent | absent | not L3+ signature; modest 6–10 |

Still **not** on habitable ocean basins: `heliu3`, `deutrm`, `xenon`, `lithia`, `boron`, `reeox`, `grphit`, `nitrat` (signature / isotope rules unchanged).

`terair` / `h2o2` remain **not** regional ores (`REGION_SKIP` in `_gen_gamein.py`). Distill `h2o2` from `water`; breathe open air or shaft air / canned `terair` as module upkeep dictates.

Generator: `play/campaign/_gen_gamein.py` ARBOR / ANVIL liquid rows + Graph liquid `graph_res`. Settlement capacity **2–4** on sea/ocean so reports show undersea civic slots; extraction capacity **2** on deep ocean cells with ore.

---

## Flavour (hard science)

- **Pontoon city:** displacement pontoons, sealed decks, desalination; crew breathe surface `terair` atmosphere — no canned-air bill.
- **Under-surface city:** pressure hull on the bottom; vertical shaft to a surface access port; elevator cages move people and cargo; shaft airflow is ambient surface mix forced down the column — no closed-loop scrubber tax.
- **Underwater dome:** same closed dome as airless rock; seawater outside, canned mix inside.
- **Tidal plant:** kinetic turbines in tidal streams / coastal currents; needs a terair world with liquid surface (wave/tide atmosphere coupling).
- **Underwater drill:** rotary bit and riser from a bottom skid; strips seafloor crust and nodules.
- **Subs:** pressure hulls, oil Stirling or diesel-closed cycle; ballast for depth; acoustic silence is the stealth fiction until the engine models visibility difficulty.

---

## Engine gaps (summary)

Full rows: [`engine-wishlist.md`](engine-wishlist.md).

1. `location-type` **seafloor** (solid underwater) for regions and operation/use gates.
2. Underwater **stealth** (+1 visibility difficulty; exemptions: own underwater units in region, or spaceship on orbit).
3. Gate **underwater settlement seat/reach** to `uwtruk`/`uwtank` (surface naval insufficient).
4. Optional MOVE mode **underwater** if naval must stay surface-only.
5. **`uscty` terair exemption** if engine ever auto-taxes canned air by location.
6. Tag or flag **underwater** modules/settlements for stealth and gate rules (non-crash attribute until then).
