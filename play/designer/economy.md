# Cash and market (campaign)

Canonical cash flow and t=1 UN books for `play/campaign/data.xml` and `galaxy.md`. Engine timing: [`play/player/rules.md`](../play/player/rules.md). Do **not** copy these numbers into `Tests/data.xml` (SampleGame stays at HQ **100**/week).

## Clock

| Rule | Live engine |
|------|-------------|
| Turn | **13 weeks** |
| Cash upkeep | Once, week 13 (`ExecuteMaintenance`) |
| Food / `terair` consume | Same quarterly pass (not weekly) |
| HQ / farm / mine output | Weekly (`produce` duration 1, `use-time` default 1) |
| City cash | `1000` cash / **13** weeks (~77/week) |
| Remote `BUY` | Flat **100** transfer cost if buyer and seller regions differ |
| `Market.GetPrice` | Regional average if any region posted; else catalog nominal `value`; else **0** |

Catalog `upkeep reduction` on `corphq` is **not applied** by the engine today. Nest bills below are gross.

Starting HQs must carry leftover `@produce terran`. No order → no HQ recruitment that turn. `@produce cash` on `corphq` still pays catalog cash (50/week) when explicitly ordered.

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
| Tiny energy (`wnplnt` `slrpnl`) | **1** | size 10, 1 iron |
| `city` `mtrply` | **0** | Food tax + cash produce; they are the tax base |

Crew on the module is the **minimum complement**, not the people stacks. Tanks (crew 16) are dear; infantry (no crew attr, L1) stays cheap. High-tech weapons pick up `10×level` even when small.

`play/campaign/data.xml` follows this. `Tests/data.xml` does **not**.

Worked examples (old → new):

| Module | size | crew | L | cost | Was | Now |
|--------|-----:|-----:|--:|-----:|----:|----:|
| `corphq` | 1000 | 20 | 0 | 43 | 100 | **90** |
| `brnofc` | 300 | 6 | 1 | 20 | — | **40** |
| `farms` | 1000 | 5 | 0 | 20 | 50 | **30** |
| `factry` | 1000 | 10 | 0 | 70 | 60 | **55** |
| `wnplnt` | 10 | 0 | 0 | 2 | 10 | **1** |
| `shuttl` | 200 | 2 | 0 | 11 | 100 | **10** |
| `tanks` | 240 | 16 | 1 | 24 | 24 | **80** |
| `uwtruk` | 350 | 2 | 1 | 28 | — | **25** |
| `uwtank` | 300 | 14 | 2 | 52 | — | **85** |
| `udrill` | 750 | 8 | 1 | 130 | — | **60** |
| `tdlpln` | 80 | 1 | 1 | 24 | — | **15** |
| `uscty` | 8000 | 0 | 2 | 240 | — | **90** |
| `xraylz` | 100 | 1 | 2 | 52 | 100 | **30** |
| `arkhul` | 80000 | 0 | 10 | 1080 | 300 | **635** |

Shuttle and HQ were oversized vs size/crew. Tanks pick up 16 crew. Ark picks up size and L10.

## Decision

| Knob | Campaign | SampleGame (do not retune) |
|------|----------|----------------------------|
| **One `corphq` `@produce cash`** | **50 cash / week** (650 / turn) | 100 / week (1300 / turn) |
| **One `corphq` `@produce terran`** | **1 terran / module / week** (13 / turn per module) | not on SampleGame `corphq` |
| **One `brnofc` `@produce cash`** | **20 cash / 2 weeks** (10/week effective; 130 / turn) | not in SampleGame |
| **One `brnofc` `@produce terran`** | **1 terran / 2 weeks** (6.5 / turn) | not in SampleGame |
| Faction `balance` | 10000 | 10000 |
| City produce | 1000 cash + 10 `terran` / 13 weeks | same |
| Town produce | 200 cash + 2 `terran` / 13 weeks | same |

**50/week** is the campaign HQ yield. After the upkeep pass the starting nest is cheaper than the old punitive bills (farms 50, shuttle 100, HQ 100), so year-0 is solvent with cash to spare. **Trade is still for missing ores**, not for payroll. SampleGame 100/week is the test catalog.

### Branch office (`brnofc`) vs HQ / town / city

Design intent: staff a **second settlement region** without cloning headquarters. Research L1 `brnofc` (requires `corpmg`, cost 8), build in a factory (4 weeks, half HQ metal bill), nest under a town/city/settlement.

