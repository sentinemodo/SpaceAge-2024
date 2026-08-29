# Environments: gravity, atmosphere, temperature

Campaign design for body attributes on planets/moons. Attrs are emitted on galaxy bodies (ignored by loader until TDD). Catalog planet/moon **types** carry default flavour only.

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
| `hydstg` / drives | `water` or `h2o2` as fuel | Δv | Propulsion |

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
