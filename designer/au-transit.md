# AU transit: fusion torch (design note)

Canonical play explanation: **AU hops use a fusion-class torch**. The ship **burns, coasts, burns**. Crew g-load stays manageable. The limiter is **helium-3**, not crew tolerance. Chemical / hydrolox (`rctdrv`, `hydnoz`, `autdrv`) are orbital and inner-system stages; they do **not** make the Gate in a season.

Live catalog: `campaign/data.xml`. Geometry: `campaign/gamein.1.xml` + [galaxy.md](galaxy.md). Engine duration: `SpaceTransit.DurationWeeks` (ΔAU × drive `speed` × mass factor).

**`f(ΔAU)` is live 0.1.148 (2026-08-29).** Default workshop-frigate ETAs **2 / 6 / 13 / 39**.

## Geometry (Helios)

| Body | Id | `AU=` | Source |
|------|-----|-------|--------|
| Arbor (Earth-analogue homeworld) | `P00001` | 1.0 | `gamein.1.xml` |
| Helios Gate | `P00009` | 80 | `gamein.1.xml` `<alderson>` |

ΔAU = **79 AU** = 1.182×10¹³ m (IAU AU = 1.495978707×10¹¹ m). `campaign/data.xml` has no AU attributes (catalog only). Type `adpnt` is unused; live Gates are first-class `<alderson>`.

Same numbers on Fomal: Anvil `P00005` AU 1.0 → Fomal Gate `P00010` AU 80.

## Locked `f(ΔAU)` (live 0.1.148, 2026-08-29)

Natural log (`ln` = `Math.Log`). One ceil only, after dividing by `effectiveSpeed`.

```
if ΔAU < 0.1:
    f_raw = 50 × ΔAU
else:
    f_raw = 6 + 33 × ln((1 + ΔAU) / 2.7) / ln(80 / 2.7)

duration = ceil( f_raw / effectiveSpeed )
```

Moon-scale ΔAU < 0.1 stays linear: 0.04 → **2.00**. The log is the two-point fit through **(1.7, 6)** and **(79, 39)** so those raw values are exact (not “about 6”). Compact equivalent: `9.737438 · ln(1 + ΔAU) − 3.671731`. Do **not** implement `9.74 · ln(1 + ΔAU) − 3.67`: that is 1.7 → 6.004 → ceil **7**, and 79 → 39.011 → ceil **40**.

Hops 0.1 ≤ ΔAU ≲ 0.5 yield f_raw < 1 (0.5 AU → 0.28 → ceil **1**). Seed hops start at 1.7. No extra floor.

`ceil(f_raw / 1)` on the locked hops:

| Hop | ΔAU | f_raw | Default weeks (MUST) |
|-----|-----|-------|----------------------|
| Planet → moon | 0.04 | 2.000 | **2** |
| Planet → belt | 1.7 | 6.000 | **6** |
| Planet → gas giant | 4.2 | 12.382 | **13** |
| Planet → Alderson Gate | 79 | 39.000 | **39** |

Mass / thrust clamp **0.67–1.50**, reference **40000 / 4150**, same-body surface↔orbit **1 week**, baked exits still `ceil(exitDuration / effectiveSpeed)` — unchanged.

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

Play clock is the **39-week** L2 Gate (and **12-week** L10). The 13-week / 4-week rows are the **old saturating-formula profiles**, kept so the Δv demand of a short clock stays visible.

| Profile | *T* | *a* | Peak *v* | Total Δv | Notes |
|---------|-----|-----|----------|----------|-------|
| **L2 torch, accepted clock** | **39 wk** | **0.0087 g** (0.085 m/s²) | 1,000 km/s (0.0033 *c*) | **2,000 km/s** | Comfortable g; still not chemical |
| **L10 ark, accepted clock** | **12 wk** | **0.092 g** (0.898 m/s²) | 3,260 km/s (0.011 *c*) | **6,510 km/s** | `ceil(39 / 3.5)` |
| Old L2 clock (superseded) | 13 wk | 0.078 g (0.765 m/s²) | 3,010 km/s (0.010 *c*) | 6,010 km/s | Saturating `f` Gate |
| Old L10 clock (superseded) | 4 wk | 0.824 g (8.08 m/s²) | 9,770 km/s (0.033 *c*) | 19,540 km/s | `ceil(14 / 3.5)` |

