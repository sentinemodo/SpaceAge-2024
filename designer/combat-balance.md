# Combat balance (campaign)

Canonical thresholds for retuning `campaign/data.xml` military modules. Engine rules: [`player/battle.md`](../player/battle.md). Typed matchups are design-time until TDD lands wishlist rows.

## Thresholds

| Rule | Value |
|------|-------|
| Primary outcome | **Capture complete** within ≤10 rounds |
| Small raid | **3** combat modules |
| Medium force | **8** combat modules |
| Small vs target | Capture **1** comparable-tech module in ≤10 rounds |
| Medium vs target | Capture/disable a **5-module** modest stack in ≤10 rounds |
| Tech gap | **+2 levels** ≈ **1.7–2.0×** effective combat power |

## Capture math

On capture hit, total pool progress = full `weaponDamage` (25% HP + 75% capture). Destroy uses the same `weaponDamage` entirely as HP.

Assumptions for benchmarks: **p_hit ≈ 0.5** (low defense), focus fire on one module at a time.

```
rounds_to_capture_one ≈ HP / (n_shooters × 0.5 × damage)
```

| Force | n | Constraint for ≤10 rounds |
|-------|---|---------------------------|
| Small vs 1 module | 3 | `damage ≥ HP / 15` |
| Medium vs 5 modules (sequential) | 8 | `damage ≥ HP / 8` |

Example pass: target HP 75, shooter damage 10 → small 5.0 rounds; medium clears five in ~9.4 rounds.

## HP vs size

Default loader: `HP = (mass + size) / 20` if omitted.

| Role | HP rule | Examples |
|------|---------|----------|
| Fragile | ≈ default | `xraylz`, dome habitats |
| Standard military | 2–4× default | `gunplc`, `tanks`, turrets |
| Armour plates | high HP, **no capture** (wishlist) | `cermpl`, `armplt` |
| Cities / habitats | ≈ `size/20` | `city` 1250, `smhabi` 80 |

Large structures may be fragile (city domes); armour is sturdy and soaked preferentially (5× hit weight wishlist).

## Typed weapon groups (design-time)

Catalog attrs (ignored by loader until TDD): `weapon-group`, `resists`, `armor-module`.

| Attacker (`weapon-group`) | Strong vs | Weak vs (`resists`) |
|---------------------------|-----------|---------------------|
| `laser` | unshielded | `shield` |
| `kinetic` | unarmoured | `armour` |
| `missile` | low PD | `pbpd` |
| `drone` | unwarned | `ew` |

Until the engine applies matchups, pair same-level weapons and resists in the stat ladder so +2 level advantage still holds under flat dice.

## L0–L10 combat ladder

| Level | Typical weapon dmg | Typical military HP | Typical shield def | Notes |
|-------|-------------------|---------------------|--------------------|-------|
| L0 | 2–3 | 50–70 | — | `gunplc` |
| L1 | 6–8 | 60–90 | — | `tanks`, `bltlas`, `laztrt` |
| L2 | 10–14 | 40–80 | def 4–6 | `xraylz` fragile HP |
| L3 | 12–16 | 80–120 | def 8–10 | `msltub`, `ewantn` |
| L4 | 14–18 | 100–150 | def 12 | `shplas`, `railgn`, `alndrn` |
| L5 | 16–20 | 110–160 | def 13 | `pdltur`, `cermpl` |
| L6 | 18–24 | 120–180 | def 14 | `coilgn`, `proxpd` |
| L8 | 24–32 | 150–220 | def 16 | `capshd`, `spnknc` |
| L10 | 32–45 | 180–250 | def 20 | ark grid |

**+2 level rule:** L(n+2) weapon vs L(n) defence → 3 shooters capture in ≤6 rounds; L(n) offence vs L(n+2) defence → stall or defender wins within 10 rounds.

## Benchmark matrices (post-retune — campaign/data.xml)

Assumes capture tactic, p_hit ≈ 0.5, focus fire.

### Stage L0–L1

| Scenario | Math | Rounds | Result |
|----------|------|--------|--------|
| Small 3× tanks (dmg 7) vs 1× gunplc (HP 60) | 60 / (3×0.5×7) | 5.7 | **pass** |
| Medium 8× tanks vs 5× gunplc | 5×60 / (8×0.5×7) | 10.7 | borderline (officer / prioritize → ≤10) |
| Small 3× laztrt (dmg 7) vs 1× gunplc | 60 / (3×0.5×7) | 5.7 | **pass** |
| Small 3× gunplc (dmg 3) vs 1× tanks (HP 80) | 80 / (3×0.5×3) | 17.8 | fail (static loses to mobile L1 — intended) |
| +2 levels: 3× xraylz (dmg 12) vs 1× gunplc | 60 / (3×0.5×12) | 3.3 | **pass** (≤6) |

### Stage L3–L4

| Scenario | Math | Rounds | Result |
|----------|------|--------|--------|
| Small 3× railgn (dmg 16) vs 1× shplas (HP 120) | 120 / (3×0.5×16) | 5.0 | **pass** |
| Medium 8× msltub (dmg 14) vs 5× military HP 100 | 500 / (8×0.5×14) | 8.9 | **pass** |
| Small 3× alndrn (dmg 8) vs 1× xraylz (HP 20) | 20 / (3×0.5×8) | 1.7 | **pass** |

### Golden drift

SampleGame goldens use **Tests/data.xml** (tanks dmg 4, gunplc HP 100) → capture near round 9 with officers. Campaign retune finishes **earlier**; do not change Tests goldens in this pass.

## Military production-time ladder (applied)

| Level | Use-time (weeks) | Campaign example |
|-------|------------------|------------------|
| L0 | 8 | `stnrdf` 8 wk, 8 iron + 4 titani |
| L1 | 10–13 | `armcbt` 10, `lasopt`/`lstrrt` 10, `frminf` 13 |
| L2–3 | 12–14 | `xraylo` 12, `drnhng` 14, `mslpod` 14 |
| L4–6 | 16–20 | `alnfgh` 16, `kntcgn`/`shpltc` 16, `gausgn` 20 |
| L7–10 | 24–40 | `crumtc` 24, `spngun` 32, `arkcns` 40 |

## Hull size classes

Until engine adds dedicated groups, keep `group="frigate"` and scale **size / capacity / nested combat stacks**.

| Class | Tech gate | Size / capacity | Nested combat | Hull HP |
|-------|-----------|-----------------|---------------|---------|
| Patrol | L0–1 `sshull` | 5k / 4.5k | 1–2 | 50 |
| Corvette | L2–3 `corhul` | 8k / 7k | 2–4 | 80 |
| Frigate | L4–5 `alnhul` | 12k / 10k | 4–6 | 120 |
| Destroyer | L6–7 `deshul` | 25k / 20k | 6–10 | 200 |
| Cruiser | L8–9 `cruhul` | 45k / 35k | 10–16 | 350 |
| Ark | L10 `arkhul` | 80k / 60k+ | 20–40 | 500+ |

## Engine dependencies

See [`engine-wishlist.md`](engine-wishlist.md): armour 5× hit weight + no capture; shield 90% intercept; typed `weapon-group`/`resists`; hull groups beyond frigate.
