# Campaign datafiles

Live game XML for a 10-player PBEM (two starting systems, eight empty). Owned by the **game-designer** agent. Catalog load and `gamein.1.xml` load are covered by `Tests/TCampaign.cs`.

| File | Role |
|------|------|
| `data.xml` | Catalog (copy-extend from `Tests/data.xml`, never the reverse). Includes unused `adpnt` planet type (Gates are `<alderson>`), water→fuel/food techs, environment-related settlement modules, combat balance retune |
| `_gen_gamein.py` | Generator. Run `python campaign/_gen_gamein.py` to rewrite `gamein.1.xml` from `designer/galaxy.md` |
| `gamein.1.xml` | Turn-1 seed: factions 1–13, Helios/Fomal + eight empty systems, Gates, militias, 10 HQs, UN markets, live `give-module`/`research` contracts. Encoding Windows-1251 |

Engine `LoadGame` always opens **`/data/gamein.xml`**. Play and TDD copy `campaign/gamein.1.xml` → run `/data/gamein.xml` and `campaign/data.xml` → run `/data/data.xml`. Never point `/data` at `campaign/` (that would write `gameout` into the catalog tree). Never put `gamein.xml` in `/turn-dir` (`/turn-dir` is `order.*` in and `report.*` out).

**Reports-only** (`Game.exe /data <run>/data /turn-dir <run>/turn /reports`): load + `GenerateReports`, no `Execute`. Seed `turn="1"` therefore writes starting `report.1.{faction}.txt` into `/turn-dir` (plus `report.1.{faction}.xml` when that faction’s `xml-report` is true). A full turn run would `turn++` first and emit `report.2.*` plus `gameout.2.xml` — that is not this path.

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

The **public lobby website** (human-facing home, visual-tool link, current-turn orders status) is specified under `architecture/delivery/website.md` and tracked as a campaign-play todo in `architecture/delivery/campaign-play.md`. It is not this catalog tree and is not the `play/runs` AI isolation path.

Do not replace these files with SampleGame goldens.
