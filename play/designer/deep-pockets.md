# Deep pockets (subsurface resources)

Roughly **one in eight** habitable campaign regions carry a **deep pocket**: subsurface ore that **surface drills cannot see or extract**. Campaign seed places **one deep pocket per player HQ neighbourhood** (10/71 regions, ~14%).

## XML

Optional child on `<region>`:

```xml
<deep-pocket>
  <resource type="titani" quantity="45"/>
  <resource type="iron" quantity="60"/>
</deep-pocket>
```

Surface `<resource>` lines stay separate (visible whenever the region is visible).

## Visibility (live)

| Observer | Condition | Report |
|----------|-----------|--------|
| Exit hint from owned grant | Faction has **`cdrill` technology copy or core drill module** anywhere on a stack in the **source** region | `, deep pocket of resources detected` on the exit toward the pocket cell |
| Deep resource assay | Same, but **`cdrill` must be in the pocket region itself** | `Deep resources: …` line listing types and quantities |
| No cdrill | — | No hint, no deep line (surface resources unchanged) |

Nested factory copies under headquarters count toward the grant region.

## Extraction

Deep pockets require a **`cdrill` module** on-site with appropriate mining USE tech (`iminng`, `tminng`, etc.). Surface **`sdrill`** modules operate surface deposits only.

## Scouting doctrine

Prefer **`moblib` → `moblab`** with a **`cdrill` technology copy** aboard over ground trucks: the lab carries survey gear and the tech copy needed to read assays when it enters a pocket cell. Economic personas with seeded **`cdrill`** see adjacent hints from turn 1 once the factory copy is present.

## Seed map

See `HQ_DEEP_POCKETS` in `play/campaign/_gen_gamein.py` — one entry per faction 2–11 (adjacent cell or on-grant for Sundock).
