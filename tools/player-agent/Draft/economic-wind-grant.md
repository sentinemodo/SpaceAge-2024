# Economic orders — Anvil wind grant (wnplnt)

When the report **Orders template** lists **`[wnplnt]`** (wind power plant) and grant **Resources** have **iron** but **no carbon/oil**:

## Mandatory sdrill rule
- `#modulestack <sdrill-id>`: **`@use iminng` only** — mines iron from the regional deposit.
- **Never `@use hcdril`** on wind grants (hcdril is for Arbor coal/carbon cplant loops).

## Energy
- **`#modulestack <wnplnt-id>`**: **`@produce energy` only** — do not put `use wndtrb` on the wind stack.
- **`#modulestack <factry-id>`**: **`5 use wndtrb for <wnplnt-id>`** with **`+get` iron** from cargob (repeat for a second cdrill coming online). Turn 2+: skip repeat **`use msrvtm`** if moblab already exists; prefer **`use grndtr`** / armor for expansion.

## Moblab (turn 2+)
- **`#modulestack <moblab-id>`** off-grant: **`grant item N oil`** and **`grant item N food`** to the moblab — **no +get oil** from cargob (Anvil grant has no oil drilling; cargob oil is finite).
- Crew already aboard — **no +get terran** for scout moves when report shows moblab crew full.
- Scout **East Peak [R00056]** (anomaly + deep pocket) via **two ground hops** from Mid Spine (e.g. **R00049** then **R00056**); `@research R00056` when on-site.
- Preserve region exits/resources in **`knowledge.md`** before visibility is lost next report.

## Wind batch / SYNCHRO (turn 2+)
- **`#modulestack <factry-id>`:** **`5 use wndtrb for <wnplnt-id>`** + **`+get` iron** only — **no `synchro` on the factory** (SYNCHRO runs immediately in week 1).
- **`#modulestack <wnplnt-id>`:** **`has N wnplnt`** then **`-synchro wind1`** (signal after factory turbine batch).
- **`#modulestack <cdrill-id>`:** bare **`synchro wind1`**, **`-activate 1`**, then **`N use tminng` before `@use iminng`**, **`@give all to cargob`**.

## mcored / cdrill
- Turn 1: **`use mcored as newN`** on factory builds the module. Turn 2+ when **`[cdrill]`** already in Orders template: **do not** `use mcored as newN` again.
- **`use grndtr`:** **`+get 2 iron`** from cargob only (no titani).

## Cargob
- `@get all food from <farms-id>`; `@get all iron from <sdrill-id>` when iminng runs — **not** `@get all carbon from sdrill`.

## mcored (economic persona)
- `grant technology mcored` only when not already paid on report; then `use mcored as newN` with `+get` iron/titani from cargob; nest `has 1 cdrill` only after wind energy margin.

Quality gate: `DescribeWindGrantDrillViolations` and `DescribeDrillUseResourceViolations` reject `@use hcdril` on these grants.
