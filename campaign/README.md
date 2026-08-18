# Campaign datafiles

Live game XML for a 10-player PBEM (two starting systems, eight empty). Owned by the **game-designer** agent. Not used by NUnit.

| File | Role |
|------|------|
| `data.xml` | Catalog (copy-extend from `Tests/data.xml`, never the reverse). Present: live baseline plus campaign medical (`sckcns` / `sckbay` / `pharms`; `medtec`/`medirf` tagged research) |
| `gamein.xml` | Factions, galaxy, contracts, starting stacks |

Run the engine against this directory (`Game.exe /data` pointing here) only after both files exist. Until `gamein.xml` exists, `designer/galaxy.md` is the seed spec (United Star Nations, Arbor + Anvil).

Encoding: Windows-1251. Do not replace these files with SampleGame goldens.
