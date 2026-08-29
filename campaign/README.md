# Campaign datafiles

Live game XML for a 10-player PBEM (two starting systems, eight empty). Owned by the **game-designer** agent. Catalog load and `gamein.1.xml` load are covered by `Tests/TCampaign.cs`.

| File | Role |
|------|------|
| `data.xml` | Catalog (copy-extend from `Tests/data.xml`, never the reverse). Includes unused `adpnt` planet type (Gates are `<alderson>`), water→fuel/food techs, environment-related settlement modules, combat balance retune |
| `_gen_gamein.py` | Generator. Run `python campaign/_gen_gamein.py` to rewrite `gamein.1.xml` from `designer/galaxy.md` |
| `gamein.1.xml` | Turn-1 seed: factions 1–13, Helios/Fomal + eight empty systems, Gates, militias, 10 HQs, UN markets, live `give-module`/`research` contracts. Encoding Windows-1251 |

Engine `LoadGame` opens **`gamein.xml`**. Play scripts copy `gamein.1.xml` → the run folder’s `gamein.xml`. Do not point `/data` at `campaign/` (that would write `gameout` into the catalog tree).

**Seeded player passwords** (placeholders until `play/init-run.ps1` assigns run-specific ones):

| Fac | Name | Password |
|-----|------|----------|
| 1 | United Star Nations | (empty, NPC) |
| 2 | Northwind | `northwnd` |
| 3 | Greenwell | `grnwell` |
| 4 | Rivermark | `rivrmrk` |
| 5 | Sundock | `sundock` |
| 6 | Copse | `copse1` |
| 7 | Ironclad | `irnclad` |
| 8 | Oreline | `oreline` |
| 9 | Basalt | `basalt` |
| 10 | Silicate | `silicat` |
| 11 | Fission | `fission` |
| 12 | Arbor First | (empty, NPC) |
| 13 | HCS | (empty, NPC) |

L3+ signature ores from `designer/resources.md` (`lithia`, `reeox`, …) are **not** seeded until those item rows exist in `data.xml` (unknown `resource type` throws on load). Empty systems use live stand-ins (`heliu3`, `nickfe`, `carbon`, `titani`).

Environment / gravity / temperature rules: `designer/environments.md`. Galaxy seed (militias, Alderson Gates): `designer/galaxy.md`.

Do not replace these files with SampleGame goldens.
