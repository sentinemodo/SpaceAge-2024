# SampleGame turn 5 — feature track

Last updated: 2026-08-21  
Engine cited: `0.1.144`

Turns 1–4 already ship MOVE, USE, COPY, TRANSFER, RESEARCH, TRAIN, CAPTURE, hangar-launch, and quarterly upkeep. This PR **lands new engine/catalog behavior** and locks it with a Sol golden. It is not a replay of “another ship to Luna.”

Do **not** implement typed weapon groups, `USE` produce-effect `repair`, or `REPAIR` without `CanOperate`. Those stay on wishlists.

## Vehicle

1. **First commit:** this file plus player order drafts. No C#.
2. **Later commits:** one row per commit. Stay green before the next phase.
3. New goldens need `/player` validation, a ±10-line hunk in chat, and explicit human approval.
4. Bump `EngineVersion` only in the commit that changes player-visible reports or committed SampleGame goldens.
5. Independent fixture (ADR-0004): do not `copyFile` turn-4 output onto turn 5 at runtime. Freeze `gamein.5.xml` from committed `gameout.5.xml`, then apply seeds.

## Story

Replace the stub comments on `ExecuteTurn5` (`moon colony` / `earth vs moon officers`):

- **Gelvaren** factory `[000023]` in Sydney builds an **orbital rocket launcher** `[orbrkt]` into shuttle `[117]` (`USE orbrkt as new141 for 117`). The shuttle `STACK OUT`s into **R00003** (same location; nested in the city it cannot fly) until `HAS 1 orbrkt`, then hauls to **Earth orbit `[O00002]`**, arriving **around week 10**. No `[orassm]` / `[orcmpx]`; no orbit `USE`.
- **Caste Prime** fuels alien drones `[450]`, leaves the immobile wreck `[200]` on Luna, and returns hull `[101]` plus the drones to **Earth orbit `[O00002]`**. They may arrive early and wait.
- Combat is weekly while co-located with `TACTIC destroy`. Intended outcome: **drones victory**.

Player drafts (live verbs only): `player/drafts/orders.5.2.txt`, `player/drafts/orders.5.3.txt`. Copy into `Tests/SampleGame/orders.5.*.txt` only after the fixture freeze.

## New vs already coded

| Feature | Engine today | Turn 5 job |
|---------|--------------|------------|
| **Orbital rocket launcher catalog** | Live `[rckter]` makes infantry item `[rctlnc]`. `[gunplc]` / `[laztrt]` only operate on solid ground. Campaign `[mslpod]`/`[msltub]` is not in `Tests/data.xml`. | **Implement:** L1 tech `[orbrkt]`, `use-allowed-in` production (ground **or** orbit), **use-time 8 weeks**. Module `[orbrkt]` operates in **orbit**. Size 40 so shuttle 125 still holds crew, food, terair, and fuel. Seed a copy on factory `[000023]` (replace `[armcbt]`). Twin into `campaign/data.xml`. |
| **Drone helium fuel as a root** | `[alndrn]` `capacity="0"`. `NeedFuel` reads `RootModuleStack.ItemStacksSumRecursive`. Nested in wreck `[200]`, helium on `[205]` would count; nested drones **inherit wreck immobile**. `STACK OUT` makes them mobile but they cannot `GET` helium. | **Implement:** give drones enough capacity to hold fuel (1 `heliu3` per drone) **or** let fuel items ignore capacity. Seed ≥4 `heliu3` on shuttle `[100]` or cargo `[205]` — `he3min` is 8 weeks per 1 unit, so mining cannot fuel four drones this quarter. |
| **Space battle at a named orbit ~week 10** | Co-located `ATTACK` / `TACTIC` already work. Hangar launch is battle-only (no `LAUNCH` verb). `ATTACK` only sets enemy attitude. | **Prove:** `[117]` (with nested `[orbrkt]`) at `[O00002]` around week 10; Caste drones as **roots** may wait there earlier. |
| **Freeze `gamein.5.xml`** | File is still the turn-36 stub (Mercury, old frigate/station). | **Replace** with committed `gameout.5.xml`, then seed `orbrkt` on `[000023]`, `uraniu`/`h2o2` on `[117]`, and ≥4 `heliu3` on `[100]` or `[205]`. Shuttle `[117]` keeps `[servic]`. |

`JUMP`, typed `weapon-group` vs `resists`, item `attack` in `ModuleStack.Attack`, `TRANSFER ALL`, medical-facility heal, and `USE repair`: out of this PR.

## Wishlist scan (not this PR)

