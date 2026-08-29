# Cash and market (campaign)

Canonical cash flow and t=1 UN books for `campaign/data.xml` and `galaxy.md`. Engine timing: [`player/rules.md`](../player/rules.md). Do **not** copy these numbers into `Tests/data.xml` (SampleGame stays at HQ **100**/week).

## Clock

| Rule | Live engine |
|------|-------------|
| Turn | **13 weeks** |
| Cash upkeep | Once, week 13 (`ExecuteMaintenance`) |
| Food / `terair` consume | Same quarterly pass (not weekly) |
| HQ / farm / mine output | Weekly (`produce` duration 1, `use-time` default 1) |
| City cash | `1000` cash / **13** weeks (~77/week) |
| Remote `BUY` | Flat **100** transfer cost if buyer and seller regions differ |
| `Market.GetPrice` | **0** until a trade posts a price; NPC auto-list skips price ≤ 0 |

Catalog `upkeep reduction` on `corphq` is **not applied** by the engine today. Nest bills below are gross.

Starting HQs must carry leftover `@produce cash`. No order → no HQ income that turn.

## Module cash upkeep

Per module per turn. Race wages (crew 1, officer 10) are **extra**.

```
raw = size/200 + 4×crew + build_cost/8 + 10×level
upkeep = 1                    if size ≤ 15
       = 5                    if raw < 5 and size > 15
       = round(raw to 5)      otherwise
```

`build_cost` is the producing tech’s `use-consume` items at nominal market values (table below). `level` is that tech’s catalog level.

| Exception | Cash | Why |
|-----------|------|-----|
| `city` `mtrply` | **0** | Food tax + cash produce; they are the tax base |
| Tiny energy (`wnplnt` `slrpnl`) | **1** | size 10, 1 iron |

Crew on the module is the **minimum complement**, not the people stacks. Tanks (crew 16) are dear; infantry (no crew attr, L1) stays cheap. High-tech weapons pick up `10×level` even when small.

`campaign/data.xml` follows this. `Tests/data.xml` does **not**.

Worked examples (old → new):

| Module | size | crew | L | cost | Was | Now |
|--------|-----:|-----:|--:|-----:|----:|----:|
| `corphq` | 1000 | 20 | 0 | 43 | 100 | **90** |
| `farms` | 1000 | 5 | 0 | 20 | 50 | **30** |
| `factry` | 1000 | 10 | 0 | 70 | 60 | **55** |
| `wnplnt` | 10 | 0 | 0 | 2 | 10 | **1** |
| `shuttl` | 200 | 2 | 0 | 11 | 100 | **10** |
| `tanks` | 240 | 16 | 1 | 24 | 24 | **80** |
| `xraylz` | 100 | 1 | 2 | 52 | 100 | **30** |
| `arkhul` | 80000 | 0 | 10 | 1080 | 300 | **635** |

Shuttle and HQ were oversized vs size/crew. Tanks pick up 16 crew. Ark picks up size and L10.

## Decision

| Knob | Campaign | SampleGame (do not retune) |
|------|----------|----------------------------|
| **One `corphq` produce** | **50 cash / week** (650 / turn) | 100 / week (1300 / turn) |
| Faction `balance` | 10000 | 10000 |
| City produce | 1000 cash + 10 `terran` / 13 weeks | same |

**50/week** is the campaign HQ yield. After the upkeep pass the starting nest is cheaper than the old punitive bills (farms 50, shuttle 100, HQ 100), so year-0 is solvent with cash to spare. **Trade is still for missing ores**, not for payroll. SampleGame 100/week is the test catalog.

## Starting nest (cash / turn)

Crew **30**. Arbor **3** `farms` + **2** `cplant`. Anvil **2** `farms` + **8** `wnplnt`. Shared: `corphq` 1, `cargob` 2, `cdrill` 1, `factry` 2, CEO officer. Rates from the formula.

| Line | Qty | Rate | Arbor | Anvil |
|------|-----|------|------:|------:|
| `corphq` | 1 | 90 | 90 | 90 |
| `cargob` | 2 | 10 | 20 | 20 |
| `farms` | 3 / 2 | 30 | 90 | 60 |
| `cdrill` | 1 | 50 | 50 | 50 |
| `factry` | 2 | 55 | 110 | 110 |
| `cplant` / `wnplnt` | 2 / 8 | 40 / 1 | 80 | 8 |
| CEO officer | 1 | 10 | 10 | 10 |
| Crew | 30 | 1 | 30 | 30 |
| **Nest bill** | | | **480** | **378** |

```
HQ_income_per_turn = produce_per_week × 13
net = HQ_income_per_turn − nest_bill
```

| HQ cash / week | / turn | Arbor net | Anvil net | Play |
|----------------|-------:|----------:|----------:|------|
| 40 | 520 | +40 | +142 | Solvent, tight |
| **50** | **650** | **+170** | **+272** | Solvent; cash for one UN factory bill / turn |
| 60 | 780 | +300 | +402 | Soft |
| 100 (test catalog) | 1300 | +820 | +922 | HQ is the economy |

Bank 10000 with **no** `@produce cash`: Arbor lasts ~20 turns on the nest bill alone. That is the missed-order safety net, not the plan.

## Why not keep 100

- **City vs HQ.** `popcnt` is 26 weeks and 100 iron for 1000 cash/turn (0 cash upkeep). Campaign HQ is 6 weeks and mixed metals for 650 cash/turn (90 upkeep). City still wins once standing; HQ is the early solvent.
- **Second HQ.** Arbor has no surface `copper`. At 50/week a second HQ still wants the Anvil hop; at 100/week the first HQ already funds the trip with spare change.
- **UN books.** Assembly buys food at **1**. Three farms dump 195 food/turn = 195 cash — less than one HQ week at 100. Markets only matter if HQ is tight.

