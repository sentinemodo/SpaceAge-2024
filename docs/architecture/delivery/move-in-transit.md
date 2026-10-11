# Move in transit — design plan

## Problem

Reports and order templates show only vague movement (`will move for another N weeks before next destination`) without **origin** and **destination** region id/name. Players cannot see the two meaningful choices mid-trip: continue to the forward region or **move back** to the origin with a shorter remaining time.

## Goals

1. **Reporting & orders template** — While moving, show origin and destination (name + id), total leg duration, and weeks remaining (owner and observers).
2. **Continue by default** — No new `move` line next turn continues travel toward the current destination (existing persisted `MoveOrder`).
3. **Move back** — `move back` or `move <origin-region-id>` reverses toward the origin; remaining time equals weeks already traveled on the current leg (`initial − durationLeft` after each week’s tick).
4. **Symmetric bookkeeping** — Persist `return-leg-weeks` on the `Moving` effect: increment when decreasing forward `durationLeft`, decrement when decreasing on the return leg (mirror of elapsed).
5. **Mid-trip move constraints** — While `MovingTo != null`, new `move` may only target the current forward destination or the recorded origin; any other destination fails with an event report.
6. **Effective region** — For immediate orders, unit is in **origin** if elapsed &lt; half of `InitialDuration`, else in **destination** region (by time, not map position). Parent location stays origin until arrival.
7. **Give/get while traveling** — Silent fail: no event, `Executed` stays false.
8. **Other long orders** — Blocked while in transit (no execution, not marked executed).
9. **Parse** — `move back` as first-class syntax.

## Data model (`Moving` effect)

| Field | Purpose |
|-------|---------|
| `Origin` | Departure holder when current leg started |
| `Destination` | Current leg target (`MovingTo`) |
| `InitialDuration` | Weeks for current leg at leg start |
| `Duration` | Weeks remaining (existing) |
| `ReturnLegWeeks` | Weeks traveled from origin on forward leg; used for reverse duration |

XML: `origin`, `destination`, `move-mode`, `initial-duration`, `return-leg-weeks` (plus existing `duration`).

## `MoveOrder` changes

- On depart: set `Moving.Origin`, sync `ReturnLegWeeks = 0` for new forward leg.
- Each moving week: adjust `ReturnLegWeeks` (+1 forward, −1 on return leg).
- `ApplyInTransitRedirect(week, newDest)` when already moving.
- `Report()`: in-progress lines include `from <origin> to <dest>` and remaining weeks.

## Tests (`Tests/TMoveInTransit.cs`)

- Report/effect text includes origin and destination ids.
- Partial trip → `move back` → arrives origin in elapsed weeks.
- Continue without re-ordering reduces ETA.
- Invalid third region fails.
- Get/give silent fail in transit.
- Produce (or train) long order does not run in transit.
- Effective region flips at half duration (via order that cares about location, or direct helper assertion).
- Observer report includes travel timing (same effect visibility).

## Docs (before commit)

`/player` sync: `docs/human/rules.md`, `play/player/rules.md`, allowlist if `move back` is new token.

## Implementation order

1. `Moving` + XML + description/report
2. `ModuleStack` travel helpers (`IsInTransit`, `TravelEffectiveRegion`, give/get guards)
3. `MoveOrder` leg bookkeeping + redirect + report
4. `Orders` long-order guard
5. `OrdersReader` merge in-progress `MoveOrder`
6. Rules docs + tests green
