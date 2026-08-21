# Campaign datafiles

Live game XML for a 10-player PBEM (two starting systems, eight empty). Owned by the **game-designer** agent. Not used by NUnit.

| File | Role |
|------|------|
| `data.xml` | Catalog (copy-extend from `Tests/data.xml`, never the reverse). Includes water→fuel/food techs, environment-related settlement modules, combat balance retune |
| `gamein.xml` | Factions, galaxy, contracts, starting stacks (regenerate from design specs; not present on all branches) |

Environment / gravity / temperature rules: `designer/environments.md`. Galaxy water and moon seed rules: `designer/galaxy.md`.

Encoding: Windows-1251. Do not replace these files with SampleGame goldens.
