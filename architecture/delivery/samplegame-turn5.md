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

- **Gelvaren** shuttle `[117]` leaves Southern Hemisphere `[R00003]`, hops to **Earth orbit `[O00002]`**, builds an **orbital complex** with level-0 `[orassm]`, then an **orbital rocket launcher** `[orbrkt]` nested on that complex.
- **Caste Prime** fuels alien drones `[450]`, leaves the immobile wreck `[200]` on Luna, and returns hull `[101]` plus the drones to **Earth orbit `[O00002]`**.
- Both fight in space. Intended outcome: **drones victory**.

Player drafts (live verbs only): `player/drafts/orders.5.2.txt`, `player/drafts/orders.5.3.txt`. Copy into `Tests/SampleGame/orders.5.*.txt` only after the fixture freeze.

## New vs already coded

| Feature | Engine today | Turn 5 job |
|---------|--------------|------------|
| **Orbital rocket launcher catalog** | Live `[rckter]` makes infantry item `[rctlnc]`. `[gunplc]` / `[laztrt]` only operate on solid ground. Campaign `[mslpod]`/`[msltub]` is not in `Tests/data.xml`. | **Implement:** L1 tech `[orbrkt]` (orbit `use-allowed-in` production) producing military module `[orbrkt]` that operates in **orbit** (and nested on `space station` / `frigate`). Seed a copy on Gelvaren / shuttle `[117]`. Twin into `campaign/data.xml`. |
| **Shuttle orbit USE time** | Shuttle `<use location-type="orbit" efficiency-multiplier="10"/>`. `UseOrder` does `UseTime * multiplier` → `[orassm]` is **50 weeks**. | **Fix:** set shuttle multiplier to `1` (or invert the formula with a unit test). Shuttle must finish `orassm` then `orbrkt` inside one 13-week quarter after a 1-week hop. |
| **Drone helium fuel as a root** | `[alndrn]` `capacity="0"`. `NeedFuel` reads `RootModuleStack.ItemStacksSumRecursive`. Nested in wreck `[200]`, helium on `[205]` would count; nested drones **inherit wreck immobile**. `STACK OUT` makes them mobile but they cannot `GET` helium. | **Implement:** give drones enough capacity to hold fuel (1 `heliu3` per drone) **or** let fuel items ignore capacity. Seed ≥4 `heliu3` on shuttle `[100]` or cargo `[205]` — `he3min` is 8 weeks per 1 unit, so mining cannot fuel four drones this quarter. |
| **Space battle at a named orbit** | Co-located `ATTACK` / `TACTIC` already work. Hangar launch is battle-only (no `LAUNCH` verb). | **Prove:** both sides at `[O00002]` this quarter; drones as **roots** (drafts `STACK OUT` so they fire round 1). |
| **Freeze `gamein.5.xml`** | File is still the turn-36 stub (Mercury, old frigate/station). | **Replace** with committed `gameout.5.xml`, then seed `orbrkt`, shuttle fuel (`uraniu`/`h2o2`), helium, and replace `[servic]` on `[117]` (tech-cap 1). |

`JUMP`, typed `weapon-group` vs `resists`, item `attack` in `ModuleStack.Attack`, `TRANSFER ALL`, medical-facility heal, and `USE repair`: out of this PR.

## Wishlist scan (not this PR)

| Source | Candidate | Why not turn 5 |
|--------|-----------|----------------|
| `player/order_wishlist.md` | `TRANSFER ALL` / pick damaged module | Drafts use `STACK OUT` + `GIVE`/`GET`. |
| `player/order_wishlist.md` | `REPAIR` without `CanOperate` | Captured Sydney guns are leftovers; not the space beat. |
| `player/technologies_wishlist.md` | Wire `USE repair` / `[medfac]` | Explicitly out since turn 4. |
| `designer/engine-wishlist.md` | Typed weapons, shield intercept, armour hit-bias | Too large; Battle stays flat. |
| `designer/engine-wishlist.md` | Item `attack`/`damage` in battle (`rctlnc`) | Infantry rockets, not orbital `[orbrkt]`. |
| `designer/engine-wishlist.md` | Shuttle launch fuel surcharge / land bans | Would block `[117]` unless extra `h2o2` is seeded; skip. |
| `designer/engine-wishlist.md` | Space transit ΔAU × drive speed | Luna→Earth must complete **this quarter**. Keep ETA 0/1. |
| `designer/engine-wishlist.md` | Moon `@name` / `loadGalaxyExits` | Hull already space-hops without matching exits. |

## Faction intents

State: `Tests/SampleGame/testreport.5.{1,2,3}.txt` (briefing after ExecuteTurn4).

- **NPC `[1]`** — Thin `orders.5.1.txt` only if leftovers must stay explicit. Auto-offers already run.
- **Caste Prime `[2]`** — Do not fly wreck `[200]`. Seed helium; `STACK OUT` drones; `MOVE` drones `O00001` then `O00002`; hull `[101]` `MOVE O00002`; `TACTIC destroy`; `ATTACK` Gelvaren shuttle/complex. Earth leftovers only.
- **Gelvaren `[3]`** — Seed `[orbrkt]` on `[117]` (drop `[servic]`). Crew + iron + fuel, `STACK OUT`, `MOVE O00002`, `-use orassm as new140`, `--use orbrkt as new141 for new140`. `ATTACK` drones/hull. Keep Earth war leftovers.

## Commits

| # | Phase | Production | Prove with |
|---|--------|------------|------------|
| 0 | Docs | This checklist + `player/drafts/orders.5.*` | n/a |
| 1 | Catalog `[orbrkt]` | `Tests/data.xml` + `campaign/data.xml` | `TDataFile`: level, orbit use, module operates in orbit, attack/damage vs drones (drones can still win) |
| 2 | Shuttle orbit USE | multiplier `1` (or formula invert) | Unit test: shuttle `orassm` duration ≤ 5 weeks, not 50 |
| 3 | Drone fuel pocket | `alndrn` capacity (or fuel-ignores-capacity) | Unit test: root drone `GET`/`NeedFuel` with `heliu3` |
| 4 | Freeze `gamein.5.xml` | Replace stub with `gameout.5.xml` + seeds | Load: turn 5, `[450]` on Luna, `[117]` has `orbrkt` + fuel, ≥4 `heliu3` on `[100]` or `[205]` |
| 5 | Orders | Copy player drafts to `orders.5.2.txt` / `orders.5.3.txt` | Files only |
| 6 | Wire `ExecuteTurn5` | Load fixture + orders + `Sequence`; story asserts | Red then green; `[Ignore]` until honest |
| 7 | Goldens | `testreport.6.*` / `gameout.6.xml` after `/player` + human | Un-ignore `ExecuteTurn5` |
| 8 | Merge | Full `Tests.dll` | No ignored SampleGame execute tests |

## Risks

- `[117]` tech-cap 1: seeding `orbrkt` must replace `servic` or COPY fails.
- Conditioned `--use orbrkt as new141 for new140` waits until `orassm` completes; if shuttle multiplier stays 10, both USEs miss the quarter.
- Drones nested in `[200]` are immobile until `STACK OUT`.
- Gelvaren already `attitude enemy` vs Caste; Caste still `DECLARE` so they can `ATTACK`.
- Do not steal combat `Sequence.Ints` for captured-stack names (turn 4 lesson).
- `gamein.3.xml` remains an independent fixture; same rule for `gamein.5.xml` vs later turns.
