# Fauna units (campaign)

Wild fauna are **seed-only** hostile stacks (factions 14–17). No technology produces them at t=1. Each habitable body has three native tiers mapped to the military ladder: **swarm** (infantry analogue), **bulky** (tank analogue), and **apex** (L3 analogue).

Benchmarks vs player units: [`combat-balance.md`](combat-balance.md) — `inftry` HP 50 atk 2 dmg 2; `tanks` HP 80 atk 6 dmg 7; `msltub` HP 100 atk 12 dmg 14.

## Design rules

| Rule | Value |
|------|-------|
| Capture cities | **No** — omit `can-convert` |
| Upkeep / consume | **None** — wild forage off-map; no cargo nests in seed |
| Diplomacy | Fauna factions 14–17 start **neutral** (attitude 2); hostile only after contact |
| Settlement rumors | When a fauna stack occupies a region **adjacent** to a region with a settlement-group module (`town`, `city`, `mtrply`, dome variants), the engine issues an anonymous planet-scoped **Rumor** naming the fauna region and stack id (engine **0.1.167**). Counts as contact for `DECLARE`. |
| Production | **None** at campaign start — catalog modules only for combat/load |
| Tame variants | Wishlist L3+ breeding techs (below); −1 atk, +can-convert, player upkeep |

Fauna vehicle-tier units use `group="vehicle"` but **no oil fuel** — metabolism covered by food/terair consume.

---

## Tier mapping (all bodies)

| Tier | Role | Arbor | Anvil | Haven | Graph |
|------|------|-------|-------|-------|-------|
| 1 | swarm | `brmstr` | `crstlb` | `ribgrz` | `silskk` |
| 2 | bulky | `mulcrw` | `slgmnt` | `glacra` | `qtzrol` |
| 3 | apex | `canalp` | `urstlk` | `frostb` | `spngrf` |

---

## Quarterly growth (wishlist — engine `Events` / GM resolve)

Evaluated **once per quarter** (week 13), per **fauna group**: all stacks owned by fauna factions 14–17 in one region.

Use the region’s native tier module ids from the table above. Roll independently per rule unless noted.

### Tier 1 (swarm stacks)

| Roll | Chance | Effect |
|------|--------|--------|
| Grow | **25%** | Each tier-1 stack: `qty += ceil(qty / 2)` |
| Promote | **50%** if **≥4** tier-1 units in group (sum of quantities) | Spawn **1** tier-2 stack (qty 1) in same region |

### Tier 2 (bulky stacks)

| Roll | Chance | Effect |
|------|--------|--------|
| Breed tier 1 | **100%** | Spawn tier-1 qty = current tier-2 unit count (sum tier-2 quantities) |
| Grow tier 2 | **25%** | Each tier-2 stack: `qty += ceil(qty / 2)` |
| Promote | **50%** if **≥4** tier-2 units in group | Spawn **1** tier-3 stack (qty 1) |

### Tier 3 (apex stacks)

| Roll | Chance | Effect |
|------|--------|--------|
| Breed tier 1 | **100%** | Spawn tier-1 qty = **2 ×** tier-3 unit count |
| Breed tier 2 | **100%** | Spawn tier-2 qty = tier-3 unit count |
| Grow tier 3 | **10%** | Each tier-3 stack: `qty += ceil(qty / 2)` |

New stacks inherit faction, forage cargo (`cargob` with food/terair), and native module type for that body.

---

## Movement (wishlist — end of quarter, after growth)

Same **fauna group** (one region, one fauna faction).

| Apex count (tier 3 units) | Chance | Behaviour |
|---------------------------|--------|-----------|
| **< 2** | **25%** | Entire group **MOVE** one random **solid** neighbour region (not `ocean`, `sea`, or other water) |
| **≥ 2** | **100%** | **Split**: half of stacks (round up) stay; half **MOVE** to a random eligible neighbour |

Split stacks keep proportional nested cargo. Movement uses ground `move` speed; no DECLARE required.

---

## Battle rewards (wishlist — on fauna stack destroyed)

When fauna modules are **destroyed** (not captured), drop **item stacks** and **research points** to attackers. Split loot **evenly among participating attacker stacks** (survived to battle end; equal shares, remainder to lowest stack id).

### Research points (RP) per unit destroyed

| Tier | RP / unit |
|------|-----------|
| 1 | 3 |
| 2 | 8 |
| 3 | 20 |

RP credited to the **faction** of each participating stack (split like items).

### Item drops per unit destroyed (by body)

| Body | Tier 1 | Tier 2 | Tier 3 |
|------|--------|--------|--------|
| **Arbor** | food 15, carbon 8 | food 25, carbon 15, iron 5 | food 40, carbon 25, water 20 |
| **Anvil** | silici 10, iron 8 | titani 12, silici 15 | uraniu 8, titani 10 |
| **Haven** | food 20, water 15 | food 30, water 25, methn 10 | food 50, water 40, heliu3 5 |
| **Graph** | silici 12, alumin 8 | silici 20, iron 10 | silici 30, platnm 5 |

