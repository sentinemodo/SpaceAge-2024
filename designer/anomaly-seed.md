# HQ-adjacent regional anomalies (t=1 seed)

Ten **minor** anomalies — one orthogonally adjacent cell per player HQ grant on Arbor/Anvil. Emitted as `<anomaly type="…" description="…"/>` on the **anomaly region**, not on the grant. Turn-1 region reports from the grant append `, anomaly detected` on the matching exit (engine **0.1.160+**).

| Faction | Grant | Anomaly region | `type` | Notes |
|---------|-------|----------------|--------|-------|
| 2 Northwind | Northwind Grant | Mid Vale | spectral | Albedo step on grassland |
| 3 Greenwell | Greenwell Grant | South Ridge | magnetic | Banded-iron subsurface lens |
| 4 Rivermark | Rivermark Grant | East Peak | seismic | Microseismic ridge cluster |
| 5 Sundock | Sundock Grant | East Steppe | gravimetric | Local g anomaly under steppe |
| 6 Copse | Copse Grant | Windgap | spectral | IR line in wind-cut saddle |
| 7 Ironclad | Ironclad Grant | Slope | magnetic | Magnetite talus streak |
| 8 Oreline | Oreline Grant | Mid Spine | radiometric | Pitchblende gamma hot spot |
| 9 Basalt | Basalt Grant | Crag | seismic | Shallow density contrast |
| 10 Silicate | Silicate Grant | Bench | spectral | Altered silicate regolith |
| 11 Fission | Fission Grant | East Peak | radiometric | Uraninite crest dosimeter spike |

Source of truth for ids: `campaign/_gen_gamein.py` (`HQ_ANOMALIES`). Regenerate `gamein.1.xml` after edits.

Investigation payouts by tech band: [`anomaly-investigation.md`](anomaly-investigation.md). Markup here is for **survey hints only** until `INVESTIGATE` lands (wishlist).