| Source | Candidate | Why not turn 5 |
|--------|-----------|----------------|
| `player/order_wishlist.md` | `TRANSFER ALL` / pick damaged module | Drafts use `STACK OUT` + `GIVE`/`GET`. |
| `player/order_wishlist.md` | `REPAIR` without `CanOperate` | Captured Sydney guns are leftovers; not the space beat. |
| `player/technologies_wishlist.md` | Wire `USE repair` / `[medfac]` | Explicitly out since turn 4. |
| `designer/engine-wishlist.md` | Typed weapons, shield intercept, armour hit-bias | Too large; Battle stays flat. |
| `designer/engine-wishlist.md` | Item `attack`/`damage` in battle (`rctlnc`) | Infantry rockets, not orbital `[orbrkt]`. |
| `designer/engine-wishlist.md` | Shuttle orbit `efficiency-multiplier="10"` (50-week `orassm`) | No orbit `USE` this story; leave for a later construction beat. |
| `designer/engine-wishlist.md` | Shuttle launch fuel surcharge / land bans | Would block `[117]` unless extra `h2o2` is seeded; skip. |
| `designer/engine-wishlist.md` | Space transit ΔAU × drive speed | Luna→Earth must complete **this quarter**. Keep ETA 0/1. |
| `designer/engine-wishlist.md` | Moon `@name` / `loadGalaxyExits` | Hull already space-hops without matching exits. |

## Faction intents

State: `Tests/SampleGame/testreport.5.{1,2,3}.txt` (briefing after ExecuteTurn4).

- **NPC `[1]`** — Thin `orders.5.1.txt` only if leftovers must stay explicit. Auto-offers already run.
- **Caste Prime `[2]`** — Do not fly wreck `[200]`. Seed helium; `STACK OUT` drones; `MOVE` drones `O00001` then `O00002`; hull `[101]` `MOVE O00002`; wait in orbit; `TACTIC destroy`; `ATTACK 117`. Earth leftovers only.
- **Gelvaren `[3]`** — Seed `[orbrkt]` on factory `[000023]` (drop `[armcbt]`). `USE orbrkt as new141 for 117`. Shuttle: crew, `SET ONLINE`, `STACK OUT` into **R00003** (same location, becomes a mobile root), `HAS 1 orbrkt`, `-MOVE O00002` (~week 10). `TACTIC`/`ATTACK` drones and hull. Keep Earth war leftovers. `[117]` keeps `[servic]`.

## Commits

| # | Phase | Production | Prove with |
|---|--------|------------|------------|
| 0 | Docs | This checklist + `player/drafts/orders.5.*` | n/a |
| 1 | Catalog `[orbrkt]` | `Tests/data.xml` + `campaign/data.xml` | `TDataFile`: L1, production anywhere, use-time 8, size 40 (shuttle still holds crew/food/air/fuel), module operates in orbit, drones can still win |
| 2 | Drone fuel pocket | `alndrn` capacity (or fuel-ignores-capacity) | Unit test: root drone `GET`/`NeedFuel` with `heliu3` |
| 3 | Freeze `gamein.5.xml` | Replace stub with `gameout.5.xml` + seeds | Load: turn 5, `[450]` on Luna, `[000023]` has `orbrkt`, `[117]` has fuel, ≥4 `heliu3` on `[100]` or `[205]` |
| 4 | Orders | Copy player drafts to `orders.5.2.txt` / `orders.5.3.txt` | Files only |
| 5 | Wire `ExecuteTurn5` | Load fixture + orders + `Sequence`; story asserts | Red then green; `[Ignore]` until honest |
| 6 | Goldens | `testreport.6.*` / `gameout.6.xml` after `/player` + human | Un-ignore `ExecuteTurn5` |
| 7 | Merge | Full `Tests.dll` | No ignored SampleGame execute tests |

## Risks

- Factory `[000023]` tech-cap 1: seeding `orbrkt` must replace `armcbt`.
- Shuttle `[117]` must **not** `MOVE` to orbit on week 1, or `USE … FOR 117` is a different location. It **must** `STACK OUT` into R00003 (nested in the city it inherits immobile, same as drones in wreck `[200]`).
- `HAS 1 orbrkt` then `-MOVE O00002` is what lands the haul ~week 10; if `HAS` sees the shuttle’s own modules, delay fails (probe nested type `orbrkt`, not `HAS MODULES`).
- Consume items must fit factory stocks (8 iron / 4 silici / 3 titani) or extra-seed.
- Drones nested in `[200]` are immobile until `STACK OUT`.
- Gelvaren already `attitude enemy` vs Caste; Caste still `DECLARE` so they can `ATTACK`.
- Do not steal combat `Sequence.Ints` for captured-stack names (turn 4 lesson).
- `gamein.3.xml` remains an independent fixture; same rule for `gamein.5.xml` vs later turns.