| Source | Cash | Terran | Cash upkeep / turn | Region buffs | Footprint |
|--------|------|--------|-------------------:|--------------|-----------|
| `corphq` | **50 / week** | **1 / week** | 90 | upkeep −0.1 + fast construction (catalog; engine may ignore reduction) | size 1000, crew 20, tech-cap 2 |
| `brnofc` | **20 / 2 weeks** (~10/wk) | **1 / 2 weeks** | **40** | **none** | size 300, crew 6, tech-cap 1 |
| `town` | 200 / 13 wk (~15/wk) | 2 / 13 wk | 0 | — | settlement shell |
| `city` | 1000 / 13 wk (~77/wk) | 10 / 13 wk | 0 | — | settlement shell |

Worked upkeep for `brnofc` (L1, build_cost = 5×2 + 1×4 + 2×3 = **20**):

```
raw = 300/200 + 4×6 + 20/8 + 10×1 = 1.5 + 24 + 2.5 + 10 = 38
upkeep = round(38 to 5) = 40
```

**Shared `ProduceDuration`:** the loader keeps one duration per module (last `<produce>` wins). Branch office therefore uses **duration 2 on both lines**; cash quantity is **20** so effective cash is still ~10/week when `@produce cash` is ordered. Do not mix duration 1 and 2 on the same module until the engine stores per-item duration.

Net if always `@produce cash`: 130 − 40 = **+90 / turn** (HQ cash path is 650 − 90 = +560). Net if always recruiting: −40 cash / turn for **6.5 terran / turn** (~325 cash-equivalent at UN ask 50) — the intended job. Two branch offices still do not match one HQ on cash or on recruitment cadence, and they never grant HQ’s region-wide construction/upkeep effects.

## Starting nest (cash / turn)

Crew **30**. Arbor **3** `farms` + **2** `cplant`. Anvil **2** `farms` + **8** `wnplnt`. Shared: `corphq` 1, `cargob` 2, **`sdrill` 1** (surface drill only at seed), `factry` 2, CEO officer. Rates from the formula.

### Persona startup packages (`init-run.ps1`)

| Item | Default (non-economic) | Economic persona | Researcher persona |
|------|------------------------|------------------|-------------------|
| HQ extractor | `sdrill` module | `sdrill` module | `sdrill` module |
| Factory tech copy | none | **`cdrill`** on HQ `factry` | **`moblib`** on HQ `factry` |
| Faction `balance` | **10000** | **9000** (−1000) | **9000** (−1000) |
| `credit-line` | 10000 | 10000 | 10000 |
| Cargo extras | seed default | **10 titani** on HQ `cargob` (first `cdrill` build) | **5 oil** on HQ `cargob` |

Economic Interests pay **1000 cash** at init for a factory **`cdrill`** copy, then **`use cdrill`** to field the first **core drill** and stack more **`agrplx` / `cdrill`** on the grant once **`cplant`** energy keeps pace. Non-economic factions research **`cdrill`** normally (L1, 8 RP default). `_gen_gamein.py` emits **`sdrill`** only; persona injections happen after the preference roll.

| Line | Qty | Rate | Arbor | Anvil |
|------|-----|------|------:|------:|
| `corphq` | 1 | 90 | 90 | 90 |
| `cargob` | 2 | 10 | 20 | 20 |
| `farms` | 3 / 2 | 30 | 90 | 60 |
| `sdrill` | 1 | 35 | 35 | 35 |
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

Bank 10000 with **no** `@produce terran` (and no cash produce elsewhere): Arbor lasts ~20 turns on the nest bill alone. That is the missed-order safety net, not the plan. HQ recruitment is the default leftover order, not cash printing.

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

`play/campaign/data.xml` `corphq`:

```xml
<produce item="cash" quantity="50" duration="1"/>
<produce item="terran" quantity="1" duration="1"/>
```

`play/campaign/data.xml` `brnofc`:

```xml
<produce item="cash" quantity="20" duration="2"/>
<produce item="terran" quantity="1" duration="2"/>
```

`@produce terran` on HQ uses the terran line only (1 per active module per week). `@produce cash` uses the cash line only (50/week). Branch office same filter: terran order → 1 / 2 weeks; cash order → 20 / 2 weeks (~10/week). Both lines share module `ProduceDuration` (loader last-wins), so durations match. Engine must filter `ItemsProduction` by ordered item type and multiply by `QuantityOperational`.

Standing offers live on UN `city` stacks in `gamein` (see [`galaxy.md`](galaxy.md)). Leave `Tests/data.xml` at 100.

## Auto economy (open beta — engine live rules)

