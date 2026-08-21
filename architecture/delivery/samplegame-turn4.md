# SampleGame turn 4 — feature track

Last updated: 2026-08-19  
Engine cited: `0.1.141`

Turns 1–3 already ship MOVE, USE, CAPTURE, wreckage RESEARCH, and standing BUY/SELL. This PR **lands new engine behavior** and locks it with a Sol golden. It is not a replay of “another ship to Luna.”

Do **not** implement `USE` produce-effect `repair` (catalog `[repair]`). That path is replaced by the **`REPAIR`** order.

## Vehicle

1. **First commit:** this file. No C#.
2. **Later commits:** one row per commit. Stay green before the next phase.
3. New goldens need `/player` validation, a ±10-line hunk in chat, and explicit human approval.
4. Bump `EngineVersion` only in the commit that changes player-visible reports or committed SampleGame goldens.
5. Independent fixture (ADR-0004): do not `copyFile` turn-3 output onto turn 4.

## New vs already coded

| Feature | Engine today | Turn 4 job |
|---------|--------------|------------|
| **RESEARCH grants a technology** | `ResearchOrder.Execute`: `RollBreakthrough` → `ReceiveTechnologyCopy`. Unit-tested. SampleGame never got a breakthrough (Gelvaren `[116]` leftover `@research tag repair`, 11 RP). | **Prove:** seed `Sequence` with `0`; assert `TechnologiesToShow` / stack copy. |
| **COPY** | Parses and executes (`ReceivingTechnology`). Unused in SampleGame. `COPY all` still TODO — not this PR. | **Prove:** `COPY alnfgh TO <stack>` (or another wreck tech), same location, capacity ok. |
| **TRANSFER as text** | `TransferOrder.Parse` understands `TRANSFER <n> TO <id>`. `OrdersReader` has no `case "transfer"`. XML load already works. | **Implement:** add the switch; unit test text parse vs XML. |
| **GenerateOffers** | `Game.GenerateOffers` is empty. `TMarket.ProcessGenerateAutoOffers` is `[Ignore]` + `Assert.Fail`. Standing BUY/SELL already process. | **Implement:** NPC/neutral cities auto-list available inventory (no simultaneous buy+sell of the same type). Un-ignore that test. |
| **TRAIN leftover reconnect** | `TrainOrder` starts `TrainingOfficer` / `TrainingSkill`. Leftover `TRAIN` does not reattach (unlike leftover `USE`). | **Implement:** leftover pattern like USE. Prove duration continues after reload. |

`UpdateRates` stays stubbed. `ExecuteTurn5`, JUMP, typed weapons, `USE` repair-effect: out of this PR.

## Faction intents

State: `Tests/SampleGame/testreport.4.{1,2,3}.txt` (briefing after ExecuteTurn3).

- **NPC `[1]`** — Auto-offers from Berlin/Sydney inventory. Thin `orders.4.1.txt` only if leftovers must stay explicit.
- **Caste Prime `[2]`** — Do not re-claim wreck `[200]`. COPY a wreck tech. TRANSFER drones `[207]` (or another nested module) onto hull `[101]` or shuttle `[100]`. TRAIN at library `[115]` or CEO skill. Earth leftovers only.
- **Gelvaren `[3]`** — Keep `@research tag repair` on `[116]`. Breakthrough must fire this quarter. COPY the granted tech onto factory `[000023]` if needed. Earth war with existing CAPTURE/MOVE. Shuttle `[117]` launch is optional color.

No wreck seed/`dmecns` inject unless COPY/TRANSFER has nothing legal to move; drones already exist.

## Commits

| # | Phase | Production | Prove with |
|---|--------|------------|------------|
| 0 | Docs | This checklist. No C#. | n/a |
| 1 | TRANSFER text | `case "transfer"` in `OrdersReader` | UnitTests: `TRANSFER n TO id` matches XML |
| 2 | GenerateOffers | Fill `Game.GenerateOffers` | Un-ignore `ProcessGenerateAutoOffers` |
| 3 | TRAIN leftover | Reconnect leftover `TRAIN` to `Training*` | Unit test save/reload duration continues |
| 4 | Freeze `gamein.4.xml` | Replace turn-36 stub with `gameout.4.xml` | Load: turn 4, `[200]` owned by 2, `[207]` present, `[116]` has RP |
| 5 | Orders | `/player` writes `orders.4.2.txt` / `orders.4.3.txt` | Files only |
| 6 | Wire `ExecuteTurn4` | Load fixture + orders + `Sequence`; story asserts | Red then green; `[Ignore]` until honest |
| 7 | Goldens | `testreport.5.*` / `gameout.5.xml` after `/player` + human | Un-ignore `ExecuteTurn4` |
| 8 | Merge | Full `Tests.dll` | Only `ExecuteTurn5` stays ignored among SampleGame executes |

## Risks

- Breakthrough uses cheapest available tech; push one `Sequence` `0` per weekly output point.
- COPY fails if receiver tech-capacity is full.
- GenerateOffers must not rewrite standing offers that lock turns 1–3 goldens.
- TRAIN officer duration may exceed 13 weeks; leftover reconnect is the feature even if completion is turn 5.
