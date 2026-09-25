# Faction report XML: exit target stubs (visual tool)

Last updated: 2026-09-14

**Audience:** TDD (`Game.exe` report XML writer), visual-tool parser. **Designer does not implement C#.**

## Problem

Faction-filtered report XML (`report.{turn}.{faction}.xml`) is built by `Galaxy.SaveXml(doc, faction)`. A `<region>` is emitted only when `Region.Visible(faction)` — the faction holds at least one module stack in that cell ([`Region.Visible`](../../Game/data%20structures/Region.cs)). Exits on **visible** regions still list neighbours by id only:

```xml
<exit region="R00002">
  <exitmode mode="ground" duration="3"/>
</exit>
```

When `R00002` is not visible, it is **absent** from the galaxy slice. The visual tool regional map (`buildRegionMapCells` in `tools/visual-tool/src/parsers/reportXml.ts`) needs **X**, **Y**, and **type** for every exit target on the same body to lay out the grid and grey out unexplored neighbours. Today it falls back to “region not in report XML” with no coordinates.

Text reports already describe exit targets from live `Region` objects via `Exits.Report` (name, terrain, mode, anomaly/deep/settlement hints). XML reports must carry the same **map-relevant** facts without leaking full stacks for unseen cells.

## Required engine behaviour

When emitting `<exit …>` on a **visible** source region in a faction XML report:

