# Military battle doctrine (post-bootstrap turns)

Use with `play/player/personas/all.md` and the faction report **Battles report** + **Orders template**.

## DECLARE fauna

- Declare **only** fauna faction ids that appear as **owners** in battles or hostile rumors **on the home planet** this quarter.
- Arbor packs (14) and Anvil packs (15) are different planets — do not declare both when only one is present.

## Bank under 2000

- HQ: `@produce cash` for upkeep — **not** `@produce terran` (crew upkeep rises).
- Omit repeat `set hold 20 terran` when hold is already configured.

## Fielded tanks (turn 2+)

- **Loot at grant:** `-move` to the HQ/grant region first, then `-give N copper|iron|titani` to cargob. Cross-region GIVE fails while the tank is away from cargob.
- **Never `@give all` / `give all` from tanks** — it transfers terran, oil, and food and disables the unit. Give **metal loot only** in separate `-give` lines.
- Hull damage only: `@repair all` when the report shows **hit points below max**. Supply starvation uses `grant item food/oil`, not repair.
- Damaged tanks: return to grant (`-move R00054` or home exit), repair/resupply there — do not sit in Scree for multiple quarters with low fuel/food.
- New tank moves: prefer `move R…` with **`+get 32 food from cargob`** (and oil) **under** the move line — not `has 1 tanks` conditioned with `-get food` then `-move` (week-1 lesson from 435301).
- Omit redundant `-get` on already crewed tanks; omit repeat `tactic destroy` without a new `-move`.
- Do **not** `-move` into **lost** battle regions until **multiple** tank squads are ready.

## Modules

- Omit `set online true` on stacks that are already online in the Orders template.
- Cover every `#modulestack` id from the template (empty block if idle).
