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

- Wind grant: before `use mcored` + `has 1 cdrill`, run `@produce energy` on the wnplnt stack and expand with `use wndtrb as newN for <wnplnt-id>`.
- Coal grant: `@produce energy` on cplant after cargob pulls carbon from sdrill `@use hcdril`.

## Military HQ terran

- When bank balance ≥ 5000 and building tanks: `grant item N terran to <hq-id>` so `-get 16 terran` on each `has 1 tanks` block does not drain the starting ~20 crew below zero.

## MOVE

- Ground `move R…` / `-move R…` must target a region id listed under **Exits** from the grant cell (see report). Scout truck: `move <exit>` then `+get` terran/oil/food.

## Economic bootstrap

- After `grant technology msrvtm`, build a moblab: `use msrvtm as newN for <hq-id>` with `+get` iron/titani, then `move` toward a deep metal pocket exit from the grant table.
- Factory: `use mcored as newN` with `+get` from cargob; nest `has 1 cdrill`, `-get 6 terran`, `deactivate 1` when energy allows.

## Verbs

- Immediate quarter: lowercase `grant`, `get`, `use`, `move`, `has`, `-get`, `-move`.
- Continuous: `@produce`, `@use`, `@get`, `@move`, `@research`.