*a* scales as 1/*T*². Accepted 12 wk / 39 wk → *a* × (39/12)² = **10.6×**. Old 4 wk / 13 wk was the same ratio.

### 79 AU, *a* = 1.5 g (burn–coast–burn)

| *T* | Burn each end | Coast | Cruise *v* | Δv | Full-burn reach if no coast |
|-----|---------------|-------|------------|-----|------------------------------|
| **39 wk (accepted)** | **0.39 d** | **38.9 wk** | 500 km/s | **1,000 km/s** | 13,700 AU |
| **12 wk (accepted ark)** | **1.3 d** | **11.6 wk** | 1,650 km/s | **3,310 km/s** | 1,300 AU |
| 13 wk (old clock) | 1.2 d | 12.7 wk | 1,520 km/s | 3,050 km/s | 1,520 AU |
| 4 wk (old clock) | 4.6 d | 2.7 wk | 5,850 km/s | 11,700 km/s | 144 AU |

1.5 g does not shorten a *fixed* Gate hop; it **cuts burn time** and Δv versus a continuous low-g burn. Chemical still cannot supply thousands of km/s. The accepted 39-week clock is a long coast with a ~1,000 km/s Δv budget — fusion, not oxyhydro.

## Game mapping

| Role | Tech | Module | Space `speed` | Fuel | Gate (79 AU) ETA |
|------|------|--------|---------------|------|------------------|
| Chemical / hydrolox | `areact` `hydstg` `autprp` | `rctdrv` `hydnoz` `autdrv` | **0.5** | `h2o2` (cheap, 1/wk on `rctdrv`) | **78** wk — not the crossing. Default-mass `rctdrv` hits MIN → **117** |
| **L2 fusion torch** | **`fustch`** | **`fustor`** | **1** | **2 `heliu3` / week** | **39** wk |
| L6 improved fusion | `fusdrv` (requires `fustch`) | `fuseng` | 2 (planned) | `heliu3` | **20** wk |
| L10 ark | `arkdrv` | `arkeng` | ~3.5 (planned) | `heliu3`+`deutrm` | **12** wk (`ceil(39 / 3.5)`) |

`SpaceTransit` at speed 1: locked `f(ΔAU)` above, then ÷ `effectiveSpeed`. 79 AU → **39** weeks at speed 1, **12** weeks at speed 3.5.

**Why fusion at L2:** `he3fus` already builds `fusrec` (He3 power). The missing piece was a **drive**. `fustch` requires `he3fus`. Ion (`ionthr`) stays a planned high-Isp electric side path, not the Gate unlock.

**Fuel vs reaction:** `rctdrv` burns 1 `h2o2` per week. `fustor` burns **2 `heliu3` per week**. He3 is rare (ice/regolith, not habitable basins).

A **39-week Gate** burns **78 `heliu3`**. That is not a workshop tank: a dedicated `he3ext` (20 / 13 wk) needs **four** extractor cycles (~80 He3, 52 weeks) to fill a one-way Gate load. Stockpile on ice moons / UN market; do not treat 26 He3 as a Gate reserve.

Same-body surface ↔ orbit stays **1 week** (not this formula). Gate ↔ pair Gate is **`JUMP` 1 week**, not a torch hop.

## Mass / thrust (live 0.1.148, 2026-08-29)

Catalog `speed` still owns tech (`rctdrv`/`hydnoz` **0.5**, `fustor` **1**). Mass only scales around the default ship. `fustor` speed stays **1**. `f(ΔAU)` is live (2 / 6 / 13 / 39).

```
load = (sum nested space mass-capacity) / max(Mass, 1)
referenceLoad = 40000 / 4150   // ≈ 9.6386
massFactor = clamp(load / referenceLoad, 0.67, 1.50)
effectiveSpeed = catalogSpaceSpeed * massFactor
duration = ceil( f(ΔAU) / effectiveSpeed )   // one ceil
```

- **Reference thrust** 40000 (`fustor` mass-capacity). **Reference mass** 4150 (default workshop frigate below).
- **MIN 0.67 / MAX 1.50** — scout ~33% faster, same-engine cargo ~50% slower. Unclamped scout ×0.59 / cargo ×3.5 is too swingy.
- Same-body surface↔orbit stays **1 week** (no mass). Launch/land surcharge is already the cost of that hop.
- Baked region/belt space exits: **yes**, `ceil(exitDuration / effectiveSpeed)` (scout reaches Scoria faster than a loaded hauler on the same 8-week exit).
- No drive / mass-capacity 0: **massFactor = 1** (speed-only, current behaviour).
- Hull `sshull` has neither speed nor mass-capacity. Engine looks up **nested** space-move modules (sum thrust; same speed lookup MOVE already uses). Extra torches raise `load` and can hit MAX.
- Chemical `rctdrv` (thrust 10000) on a default-mass hull hits MIN: published chemical ETAs ×1.5 (Gate **117** wk). Still not the crossing. Speed 0.5 remains the tech term (`ceil(39 / 0.5)` = **78** before the mass clamp).

`JUMP` is not a MOVE. Out of scope.

### Locked reference ships (one `fustor`, speed 1, thrust 40000)

Masses use live `campaign/data.xml` (`sshull` 100, `cbridg` 300, `fisrec` 140, `fustor` 1400, `crwqrt` 400, `factry` 750, `cmplib` 50, `cargob` 200; `terran` 4, `food`/`terair`/`h2o2`/`heliu3` 1, `uraniu` 8, `iron` 10).

| Ship | Modules | Crew | Stores | Mass | T/M | Factor |
|------|---------|------|--------|------|-----|--------|
| **Scout** | 1 `sshull` 1 `cbridg` 1 `fisrec` 1 `fustor` 1 `crwqrt` | 6 `terran` | 15 food, 15 terair, 20 heliu3, 2 uraniu | **2430** | 16.46 | **1.50** |
| **Default workshop frigate** | scout + 2nd `fisrec` + 2nd `crwqrt` + `factry` + `cmplib` | 20 `terran` | 40 food, 40 terair, 20 h2o2, 26 heliu3, 8 uraniu, 20 iron | **4150** | 9.64 | **1.00** |
| **Cargo** | 1 `sshull` 1 `cbridg` 2 `fisrec` 1 `fustor` 2 `crwqrt` **3 `cargob`** | 10 `terran` | 20 food, 20 terair, 26 heliu3, 4 uraniu, 2 h2o2, **1080 iron** (360 × 3; `cargob` cap 1800, iron size 5) | **14420** | 2.77 | **0.67** |

Default **must** keep the speed-1 AU table (2 / 6 / 13 / 39).

**26 `heliu3` is 13 weeks of torch — a gas-giant tank, not a Gate tank.** A Gate run needs **78 He3** (39 × 2). Loading 52 more on the default hull is mass 4202; factor stays ≈0.99; Gate stays 39. **Scout 20 `heliu3` is 10 weeks of torch**, not a Gate reserve. Scout Gate is **26 weeks = 52 He3**. Cargo Gate is **59 weeks = 118 He3**. Workshop XML stores are unchanged this pass (TDD/designer follow-up if catalog flavour still reads as a Gate tank).

### Approved weeks (speed 1, clamp 0.67–1.50)

`duration = ceil(f_raw(ΔAU) / effectiveSpeed)`. One ceil (live `SpaceTransit`). Do **not** `ceil` the published integer then divide again (that would add a week on several scout/cargo hops).

| Hop | ΔAU | Scout (1.50) | Default (1.00) | Cargo (0.67) |
|-----|-----|--------------|----------------|--------------|
| Planet → moon | 0.04 | 2 | **2** | 3 |
| Planet → belt | 1.7 | 4 | **6** | 9 |
| Planet → gas giant | 4.2 | 9 | **13** | 19 |
| Planet → Alderson Gate | 79 | 26 | **39** | 59 |

Scout: `ceil(2/1.50)=2`, `ceil(6/1.50)=4`, `ceil(12.382/1.50)=9`, `ceil(39/1.50)=26`.
Cargo: `ceil(2/0.67)=3`, `ceil(6/0.67)=9`, `ceil(12.382/0.67)=19`, `ceil(39/0.67)=59`.

After TDD lands the formula: tick the wishlist retune row; chemical-column footnote on the [galaxy.md](galaxy.md) travel table already uses MIN (Gate 117).

## Do not implement here

Duration, fuel consume, and `f(ΔAU)` are live. Planned later speeds on `fuseng` / `arkeng` land with those modules in the campaign catalog.