Keep city at 1000/13 weeks. One city still beats one HQ (1000 vs 650) once it is standing and fed. HQ is the early-game solvent; metro is the late-game printer.

## Production cost (bid floor)

Extractor-week cost uses **L0 `sdrill` 35** cash/turn (starting nest also has a `cdrill` at 50 — same outputs, dearer). Farm uses **30**/turn. Output is weekly × 13.

| Item | Tech / week | Units / turn | Cash / unit | Bid (UN buys) | Ask (UN sells) | Nominal `value` |
|------|-------------|-------------:|------------:|--------------:|---------------:|----------------:|
| `food` | `farmng` 5 | 65 | 0.46 | **1** | **4** | 2 |
| `water` | `icemin` 3 | 39 | 0.90 | 1 | 2 | 1 |
| `h2o2` | `wtrdst` 1→3 | 39 | ~0.3 + water | 1 | 3 | 2 |
| `terair` | `lifsys` 10 | 130 | cheap | 1 | 2 | 1 |
| `iron` | `iminng` 3 | 39 | 0.90 | 1 | 3 | 2 |
| `carbon` | `hcdril` 1 | 13 | 2.69 | 2 | 4 | 3 |
| `oil` | `oildwe` 2 | 26 | 1.35 | 2 | 5 | 3 |
| `silici` | `slcmlt` 1 | 13 | 2.69 | 2 | 4 | 3 |
| `titani` | `tminng` 2 | 26 | 1.35 | 2 | 5 | 4 |
| `copper` | `cminng` 2 | 26 | 1.35 | 2 | 5 | 4 |
| `uraniu` | `uminng` 1 | 13 | 2.69 | 4 | 8 | 6 |
| `terran` | city 10/13 | 10 | labour | — | **50** | 50 |

**Bid ≈ 1× cost** (player dump). **Ask ≈ 3–5×** (emergency buy). Anvil food ask is **6** (hungry metal world). Arbor metal ask is the high column (no surface `titani`/`copper`/`uraniu`).

Integer prices only. Remote transfer **100** kills 1-credit dribs: ship **bulk** (value ≫ 100) or trade in the same region.

`value` is design-time until TDD loads it into `Market.GetPrice` (today 0 → NPC auto-list skipped). **t=1 markets are standing XML offers**, not auto-list.

## t=1 UN books

Quantities = **one bootstrap**, not a skip of the resource split. 10 `titani` = one `factry` (`indust`). After that, fly or contract.

### Assembly (Arbor capital, food-export)

| Side | What | Qty | Price | Why |
|------|------|----:|------:|-----|
| buy | `food` | 500 | 1 | Dump; ~2.5 farm-turns from a player nest |
| buy | `carbon` | 80 | 2 | Local peat/coal |
| buy | `farms` | 2 | 100 | SampleGame module bid |
| sell | `food` | 120 | 4 | Emergency calories (not a diet) |
| sell | `iron` | 30 | 3 | One factory **or** three farms, not both |
| sell | `silici` | 15 | 4 | Arbor sediments are low |
| sell | `titani` | 10 | 6 | **One** factory bill; then Anvil |
| sell | `copper` | 8 | 6 | **One** HQ / electronics bill |
| sell | `terair` | 50 | 1 | Canned air for first hops |
| sell | `terran` | 50 | 50 | Labour; city also produces 10/turn |

### Slagport (Anvil, hungry metal town)

| Side | What | Qty | Price | Why |
|------|------|----:|------:|-----|
| buy | `food` | 200 | **2** | Premium vs Assembly 1 — Arbor can ship |
| buy | `iron` | 40 | 1 | Local dump |
| buy | `titani` | 20 | 2 | Local dump |
| buy | `copper` | 20 | 2 | Local dump |
| buy | `silici` | 20 | 2 | Anvil-rich |
| sell | `food` | 40 | **6** | Tight granary; do not feed the planet |
| sell | `titani` | 25 | 4 | Arbor import path |
| sell | `copper` | 25 | 4 | Arbor import path |
| sell | `uraniu` | 10 | 8 | Pocket, not a stockpile |
| sell | `silici` | 20 | 3 | Cheaper than Assembly |
| sell | `terran` | 12 | 50 | Small labour pool |

### Other UN towns

| City | Extra book |
|------|------------|
| Tidewatch | sell `oil` 15 @ 5; buy `food` 80 @ 1 |
| Windgap | buy `food` 100 @ 1; sell `food` 30 @ 4 |
| Ridge | sell `titani` 10 @ 4; buy `food` 60 @ 2 |
| Isotope | sell `uraniu` 6 @ 8; buy `food` 60 @ 2 |

Militias (Arbor First / HCS) start **without** buy/sell. Neutral trade is optional later; do not undercut UN books at t=1.

## Other cash

| Sink / source | Size | Notes |
|---------------|------|-------|
| `ctypln` | 500 cash + 26 iron | One-off; HQ at 50/week pays it in 10 weeks if you save |
| L1 research | 8 points | Labs, not cash |
| UN `give-module` pay | later | Tech/unit today; cash band 8k–15k on hostility charters ([`contracts.md`](contracts.md)) |
| Credit line 10000 @ 0.2 | interest on week 13 | Do not start in debt |
| High-g upkeep | +50% cash | Wishlist ([`environments.md`](environments.md)); not in the nest table |

## XML

`campaign/data.xml` `corphq`:

```xml
<produce item="cash" quantity="50" duration="1"/>
```

Standing offers live on UN `city` stacks in `gamein` (see [`galaxy.md`](galaxy.md)). Leave `Tests/data.xml` at 100.