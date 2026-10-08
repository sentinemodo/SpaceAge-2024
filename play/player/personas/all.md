# Shared persona rules (all AI seats)

Read this file plus your seat `persona.md` and `play/player/rules.md`.

## Story files

- Per-turn stories: `story.<factionId>.<turn>.md` (e.g. `story.9.2.md`).
- Older turn story files stay in the folder for history; draft the **current report turn** only.
- Legacy `story.md` may still exist; ingest uses the latest `story.<id>.<turn>.md` when present.

## Orders quality

- Stack ids from the report **Orders template** only.
- Immediate verbs: lowercase `grant`, `get`, `use`, `move`, `has`, `-get`, `-move`.
- Continuous: `@produce`, `@use`, `@get`, `@repair`, `@give`.
- Military post-bootstrap: see `tools/player-agent/Draft/military-battle-doctrine.md` (`-move` to grant before `-give` loot; never `@give all` from tanks; `move` + `+get food` for provisioning).
- Economic / wind: see `tools/player-agent/Draft/economic-wind-grant.md`.

## Knowledge

- Update `knowledge.md` after each report (`draft-knowledge`).
- Military seats: copy **Observed enemy units** from the Battles report into knowledge for next-turn planning.