Quantities are **per unit** in the destroyed stack; divide total drops across participating attacker stacks. Cargo nests on attackers if capacity allows; overflow spills to region as `itemstack` on ground.

---

## Unit stats by body

### Arbor (faction 14)

| Id | Name | Tier | HP | Atk | Def | Dmg | Speed | Flavour |
|----|------|------|----|-----|-----|-----|-------|---------|
| `brmstr` | brush stalker | 1 | 35 | 3 | 2 | 3 | 0.12 | Pack hunters in tall grass |
| `mulcrw` | mulch crawler | 2 | 65 | 5 | 3 | 6 | 0.35 | Peat-plated grazer |
| `canalp` | canopy alpha | 3 | 90 | 11 | 3 | 12 | 0.15 | Treeline apex; kinetic |

**Seeded:** Mid Vale (`brmstr`×2), East Steppe (`mulcrw`×1), Loess (`canalp`×1) — not on UN city cells (Windgap hosts city `100020`).

### Anvil (faction 15)

| Id | Name | Tier | HP | Atk | Def | Dmg | Speed | Flavour |
|----|------|------|----|-----|-----|-----|-------|---------|
| `crstlb` | crust burrower | 1 | 58 | 2 | 4 | 2 | 0.08 | Armoured diggers |
| `slgmnt` | slag mantlet | 2 | 75 | 6 | 5 | 6 | 0.30 | Mineral-plated grazer |
| `urstlk` | umber stalker | 3 | 85 | 10 | 4 | 13 | 0.12 | Rad spit; missile |

**Seeded:** Slope (`crstlb`×2), Scree (`slgmnt`×1), Crag (`urstlk`×1).

### Haven (faction 16)

| Id | Name | Tier | HP | Atk | Def | Dmg | Speed | Flavour |
|----|------|------|----|-----|-----|-----|-------|---------|
| `ribgrz` | ridge grazer | 1 | 42 | 2 | 3 | 2 | 0.14 | Hardy herd beasts |
| `glacra` | glacier crab | 2 | 72 | 4 | 5 | 5 | 0.25 | Ice-carapace scuttler |
| `frostb` | frost brood | 3 | 70 | 9 | 6 | 10 | 0.10 | Ice-glint beam; laser |

**Seeded:** Haven 1,1 (`ribgrz`×2), Haven 2,1 (`glacra`×1), Haven 0,1 (`frostb`×1).

### Graph (faction 17)

| Id | Name | Tier | HP | Atk | Def | Dmg | Speed | Flavour |
|----|------|------|----|-----|-----|-----|-------|---------|
| `silskk` | silicate skitter | 1 | 28 | 4 | 1 | 4 | 0.18 | Fragile shard swarms |
| `qtzrol` | quartz roller | 2 | 70 | 5 | 4 | 6 | 0.45 | Rolling boulder beast |
| `spngrf` | spine reef | 3 | 88 | 12 | 2 | 13 | 0.08 | Crystal spine; laser |

**Seeded:** Graph 2,0 (`silskk`×2), Graph 3,1 (`qtzrol`×1), Graph 1,2 (`spngrf`×1).

---

## UN fauna cull bounties (live)

`destroy-stack` contracts; cash to the destroying faction. Tier ladder matches combat risk (see [`economy.md`](economy.md) early bounty band).

| Tier | Default cash | Role |
|------|--------------|------|
| 1 swarm | **1000** | ~infantry-equivalent pack |
| 2 bulky | **2000** | ~tank-equivalent |
| 3 apex | **4000** | ~L3 predator |

Only **tier-1** destroy-stack bounties are seeded at t=1; tier 2/3 values apply when UN posts later contracts or GM injects them.

### Live t=1 bounties

| Contract | Body | Region | Target | Module |
|----------|------|--------|--------|--------|
| CT0016 | Arbor | Mid Vale | 140010 | `brmstr` ×2 |
| CT0019 | Anvil | Slope | 150010 | `crstlb` ×2 |

Haven / Graph pockets have no t=1 UN bounties. See [`contracts.md`](contracts.md).

### Living units (live)

Catalog `living-unit="yes"` on `inftry` and all fauna modules. Combat **reports** say wounded / heavily wounded / routed / slain instead of damaged / disabled / destroyed; mechanics unchanged.

---

## Tame variants (wishlist)

| Tech id | Name | Planet | Produces |
|---------|------|--------|----------|
| `brdtam` | brush domestication | Arbor | `tbrmst`, `tmulcr`, `tcanlp` |
| `crstam` | crust symbiosis | Anvil | `tcrstb`, `tslgmn`, `turstk` |
| `havbre` | haven brood imprint | Haven | `tribgz`, `tglacr`, `tfrost` |
| `grftam` | graft domestication | Graph | `tsilsk`, `tqtzrl`, `tspngr` |

Tame delta: atk −1, dmg −1, def +1, `can-convert="yes"`, upkeep ×3 vs wild.

See [`engine-wishlist.md`](engine-wishlist.md) for TDD surface.
