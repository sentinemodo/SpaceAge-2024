# Economic persona: story → orders (max grant output)

Use when **## Preference: economic** — same grant bootstrap mechanics as contractor play, but **no contract-first quarter**. The win condition for the next 1–2 quarters is **maximum sustainable output on the home grant** (food, carbon, iron, energy), not chasing open UN give-module CTs unless they are zero-cost side effects.

## Story angle (vs contractor)

| Contractor | Economic |
|------------|----------|
| **## Turn priority** on contract / defence / economy mix | **No Turn priority** — omit CT0026-style focus blocks |
| Quarter sized to CT module delivery | Quarter sized to **grant saturation** (drill → core drill → farms / deep pocket) |
| Scout for convoy routes | Scout **deep pockets** with **moblab + mcored copy** |

## Quarter arc (four strategic bullets)

1. **Energy margin** — `@produce energy` on every `[cplant]`; add `[wndtrb]` / wind plant on factory line if report shows energy deficit before heavy draws.
2. **Paid mcored ASAP** — `grant technology mcored` to `[factry-id]` then `use mcored as newN` with `+get` iron/titani from cargob **this quarter** (bank-funded). Core drill is the force multiplier; delay only if grant Resources truly lack iron/carbon for bootstrap.
3. **Surface loop** — HQ `@produce terran`, farms `@use farmng`, sdrill **one** `@use` matching grant Resources (`hcdril` if carbon/oil; `iminng` if iron — never both; **no `tminng`** unless Resources list titani).
4. **Deep pocket column** — `grant technology msrvtm`, moblab `move` to adjacent **deep pocket of resources detected** exit; stage nested `cdrill` only after energy covers `has 1 cdrill` draw (deactivate extra modules if needed).

Defer UN **town charter** (`twnbld` + transfer to faction 1) until home grant production is **bottlenecked by market or energy**, not because a CT is loud in rumors.

## Tactical objective bullets (turn N)

Copy into story **## Tactical objective** — orders agent reads this block:

- **Focus:** max home-grant output — **ignore open give-module contracts** unless already staged.
- HQ: `set hold 20 terran`, `@produce terran`.
- Factory: `grant technology mcored` (+ `msrvtm` when moblab not yet built) to factry stack id from report.
- Energy before nest: cplant `@produce energy`; expand wind if nested cdrill would brown out the grant.
- Sdrill: single `@use hcdril` or `@use iminng` per Resources line; cargob `@get all food/carbon`, sell food at town buy price.
- **mcored:** `use mcored as newN for <hq-id>` with `+get` metals from cargob — priority build this quarter.
- Moblab: `use msrvtm as newN`, move toward deep-pocket exit, carry mcored copy for field core drills later.
- **Not this quarter:** twnbld charter, CT module convoys, fauna offensives unless contact is immediate.

## Narrative tone

Hard SF operations brief: bank balance, grant Resources line, deep-pocket assays, energy watts — not boardroom contract drama.

Reference stacks: report **Orders template** ids only.