Called once per turn after week 13 maintenance, in order: `UpdateBankAccounts()` → **`UpdateRates()`** → **`GenerateOffers()`**.

### Settlement auto-buy refill (live)

Each quarter, for every NPC faction `[1]` **`town`**, **`city`**, or **`mtrply`** (metropoly) settlement stack, the engine refills buy offers to the tier defaults below. **Item quantities scale with settlement stack module count** (`quantity` on the stack). **Module bids are per stack** (singular module stacks), not multiplied by stack count.

Standing t=1 XML buys are preserved: existing offers keep their **price**; quantity is raised only when below the tier target. Militia cities (factions 12/13) and player-owned settlements are excluded unless transferred to faction 1.

| Tier | Item buys (× stack qty) | Module buys (per stack) |
|------|-------------------------|-------------------------|
| **`town`** | `food` **60 @ 1**; `iron` **15 @ 1**; `carbon` **15 @ 2** | — |
| **`city`** | `food` **100 @ 1**; `iron` **25 @ 1**; `carbon` **25 @ 2**; `silici` **20 @ 2**; `titani` **10 @ 2** | `cargob`, `farms`, `wnplnt`, `cplant`, `sdrill`, `cdrill`, `factry` — **1 each @ 50–100** (cargo/food/drill/factory @ 100, wind @ 50) |
| **`mtrply`** | `food` **500 @ 1**; `iron` **40 @ 1**; `carbon` **40 @ 2**; `silici` **30 @ 2**; `titani` **25 @ 2**; `copper` **25 @ 2**; `uraniu` **10 @ 4** | Same module types as city — **2 each @ 50–100** |

Implementation: `SettlementBuyBook.cs` + `Game.RefillSettlementBuyOffers()`.

### GenerateOffers — auto-sell (live)

Each quarter, for every NPC faction `[1]` stack whose module type is **`city`**:

- Scan on-hand item stacks (skip `cash`).
- If the city already has a **buy or sell** offer for that item type, skip (no simultaneous buy+sell of the same type; **standing offers are never rewritten**).
- Otherwise create a new **sell** offer: quantity = on-hand, price = `Market.GetPrice(item)` (regional average if any region posted a price, else catalog nominal `value`; skip if price ≤ 0).
- Farms and non-city stacks are not auto-listed.

### UpdateRates (open beta)

**Offer-pressure drift (per region, each quarter):** For each type with a regional list price, scan standing offers in that market. Highest **economically significant** buy bid below list pulls price down; lowest significant sell ask above list pulls up; both together target the average of those two anchors. Each move is capped at **10%** of the current list (minimum 1 credit step). Outliers are ignored for drift: sell ask **> 10×** list; buy bid **< list ÷ 10** (bid 1 at list 10 counts; bid 1 at list 100 does not). Orders remain valid and can still match; outlier **trades** with an explicit sell price also skip updating the regional list.

**Galaxy average sync:** For each `Region`, for each `ItemType` that has a price in **any** regional `Market.PriceList`, set this region's price to the **galaxy-wide average** of all regions that posted that type (integer rounding, minimum 1 if average ≥ 0.5). Types with no posted prices are unchanged. Standing offer **objects** keep their saved `Price` field; only regional `PriceList` entries move (feeds `GetPrice` for new auto-listings next quarter).

**Bank rates (player factions 2–11 only):**

| Condition | Adjustment |
|-----------|------------|
| Balance > 5 000 | `depositRate -= 0.005` (floor **0.01**) |
| Balance > 0 and ≤ 5 000 | no deposit change |
| Balance < 0 | `creditRate += 0.005` (ceiling **0.25**) |
| Balance ≥ 0 | no credit change |

NPC factions 1 / 12 / 13 are unchanged. Rates persist in save XML (`deposit-rate`, `credit-rate` on `<faction>`).

### Beta verification

- SampleGame: `ProcessGenerateAutoOffers` and turn 4 golden assert NPC city sells remain.
- SampleGame: `RefillSettlementBuyOffers_*` restores city food buys and adds missing module bids.
- Campaign: load `play/campaign/data.xml` + UN city snippet → `GenerateOffers` does not duplicate standing sells from `gamein.1.xml`.
- Unit: `UpdateRates` moves regional food price toward average after a trade posts a new price in one region.

### Settlement market evolution (wishlist — not live)

See [`engine-wishlist.md`](engine-wishlist.md): fulfilled buy books spawn garrison units; tech level on planet expands the autobuy resource list; nested factory production adds auto-sell module offers.