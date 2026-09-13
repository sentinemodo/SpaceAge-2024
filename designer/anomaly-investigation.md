# Anomaly investigation

Regional **anomalies** are one-cell survey targets on the habitable grids. They are emitted as `<anomaly type="…" description="…"/>` on the **anomaly region**. Turn-1 reports from an adjacent HQ grant already append `, anomaly detected` on the matching exit (engine **0.1.160**). Full `description` stays hidden until investigation resolves the site.

Hard science only: spectra, magnetometry, passive seismics, gravimetry, gamma spectroscopy — not prophecy or psionics.

Seed list (HQ-adjacent, t=1): [`anomaly-seed.md`](anomaly-seed.md). Source ids: `campaign/_gen_gamein.py` (`HQ_ANOMALIES`).

---

## Investigation loop

### 1. Detect

| Stage | Player action | Engine today |
|-------|---------------|--------------|
| Distant hint | Own or SEE the **source** region (usually the HQ grant) | Exit line toward anomaly cell: `…, anomaly detected` |
| Approach | `MOVE` a ground stack onto the anomaly region (typical: `trucks` + `moblab`, 1 week per orthogonal hop) | Region visible; `<anomaly>` attrs load but payout blocked |
| Type ID | Optional: officer item `senpak` or module `survsc` on stack (wishlist **SEE** bonus) | Today: read `type` only after on-site or from saved notes |

### 2. Characterize (`RESEARCH` — live **0.1.162**)

Order: **`RESEARCH <region-id>`** when the target region has an unresolved `<anomaly>` (e.g. `RESEARCH R00009`).

- Stack must be **on** the anomaly region with at least one **research** group module (`moblab`, `cmplib`, `survsc`, `seissc`, `radlab`, `maglab`, `optlab`, …).
- Each week on `RESEARCH`, add **investigation points** to the region (persist `<progress>` / `<resolved>` under `<anomaly>` on save; rewards stay in gamein).
- Throughput = sum of module **investigation rates** (below), × **type match** multiplier, + **`research-throughput`** items (`senpak`, `gravrt`) on stack or officers (same bonus as normal lab `RESEARCH`).

**HQ minor anomaly threshold:** **8 investigation points** (≈8 weeks solo on-match `moblab`, ≈16 weeks off-match magnetic/seismic/gravimetric). **Resolve payout:** **+20 RP** on the completing lab (band 0) — a one-shot boost worth roughly 1.5 quarters of stationary `cmplib` output (~13 RP/qtr) or ~3 quarters of field `moblab` (~6 RP/qtr), so the adjacent anomaly beats idling on a new fixed lab.

**Major anomalies** (belts, ice moons, empty-system sites — injected later per [`contracts.md`](contracts.md)): **24–64 points**, same order, often gated by `research` contract on a wreck stack co-located in the region.

### 3. Resolve

When progress ≥ threshold:

1. Mark region **resolved** (remove or downgrade `<anomaly>` in save; keep a one-line “survey complete” blurb in reports).
2. Apply **payout band** = highest band the completing faction **could USE** today (known tech level), not stack module level alone.
3. Emit a **faction event** + **region report** paragraph (mechanism, numbers, no loot crate flavour).

Repeat visits to a resolved site give only the blurb — no duplicate tech copies.

### Module investigation rates (design targets)

| Module | Base pts/week | Best match (`type`) | Off-match |
|--------|---------------|---------------------|-----------|
| `moblab` | 1 | spectral, radiometric | ×0.5 |
| `cmplib` | 2 | spectral, radiometric (off-site data crunch) | ×0.5 |
| `optlab` | 2 | spectral | ×0.25 |
| `survsc` | 2 | spectral | ×0.5 |
| `seissc` | 2 | seismic, gravimetric | ×0.5 |
| `maglab` | 2 | magnetic | ×0.5 |
| `radlab` | 2 | radiometric | ×0.5 |
| `dpsens` | 3 | gravimetric, generic `anomaly` | ×0.5 |
| `bolsen` | 2 | spectral (thermal IR) | ×0.5 |

