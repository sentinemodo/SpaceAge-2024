# Order draft quality (AI seat turn 1)

Compact do/don't for grant-bootstrap orders. Stack ids must come from the report Orders template.

## GRANT

- GRANT `item` / `technology` targets must be **numeric modulestack ids** (e.g. `250005`), not aliases like `factory` or `cargob`.
- GRANT may appear under `#faction` or under `#modulestack`.
- Pay for factory tech copies before USE: `grant technology mcored`, `grant technology msrvtm`, `grant technology armcbt` to the **factry** stack id.

## Drill `@use` (one tech per sdrill stack)

- Read **Resources** on the grant region in the report.
- `@use hcdril` only if Resources list **carbon** or **oil** (Arbor / coal path).
- `@use iminng` only if Resources list **iron** (Anvil / metals path).
- **Never** put `@use hcdril` and `@use iminng` on the same sdrill — the engine runs hydrocarbons first and iminng never executes.
- Anvil wind grants (`wnplnt`): use `@use iminng` on sdrill, **not** `@use hcdril`; energy from `#modulestack <wnplnt-id>` `@produce energy`.

## Energy before nested modules

- Wind grant turn 1: before `use mcored` + `has 1 cdrill`, `@produce energy` on wnplnt and `5 use wndtrb for <wnplnt-id>` on **factory** with `+get` iron.
- Wind grant turn 2+ (cdrill already fielded): factory **`5 use wndtrb`** only — **no `synchro` on factory**; wnplnt stack **`has N wnplnt` / `-synchro wind1`**; cdrill stack **`synchro wind1` / `-activate 1`**, then **`N use tminng` before `@use iminng`**. `use grndtr`: **`+get 2 iron`** only.
- Coal grant: `@produce energy` on cplant after cargob pulls carbon from sdrill `@use hcdril`.

## Military HQ terran

- When bank balance ≥ 5000 and building tanks: `grant item N terran to <hq-id>` so `-get 16 terran` on each `has 1 tanks` block does not drain the starting ~20 crew below zero.
- Turn 2+ with fielded tanks and bank **under 2000**: HQ `@produce cash` (not `@produce terran`); omit repeat `set hold 20 terran`.
- DECLARE only fauna ids **on this planet** (Battles report owners). Post-battle: `-move` to grant **before** `-give` loot to cargob; **never `@give all`** from tanks (use `-give N copper|iron|titani` only). `@repair all` only when hull HP below max. See `Draft/military-battle-doctrine.md`.

## Story files

- Campaign stories: `story.<factionId>.<turn>.md`; keep prior turn files; ingest embeds all turn story files for the seat.

## MOVE

- Ground `move R…` / `-move R…` must target a region id listed under **Exits** from the grant cell (see report). Scout truck: `move <exit>` then `+get` terran/oil/food.

## Economic bootstrap

- After `grant technology msrvtm`, build a moblab: `use msrvtm as newN for <hq-id>` with `+get` iron/titani, then `move` toward a deep metal pocket exit from the grant table.
- Factory: `use mcored as newN` with `+get` from cargob; nest `has 1 cdrill`, `-get 6 terran`, `deactivate 1` when energy allows.

## Verbs

- Immediate quarter: lowercase `grant`, `get`, `use`, `move`, `has`, `-get`, `-move`.
- Continuous: `@produce`, `@use`, `@get`, `@move`, `@research`.

## Orders template coverage

- Every numeric `#modulestack <id>` in the report **Orders template** must appear once in the order file.
- Use an **empty block** (header only) when that stack has no verbs this quarter (e.g. idle cdrill on grant).
- Do not emit `#person` CEO blocks unless issuing person-only verbs.

## Faction knowledge

- After each report, update `knowledge.md` (region exits, resources, scout paths, moblab fuel) via `player-agent draft-knowledge`.
- **Military:** include **Observed enemy units** from the **Battles report** (fauna stack ids, module types, HP/atk/def, outcomes) for next-turn move/tactic planning.
- Next-turn story/order drafting ingests `knowledge.md` with `story.md`.
