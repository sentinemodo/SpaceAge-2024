# Contractor: story objectives → orders (CT0026 pattern)

Use when **## Turn priority** focus is **contract** and **Active contract** is a **`give-module`** job (e.g. CT0026: 4× `[farms]` to Rootfast `[120001]` at `[R00014]`).

## Decompose the contract (tactical checklist)

Before writing `#modulestack` blocks, split the CT into **quarter-sized** slices the engine can express. Keep **strategic** contract intent but sequence **prerequisites** first:

1. **Energy** — `@produce energy` on each `[cplant]` after one-shot `get 10 carbon`; more drills and factories need regional energy surplus.
2. **Drill quarter (13 weeks)** — stage output to cargob, then mix **iron / carbon** on one `[sdrill]` (add **`2 use tminng`** only when the grant **Resources** line lists **titani**):
   ```
   @give all to <cargob-id>
   use iminng
   10 use hcdril
   ```
   Do not `@get carbon` from the drill while on `@use iminng`. Use **`@give all to cargob`** so weekly production lands in staging.
3. **Capacity expansion** — when carbon/iron flow supports it: **`fossil` → second `[cplant]`** at grant, **`sdrill` tech → second drill** (parallel factory lines). Max drills only after energy covers nominal draw. **GRANT bootstrap:** if bank balance is **above ~5000** and the report market has **no sell offer** for iron/titani/etc., **`grant item N … to <cargob-id>`** before **`USE`** on a spare factory line (same doctrine as military/economic personas).
4. **CEO on site** — charter CEOs carry **`exmgmt`** (executive management). For a heavy quarter on **`factry` / `sdrill` / `cplant`**, **`#person <ceo-id>`** + **`stack <stack-id>`** (same region) so **+25%** energy/extraction/production bonuses apply; **`stack <hq-id>`** to return.
5. **Oil / forward base (before farm convoy)** — **`grndtr` scout** to the **first neighboring region** (report **Exits**) to find oil; later **`mobctr` → `[engtrk]`** (group **`production`**, same USE set as factory) can field-build **`fossil`/`sdrill`** at the oil site. CT **`agrplx`** needs **multiple eng trucks** at the build site to parallelize (each at ~0.1 factory speed); that burns **oil**—scout and remote power/drill first.
6. **Delivery (later turns)** — `transfer` to receiver `[120001]` / faction **12** at the contract **location** when `[farms]` stacks exist there.

## Order syntax rules (contractor)

| Pitfall | Fix |
|---------|-----|
| `@get 10 carbon` on cplant | **`get 10 carbon`** once, then `@produce energy` |
| `@get all carbon` while drill on `iminng` | Use **`@give all to cargob`** + **`10 use hcdril`** block instead |
| `sell 200 food at 2` when town **buys 60 @ 1** | **`sell 60 food at 1`** |
| Factory `agrplx` then truck `[farms]` | **Field `USE` on `[engtrk]`** at contract region; add eng trucks to shorten wall-clock |
| Skip energy before new drill/cplant | **Activate cplant**, then queue **`sdrill`/`fossil`** on free factory lines |

## Do not

- Re-run **completed** town charters after CT0008 closed.
- Send multi-hop convoys to **`R00014`** before oil/scout justifies the march (fauna on alternate routes).

## Turn 2 exemplar (Rivermark CT0026 staging)

| Slice | Block | Notes |
|-------|--------|--------|
| HQ / farms / market | 220001, 220006, 220003 | `@produce terran`, `@use farmng`, `@get all food`, `sell 60 food at 1` |
| Activate cplant | 220007 | `get 10 carbon`, `@produce energy` |
| Drill mix | 220004 | `@give all to 220003`, `use iminng`, `10 use hcdril` (no `tminng` when grant Resources omit titani) |
| Scout for oil | 220005 / **new1** | `use grndtr`, `move R00016` (Central Basin), crew + 1 oil + food |
| Second drill | 220005 / **new2** | `grant item 25 iron to 220003` then `-use sdrill`, `+get 25 iron` (bootstrap when market has no iron sell offers) |

Later turns: **`mobctr`**, eng-truck columns, field **`agrplx`**, **`fossil`/`sdrill`** at oil region.

Reference: `play/runs/beta-1/factions/04/orders.4.2.2.txt`, `play/runs/beta-1/gm/f4-ct0026-quarter-plan.md`.
