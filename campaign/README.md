# Campaign datafiles

Live game XML for a 10-player **campaign** (PBAI default; same files remain PBEM-compatible). Two starting systems, eight empty, **Alderson chokepoints** at Cinder and Shards. Owned by the **game-designer** agent. Not used by NUnit.

| File | Role |
|------|------|
| `data.xml` | Live catalog plus `adpnt` planet type, campaign medical, and seed item ids (`nickfe` `lithia` `nitrat` …). L3–L10 campaign techs remain spec-only in `designer/technology.md` |
| `gamein.xml` | Turn 1: factions 1–11, 10 systems, full Arbor+Anvil grids, 16 Alderson planet-objects, landing stubs, condensed empty systems |

Starmap and AP pair table: `designer/starmap.md`. Region maps: `designer/galaxy.md`.

Encoding: Windows-1251 (ASCII body). Do not replace these files with SampleGame goldens. Regenerator: `_gen_gamein.py`.
