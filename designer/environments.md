# Environments: gravity, atmosphere, temperature

Campaign design for body attributes on planets/moons. Attrs are emitted on galaxy bodies and **live** in `LoadGalaxy` / `BodyEnvironment` (engine 0.1.148). Catalog planet/moon **types** carry default flavour only.

## Attributes (per body)

| Attr | Values | Meaning |
|------|--------|---------|
| `gravity` | `low` `normal` `high` | Surface g band (not continuous) |
| `temperature` | `habitable` `cold` `hot` | Settlement climate band |
| `atmosphere` | `none` `thin` `terair` `hostile` | Breathable / drag / heat |

`planet-atmosphere="terair"` on USE gates stays as today for habitable farming. Body-level `atmosphere` drives launch and land rules.

## Gravity + atmosphere → orbit launch (shuttles only)

**Proposal (accepted for campaign):** fixed integer **`h2o2` surcharge** per shuttle stack when completing a **surface → orbit** move. Not mass-fraction; no decimals. Same charge regardless of cargo mass (keep simple). Orbit → surface pays the same if atmosphere is present (re-entry / landing burn).

| Gravity | Atmosphere | Surcharge (`h2o2`) | Notes |
|---------|------------|-------------------|-------|
| `low` | `none` | **0** | Moon vacuum hop |
| `low` | `thin` | **2** | Thin CO2/N2 drag |
| `normal` | `none` | **4** | Airless 1g world |
| `normal` | `terair` or `thin` | **8** | Arbor/Anvil class |
| `high` | any | **16** | Expensive wells |
| any | `hostile` | use gravity row + **+4** | Venus-class heat/drag |

**Spaceships** (`frigate` / larger hulls): **cannot** use solid-surface exits on bodies with `atmosphere` ≠ `none`. They stay in orbit; only shuttles / landers ferry.

**High gravity upkeep:** modules with cash upkeep on a `gravity="high"` body pay **+50% cash** (round up). Habitats also pay +2 `food` / quarter if populated. Compensated by richer / rarer ores on high-g worlds.

## Temperature → settlement tech

| Temperature | Settlement gate | Benefit |
|-------------|-----------------|---------|
| `habitable` | `popcnt` / `city` / farms as today | Food, terair, liquid water |
| `cold` | `coldom` (cold dome) or nested `cryhab`; requires `cryres` tech path | Ice `water`, `heliu3`, `deutrm`, `ammoni`, `methn`, `xenon` pockets |
| `hot` | `hotdom` (heat dome); requires `hotset` tech | `boron`, `tungst`, `grphit`, fumarole metals, high-T ceramics feedstock |

Habitable worlds do **not** need cold/hot dome techs.

## Water as strategic resource

`water` (ice or liquid) is required feedstock for:

| Tech | Consumes | Produces | Role |
|------|----------|----------|------|
| `icemin` | regional ice (extraction) | `water` | Mine ice moons / polar ice |
| `wtrdst` | 1 `water` | 3 `h2o2` | Fuel feedstock |
| `hydrop` | 2 `water` (+ farm module) | 3 `food` | Food without grassland biosphere |
| `hydstg` / chemical drives | `water` or `h2o2` | short hops | Not AU |
| `fustch` / `fustor` | 2 `heliu3` / week | AU torch | [au-transit.md](au-transit.md) |

Homeworld **moons** must seed `water` even if rock-typed (polar ice / hydrated regolith). Liquid `water` stays rich on Arbor/Anvil oceans.

## Default body table (starting systems)

| Body | gravity | temperature | atmosphere | Water seed |
|------|---------|-------------|------------|------------|
| Arbor | normal | habitable | terair | rich liquid on ocean/sea/grassl |
| Selene (Arbor moon) | low | cold | none | **yes** polar/regolith `water` 40–80 |
| Scoria | low | hot | thin | trace/none |
| Pyre | low | hot | thin | none |
| Aeolus ices | low | cold | none | rich `water` + isotopes |
| Anvil | normal | habitable | terair | modest liquid |
| Anvil rock moon | low | cold | none | **yes** `water` 30–60 |
| Anvil ice moon | low | cold | none | rich `water` |
| Fomal ices | low | cold | none | rich `water` |

Empty systems: most moons `cold`+`low`; vulcan moons `hot`+`low`; rare high-g rock worlds carry `tungst`/`platnm`/`boron` bonuses.

## Gas-giant cloud deck

Gas giants (`type="gasgnt"`) have **no solid-surface regions**. Harvest happens in the planet **orbit**, which the engine treats as a cloud deck when the body emitted environment attrs.

### Atmosphere band → effective location-type

| Parent `atmosphere` | Orbit `EffectiveLocationType` | Cloud deck? | Orbit `<resource>` seed |
|---------------------|-------------------------------|-------------|-------------------------|
| `none` | `orbit` | No — vacuum corona; no cloud isotopes | none |
| `thin` | `atmosphere` | Yes — tenuous H/He mix | `heliu3` pocket only (30–50) |
| `hostile` | `atmosphere` | Yes — dense H/He/CH4/NH3 deck | `heliu3` + `deutrm` pockets (50–150 each) |
| `terair` | `atmosphere` | Rare on `gasgnt`; treat as thin cloud | `heliu3` trace only |

Habitable **rock/ocean** bodies keep orbit as `orbit` even when `atmosphere` ≠ `none` (Arbor/Anvil station-keeping). Only **gas-giant** orbits with emitted `atmosphere` ≠ `none` map to `atmosphere`.

Bodies that **omit** environment attrs (SampleGame) skip atmosphere gates; orbit resources are optional there.

### Orbit-held cloud resources

Seed on the gas giant `<orbit>` (not moons, not rings):

| Item | Band | Typical qty | Notes |
|------|------|-------------|-------|
| `heliu3` | pocket | 80–120 | Primordial He-3 in the upper H/He layer |
| `deutrm` | pocket | 50–100 | D/H enrichment in the hydrogen belt; **hostile** decks only |

Ice-moon regolith still carries pocket `heliu3` / `deutrm` for early L2–L4 paths; cloud decks are the **scale** source once `ramsco` and skimming techs are online. Do not duplicate the same `type` twice on one orbit.

Campaign gas giants at t=1 emit **hostile** + both isotopes (see `campaign/_gen_gamein.py`).

### Extraction modules and techs

| Id | Role | Gate |
|----|------|------|
| `ramsco` | Extraction module; passive `produce` `heliu3` in cloud deck | `operation-allowed-in location-type="atmosphere"`; USE build via `skimmn` |
| `skimmn` | Production USE → builds `ramsco` | requires `he3min` |
| `he3skm` | Active USE harvest `heliu3` from cloud | `extraction` + `location-type="atmosphere"` + `planet-type="gasgnt"` |
| `d2skm` | Active USE harvest `deutrm` from cloud | same gates; requires `d2ext` |

Regolith path unchanged: `he3min` on solid-surface extraction, `he3ext` drill, `d2ext` on ice moons. Frigate+ hulls **cannot** land on any atmosphere; shuttles ferry to ice moons. **Ram scoops operate only in gas-giant orbit** (effective `atmosphere`).