`moblab` is the **L0–1 workhorse**: half a `cmplib` on normal `RESEARCH`, but portable and sufficient to close the adjacent HQ anomaly in one season.

### Time and orders summary

```
Detect (reports) → MOVE (ground) → RESEARCH region-id (weeks) → Resolve (event + payout)
```

Optional parallel: **`RESEARCH TECHNOLOGY`** at HQ while a second team runs field **`RESEARCH <region-id>`** — investigation grants **RP head start** toward specific survey techs, not a substitute for the lab.

---

## Benefits by tech band

Research costs (default): `8 × 2^(level−1)` → L1 **8**, L2 **16**, L3 **32**, L4 **64**, L5 **128**, L6 **256**, L7 **512**, L8 **1024**, L9 **2048**, L10 **4096**.

Payouts **never skip more than one level** on the tree and **never grant L8+** from HQ-adjacent minors. Alien wreck **tech copies** stay on off-grid contracts ([`contracts.md`](contracts.md)); anomalies teach **local physics** and **shorten** the survey branch.

### L0–1 — bootstrap (filidx, cmplib, moblab, trucks)

**Tools:** `trucks`, `moblab`, HQ `cmplib`; maybe `senpak` on an officer.

| Benefit | Detail |
|---------|--------|
| **Survey blurb** | Full hard-science `description` on the region report (what the spectrometer sees). |
| **RP payout** | **+20 RP** on the completing lab toward **`optins`** (band 0) — one-time resolve bonus, not per-week output. |
| **Resource tease** | One-line hint in report: e.g. “ferrous”, “elevated U”, “silicate alteration” — **no** map quantity until L2+ band revisit rule (optional wishlist: **re-open** at higher band). |
| **Contract hook** | 10% chance UN posts a **local** `give-module` follow-up (deliver `moblab` to a neighbour faction) — flavour only at this band. |

**Realistic close time (8 pt threshold):** 8 weeks on-match `moblab` (spectral/radiometric); ~16 weeks off-match (magnetic/seismic/gravimetric). Add ~3 weeks factory pipeline (`moblib` → `moblab`) and 1 hop `MOVE` from grant. Researcher persona seeds `moblib` only — see startup package below.

### Researcher startup package (init-run)

| Item | All factions | Researcher persona only |
|------|--------------|-------------------------|
| Factory tech copy | none | `moblib` on HQ `factry` stack |
| Faction `balance` | 10000 | **9000** (−1000 cash; **`credit-line` stays 10000**) |
| Cargo | seed default | **+5 oil** on HQ `cargob` (same fuel rule as `trucks`: 1 oil / 13 weeks ground move) for initial `moblab` expedition |

Non-researcher factions research `moblib` normally (L1, 8 RP default). `_gen_gamein.py` must **not** emit `moblib` on factories; `init-run.ps1` injects it after persona roll.

### L2–3 — optical and radiation base (optins, radtol, shuttles)

**Tools:** `optlab`, `survsc` (after `survts`), `radlab` path; first **`shuttl`** for off-HQ sites later.

| Benefit | Detail |
|---------|--------|
| **Refined blurb** | Mineralogy / phase ID (e.g. magnetite vs slag, pitchblende vs potassium). |
| **RP head start** | **+8 RP** toward band-appropriate tech: **`survts`** (64), **`radtol`** (64), or **`seisns`** (128) depending on anomaly `type`. |
| **Finite deposit reveal** | On **same region** or **orthogonally adjacent** cell: set or bump a **finite** `<resource>` quantity (1–3 units/week cap) matching flavour — **not** a new infinite mine. |
| **Module discount** | One-time **−4 RP** on first USE of the matching survey module tech if not yet known (`survts` for spectral, etc.). |
| **UN charter seed** | Report mentions “Assembly Basin survey desk”; opens a soft hook for year-1 off-grid anomaly contract. |

### L4–5 — survey suite (survts, seisns, radtol, maglab)

**Tools:** `survsc`, `seissc`, `radlab`, `maglab`, `cmplab`; belt hops with `shuttl`.