1. **Map minimum** — For same-body **region→region** exits, embed enough data to draw the target cell: `X`, `Y`, `type` (terrain id), `name-en`. Apply whenever the exit is listed (the source region is visible, so the neighbour is at least **adjacent**).
2. **Intel-gated detail** — Add settlement capacity, surface `<resource>`, `<deep-pocket>`, and `<anomaly>` summary on the stub using the **same rules** as text exit hints and region assay lines (see [Visibility tiers](#visibility-tiers)).
3. **Undiscovered / no layout** — For exit targets the faction must not know (no adjacent line, no SEE/survey grant), omit coordinates or emit a placeholder stub (see [Schema](#recommended-xml-schema)). The visual tool renders those as unknown/grey without grid placement.
4. **No duplicate full regions** — When the target is already emitted as a sibling `<region>` (visible), the exit may omit `<target>` or repeat a minimal stub; consumers prefer the full `<region>` element when both exist.

Non-region exits (`orbit`, `belt`, `alderson`) are unchanged in this slice; the regional map uses region targets only.

## Visibility tiers

Align XML stubs with live text reporting ([`Exits.Report`](../../Game/data%20structures/Exits.cs), [`deep-pockets.md`](deep-pockets.md), [`anomaly-investigation.md`](anomaly-investigation.md)).

| Tier | Condition | XML on exit `<target>` |
|------|-----------|-------------------------|
| **Adjacent map cell** | Source region `Visible(faction)` and exit `To` is a `Region` on the same planet/moon | `name`, `name-en`, `X`, `Y`, `type`; attr `discovered="partial"` |
| **Visited cell** | Target region `Visible(faction)` (faction stack in cell) | Full `<region>` in galaxy slice; stub optional. Attr `discovered="yes"` if stub present |
| **Settlement hint** | Target `HasSettlement` and source visible | `<capacity group="settlement" quantity="…"/>` on stub (matches “, settlement detected”) |
| **Surface resources** | Target `Visible(faction)` | `<resource type="…" quantity="…"/>` children (same as today’s region save) |
| **Deep pocket hint** | Target `HasDeepPocket`, source visible, source `HasCdrillTechnologyFor(faction)` | `<deep-pocket>` with resources (matches “, deep pocket of resources detected”). Quantities only when mcored tech or core drill module is **in the pocket region** (matches “Deep resources:” assay) |
| **Anomaly hint** | Target has unresolved anomaly, source visible | `<anomaly type="…" points="…"/>` only — **no** `description`, rewards, or progress (matches “, anomaly detected”) |
| **Unknown** | Exit listed but faction lacks layout intel (future SEE/long-range cases) | `<target discovered="no"/>` or omit `X`/`Y`/`type`; visual tool greys label only |

**Note:** `Region.Visible` today means “stack present”, not “seen on map”. Adjacent stubs (`discovered="partial"`) intentionally expose grid layout and terrain id so the regional map matches text exit lines, while stacks and market data stay off the stub until visited.

## Recommended XML schema

### Region exit with adjacent stub (typical)

```xml
<region name="R00001" name-en="Rootfast Grant" X="2" Y="3" type="grassl">
  <capacity group="settlement" quantity="8"/>
  <exit region="R00002">
    <exitmode mode="ground" duration="1"/>
    <target name="R00002" name-en="North Ridge" X="2" Y="2" type="mountn" discovered="partial">
      <anomaly type="magnetic" points="8"/>
    </target>
  </exit>
  <exit region="R00009">
    <exitmode mode="ground" duration="1"/>
    <target name="R00009" name-en="East Flats" X="3" Y="3" type="grassl" discovered="partial">
      <capacity group="settlement" quantity="4"/>
    </target>
  </exit>
  <!-- stacks, resources, … -->
</region>
```

### Visited target (full region also present)

When `R00002` becomes visible, emit the full `<region name="R00002" …>` as today. Exits **from other regions** toward `R00002` may omit `<target>` (consumer resolves by id) or include a redundant minimal stub.

### Undiscovered placeholder

```xml
<exit region="R00099">
  <exitmode mode="ground" duration="2"/>
  <target name="R00099" discovered="no"/>
</exit>
```

### Attribute summary (`<target>`)

| Attribute | Required | When |
|-----------|----------|------|
| `name` | yes | Region id (same as parent `<exit region="…">`) |
| `name-en` | when `discovered` ≠ `no` | English label |
| `X`, `Y` | when map layout known | Grid coordinates on parent body |
| `type` | when terrain known | Catalog region type id (`grassl`, `sea`, …) |
| `discovered` | recommended | `partial` \| `yes` \| `no` |

Child elements reuse **gamein/report region shape**: `<capacity>`, `<resource>`, `<deep-pocket>`, `<anomaly>` — gated per [Visibility tiers](#visibility-tiers). Do **not** emit `<modulestack>` on stubs.

### Why nested `<target>` (not a second `<region>`)

- Keeps exit duration/mode and target payload in one place.
- Avoids duplicating full region trees for every adjacent unseen cell.
- Visual tool merges by id: full `<region>` wins over exit stub when both exist (`regionCatalog()` in `reportXml.ts`).

Alternative rejected: promoting every adjacent cell to a top-level `<region>` without stacks would confuse “visible region” semantics and inflate XML size.

## Consumer notes (visual tool)

After TDD lands:

1. Extend `parseExits()` to read optional child `<target>` and merge into `regionCatalog()` (stub fills gaps; full region overrides).
2. `buildRegionMapCells()` — place cells from stubs with `X`/`Y`; CSS class `exit-only` when `discovered="partial"` and no faction presence; `discovered="no"` shows in exit list only.
3. `describeExitTarget()` — prefer stub/full region fields for settlement, deep, anomaly summaries.

SampleGame goldens may omit `<target>` until refreshed; parser treats missing stub as today (backwards compatible).

## TDD acceptance criteria

1. **Fixture:** Two adjacent regions `R1`/`R2` on one planet; faction stack only in `R1`. Faction XML report includes `<region name="R1" …>` with exit to `R2` and `<target name="R2" X="…" Y="…" type="…" name-en="…" discovered="partial"/>`. No full `<region name="R2">`.
2. **Visited:** Same fixture with stack in `R2` → full `<region name="R2">` present; resources/stacks follow existing visibility rules.
3. **Hints:** Anomaly on `R2`, HQ on `R1` with mcored copy → stub includes `<anomaly type="…" points="…"/>`; deep pocket + mcored in `R1` → stub includes `<deep-pocket>` when assay rules match text report.
4. **Settlement:** Neighbour with `city` → stub includes settlement `<capacity>` when text report would append “, settlement detected”.
5. **NPC / unfiltered report:** `factionXMLreport == null` or faction `1` behaviour unchanged (all regions emitted; stubs optional).
6. **Load/save:** `<target>` is **report-only** — not written to `gamein.xml` / `gameout.xml` (emit in `SaveXml` report path only).

Suggested implementation surface: `Galaxy.saveExits` / region exit loop in [`Galaxy.cs`](../../Game/data%20structures/Galaxy.cs) — add `saveExitTarget(XmlDocument, XmlElement elExit, Exit exit, Faction faction, Region fromRegion)` calling shared helpers with `Exits.Report` visibility checks.

## Engine wishlist

Tracked in [`engine-wishlist.md`](engine-wishlist.md): **Exit target stubs in faction XML reports**.

## Related docs

- [`xml-schema.md`](xml-schema.md) — galaxy `<exit>` / `<region>` in gamein
- [`deep-pockets.md`](deep-pockets.md) — mcored-gated hints
- [`anomaly-investigation.md`](anomaly-investigation.md) — anomaly exit hints
- [`docs/architecture/delivery/campaign-play.md`](../docs/architecture/delivery/campaign-play.md) — visual tool dependency pointer
