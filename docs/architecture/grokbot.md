# Grok Bot ideas

Ideas collected for Cursor agents. Do not implement from this file unless explicitly asked by Andrzej.

## Open

- [ ] Technologies for creating underground regions
- [x] GM log: `TurnGmLog` + `/gm-log` on turn runs → `gmturn-log.{turn}.txt` (channels: `all`, `research`, `battles`, `market`; default off). Fauna deferred until growth ships.
- [ ] Visibility system: layered visibility of planets, then regions, then resources, then stacks, then stacks within stacks. Add technologies that improve visibility and technologies that improve stealth.

## Done

- [x] Technologies for building underwater and surface (above-water) cities — see [`play/designer/ocean-cities.md`](../play/designer/ocean-cities.md): pontoon `ptncty`, under-surface `uscty` (no terair), dome `dmdcty` on liquid (still terair), `udrill`, tidal `tdlpln`, `uwtruk`/`uwtank`, undersea resource seeds, stealth + USE seat gate.
- [x] Branch offices enabling recruitment in regions beyond headquarters — L1 tech/module `brnofc` (requires `corpmg`): 1 terran / 2 wk, 20 cash / 2 wk, upkeep 40, no HQ region effects. See `play/designer/economy.md`.