| Benefit | Detail |
|---------|--------|
| **Tech copy (minor)** | If faction lacks the **matching L4–5 survey tech**, grant **copy** of **one**: `survts`, `seisns`, or `radtol` (never two). If already known → **+16 RP** toward `deepsc` / `matcmp` instead. |
| **Deposit confirm** | Finite deposit from L2–3 band **confirmed** with assay numbers; extraction module hint in text (`nminng`, `uminng`, `gminng`). |
| **Cross-branch hook** | Magnetic/seismic payouts may add **production** tease (ore lens depth); spectral/radiometric add **research** tease (instrument calibration). |
| **Survey map** | `ObjectsSeen`-style flag: anomaly region name appears in **Survey reports** with coordinates for allies/trade partners (optional). |

### L6–7 — fusion industrial science (matlib, magsns, fusdrv era)

**Tools:** `matlab`, `maglab`, `ntdiag`, `magsns`; frigate survey of moons.

| Benefit | Detail |
|---------|--------|
| **Characterization report** | Density, susceptibility, dose rate, seismic Q — numbers usable for **module placement** (shielding, siting fission vs fusion). |
| **RP grant** | **+32 RP** toward **`magsns`**, **`matlib`**, or **`exobio`** (fauna false-positive on spectral IR). |
| **Production payoff** | One **named** off-HQ region in the **same star system** gets a **survey-only** resource tag (player must still fly and extract). |
| **Contract** | UN **`research`** on a dust/ice anomaly elsewhere (Helios belt / Aeolus moon) with **16–32 points** — not a free L6 alien hull. |

### L8–10 — long baseline and ark prep (deepsc, bolsen, dosmtr, arknav)

**Tools:** `dpsens`, `bolsen`, `crydet`, shipboard labs; Gate transit.

| Benefit | Detail |
|---------|--------|
| **Navigation / timing** | Gravimetric and seismic minors: **−5%** effective AU uncertainty on **one** adjacent orbit survey (wishlist: nav assist flag, not FTL). |
| **RP grant** | **+64 RP** toward **`deepsc`**, **`dosmtr`**, or **`navast`**. |
| **Empty-system pointer** | Report ties anomaly signature to **one** Gate-linked empty system body (“Shards nitrate evaporites match steppe IR line”) — exploration **hook**, not tech copy. |
| **No ark prizes** | HQ minors do **not** drop `arkhul`, L9–L10 modules, or militia archive techs. |

---

## Anomaly type flavour (payout tendencies)

| `type` | Physical read | L0–1 tease | L2–5 usual payoff | L6–10 escalation |
|--------|---------------|------------|-------------------|------------------|
| **spectral** | Albedo, absorption features, IR thermal line | Organics vs oxide vs glass | **`optins`/`survts`** RP; silicate/`copper` finite tag on altered regolith | **`bolsen`** thermal; tie to evaporite or geothermal contract |
| **magnetic** | Dipole / remanence, banded iron | “Ferrous lens” | **`seisns`** or **`gminng`** hook; `iron`/`nickfe` finite adjacent | **`magsns`** RP; ore body depth for `nminng` |
| **seismic** | Microseismicity, impedance contrast | “Shallow void or fault” | **`seisns`** copy or RP; groundwater/`water` finite (not volcanic) | Void → **`orbfnd`** siting note; fault → planetary stress memo |
| **gravimetric** | Δg mass concentration | “Dense basement” | **`seisns`**; `tungst`/`iron` trace finite | Mass model for **`navast`**; empty-system density analogy |
| **radiometric** | Gamma spectroscopy, dose | “Handle as ore” | **`radtol`** RP/discount; **`uminng`**/`uraniu` finite **trace** (1–2/week cap) | **`dosmtr`** RP; shielding siting for reactors |
| **anomaly** | Multi-sensor mismatch | Generic “instrument disagreement” | Player picks **best** module match at ×0.75 rate | **`deepsc`** hook only on **major** sites, not HQ minors |

Generic **`anomaly`** is reserved for **major** belt/moon/empty-system sites injected mid-game. HQ seed uses the five typed sensors only.

---

## Campaign hooks — ten HQ-adjacent anomalies (t=1 safe)

