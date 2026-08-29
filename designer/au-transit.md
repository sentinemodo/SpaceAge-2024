# AU transit: fusion torch (design note)

Canonical play explanation: **AU hops use a fusion-class torch**. The ship **burns, coasts, burns**. Crew g-load stays manageable. The limiter is **helium-3**, not crew tolerance. Chemical / hydrolox (`rctdrv`, `hydnoz`, `autdrv`) are orbital and inner-system stages; they do **not** make the Gate in a season.

Live catalog: `campaign/data.xml`. Geometry: `campaign/gamein.1.xml` + [galaxy.md](galaxy.md). Engine duration: `SpaceTransit.DurationWeeks` (ΔAU × drive `speed`).

## Geometry (Helios)

| Body | Id | `AU=` | Source |
|------|-----|-------|--------|
| Arbor (Earth-analogue homeworld) | `P00001` | 1.0 | `gamein.1.xml` |
| Helios Gate | `P00009` | 80 | `gamein.1.xml` `<alderson>` |

ΔAU = **79 AU** = 1.182×10¹³ m (IAU AU = 1.495978707×10¹¹ m). `campaign/data.xml` has no AU attributes (catalog only). Type `adpnt` is unused; live Gates are first-class `<alderson>`.

Same numbers on Fomal: Anvil `P00005` AU 1.0 → Fomal Gate `P00010` AU 80.

## Physics used (bang-bang, then coast)

Start and stop at rest. Constant |*a*|. No continuous 1 g unless we choose it.

**Full burn (accel *T*/2, flip, decel *T*/2, no coast):**

*a* = 4*d* / *T*²  
Peak *v* = *a* · *T*/2  
Total Δv = *a* · *T*

**Fixed *a* = 1.5 g and fixed *T* with *d* < *a*(*T*/2)²:** burn *t_a*, coast, burn *t_a*.  
*d* = *a* · *t_a* · (*T* − *t_a*). Crew still ≤ 1.5 g.

*g* = 9.80665 m/s². *c* = 2.99792458×10⁸ m/s.

### 79 AU, time-constrained (what *a* the clock demands)

| Profile | *T* | *a* | Peak *v* | Total Δv | Notes |
|---------|-----|-----|----------|----------|-------|
| L2 torch, continuous | 13 wk | **0.078 g** (0.765 m/s²) | 3,010 km/s (0.010 *c*) | **6,010 km/s** | Comfortable g; not chemical |
| L10 ark, continuous | 4 wk | **0.824 g** (8.08 m/s²) | 9,770 km/s (0.033 *c*) | **19,540 km/s** | Still &lt; 1 g; hotter torch |

*a* scales as 1/*T*². 4 wk / 13 wk → *a* × (13/4)² = **10.6×**.

### 79 AU, *a* = 1.5 g (burn–coast–burn)

| *T* | Burn each end | Coast | Cruise *v* | Δv | Full-burn reach if no coast |
|-----|---------------|-------|------------|-----|------------------------------|
| 13 wk | **1.2 d** | **12.7 wk** | 1,520 km/s | **3,050 km/s** | 1,520 AU |
| 4 wk | **4.6 d** | **2.7 wk** | 5,850 km/s | **11,700 km/s** | 144 AU |

1.5 g does not shorten a *fixed* 13- or 4-week Gate hop; it **cuts burn time** and Δv versus a continuous low-g burn. Chemical still cannot supply thousands of km/s.

## Game mapping

| Role | Tech | Module | Space `speed` | Fuel | Gate (79 AU) ETA |
|------|------|--------|---------------|------|------------------|
| Chemical / hydrolox | `areact` `hydstg` `autprp` | `rctdrv` `hydnoz` `autdrv` | **0.5** | `h2o2` (cheap, 1/wk on `rctdrv`) | ~28 wk — not the crossing |
| **L2 fusion torch** | **`fustch`** | **`fustor`** | **1** | **2 `heliu3` / week** | ~14 wk (formula floor ~8–14 at speed 1) |
| L6 improved fusion | `fusdrv` (requires `fustch`) | `fuseng` | 2 (planned) | `heliu3` | ~7 wk |
| L10 ark | `arkdrv` | `arkeng` | ~3.5 (planned) | `heliu3`+`deutrm` | **4 wk** |

`SpaceTransit` at speed 1: `8 + 6 · (ΔAU / (ΔAU + 0.8))` weeks, then ÷ speed. 79 AU → ~14 weeks at speed 1, ~4 weeks at speed ~3.5.

**Why fusion at L2:** `he3fus` already builds `fusrec` (He3 power). The missing piece was a **drive**. `fustch` requires `he3fus`. Ion (`ionthr`) stays a planned high-Isp electric side path, not the Gate unlock.

**Fuel vs reaction:** `rctdrv` burns 1 `h2o2` per week. `fustor` burns **2 `heliu3` per week**. He3 is rare (ice/regolith, not habitable basins). A 13-week Gate run is ~26 He3 — a dedicated `he3ext` (20 / 13 wk) plus stockpile, not a casual tank of oxyhydro.

Same-body surface ↔ orbit stays **1 week** (not this formula). Gate ↔ pair Gate is **`JUMP` 1 week**, not a torch hop.

## Do not implement here

Duration and fuel consume on space `MOVE` are live. Do not retune `SpaceTransit` from this note. Planned later speeds on `fuseng` / `arkeng` land with those modules in the campaign catalog.
