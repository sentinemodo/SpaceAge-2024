# Campaign datafiles

Live game XML for a 10-player PBEM (two starting systems, eight empty). Owned by the **game-designer** agent. Not used by NUnit.

| File | Role |
|------|------|
| `data.xml` | Catalog (copy-extend from `Tests/data.xml`, never the reverse). Includes `adpnt` planet type, water→fuel/food techs, environment-related settlement modules, combat balance retune |
| `gamein.xml` / `gamein.1.xml` | Factions 1–13, galaxy, contracts, starting stacks (regenerate from `designer/galaxy.md`; not present yet). Engine `LoadGame` opens **`gamein.xml`**; play scripts copy `gamein.1.xml` → `gamein.xml` |

Environment / gravity / temperature rules: `designer/environments.md`. Galaxy seed (militias, Alderson Gates): `designer/galaxy.md`.

Encoding: Windows-1251. Do not replace these files with SampleGame goldens.