Design rule: each payout is **unique per faction**, **local** (same planet), **finite**, and **≤ L5 tech copy** if a copy is granted at all. No t=1 wreck stacks on these cells.

| Faction | Region | Type | Intended resolve (L2–5 band) | t=1 balance note |
|---------|--------|------|------------------------------|------------------|
| 2 Northwind | Mid Vale | spectral | +4 RP `optins`; blurb confirms buried **iron oxide** in grassland | No new ore on grant tile |
| 3 Greenwell | South Ridge | magnetic | Finite **`iron`** on South Ridge (1/wk) after `nminng` | Banded-iron fantasy; not nickfe bonanza |
| 4 Rivermark | East Peak | seismic | +8 RP `seisns`; **`water`** finite on adjacent vale (fault conduit) | No geothermal plant |
| 5 Sundock | East Steppe | gravimetric | Gravimetric memo; +8 RP `seisns`; **`tungst`** trace finite (1/wk) on steppe | Namesake “dense basement” |
| 6 Copse | Windgap | spectral | +8 RP `survts`; IR line = **shallow heat**, not lava | Hooks year-2 **`solthp`** siting text only |
| 7 Ironclad | Slope | magnetic | Confirm **magnetite** vs slag; +8 RP `gminng` | Supports Anvil metal fantasy |
| 8 Oreline | Mid Spine | radiometric | +8 RP `radtol`; **`uraniu`** trace finite (1/wk, cap 2) | Oreline name; not a reactor site |
| 9 Basalt | Crag | seismic | Void/lens blurb; finite **`copper`** on Crag (1/wk) | Shallow contrast, not cave city |
| 10 Silicate | Bench | spectral | +8 RP `optins`; **`silici`** assay bump on Bench (+1 finite) | Altered regolith only |
| 11 Fission | East Peak | radiometric | +8 RP `radtol`; **`uraniu`** trace (1/wk) — **distinct** dose from faction 8 | Faction name flavour; still trace |

**L0–1 only:** all ten grant **survey blurb + 4 RP** toward `optins` even if the player never reaches L2.

**Defer to contracts:** alien threat, fauna, L3–L6 **wreck** `research`, and empty-system anomalies stay on [`contracts.md`](contracts.md) cadence (year 1+). HQ cells are **training anomalies**, not second wrecks.

---

## Engine gaps (see [`engine-wishlist.md`](engine-wishlist.md))

Rows added for TDD:

| Need | Objective | Suggested surface |
|------|-----------|-------------------|
| **`INVESTIGATE` order** | Weekly investigation progress on a stack sitting on an anomaly region | `InvestigateOrder`; parse like `ResearchOrder` |
| **Anomaly progress / resolved state** | Persist partial progress and prevent double payout | Region attrs `investigation-progress`, `investigation-resolved`; save/load in galaxy |
| **Type-matched investigation throughput** | `survsc`/`seissc`/`radlab`/`maglab` multiply points by `type` | Module attr `investigation-types="spectral magnetic"` or match table in code |
| **Investigation payout dispatcher** | On resolve, grant RP bank, finite resource, tech copy/discount by faction tech band | `AnomalyInvestigation.Complete(faction, region)` |
| **Re-open at higher band** (optional) | Resolved L0–1 site offers L4+ payout when player returns with better lab | Second threshold or `investigation-band` flag |
| **`SEE` / scan bonus** | `survsc`, `senpak` improve detect range or off-match rate | Existing wishlist row — tie to investigation |

**Live today (do not re-wishlist):** exit hint `, anomaly detected`; `moblab` fractional `research-output`; `RESEARCH` space-object proximity blurbs.

---

## Handoff

| Artifact | Action |
|----------|--------|
| `designer/anomaly-investigation.md` | This doc — benefit tiers, loop, HQ hooks |
| `designer/engine-wishlist.md` | New investigation rows |
| `campaign/*` | No XML change in this pass (rename + payouts after TDD) |
| `/player` | Do not write `order.investigate` until `INVESTIGATE` lands |

When `INVESTIGATE` is live, regenerate HQ anomalies only if payout attrs are added to region XML; until then, payouts can be **table-driven** from faction + region id + type.
