# Game designer workspace

The **game-designer** Cursor agent (`.cursor/agents/game-designer.md`) owns the **campaign** catalog and galaxy. It does not change C# or `Tests/**`.

| File | Purpose |
|------|---------|
| [xml-schema.md](xml-schema.md) | Tokens `DataFile` currently loads |
| [galaxy.md](galaxy.md) | 10 players, UN + Arbor First/HCS, two occupied starts, Helios–Fomal Alderson pair, eight empty systems |
| [environments.md](environments.md) | Gravity, atmosphere, temperature, launch surcharge |
| [combat-balance.md](combat-balance.md) | Raid sizes, capture-in-10, typed matchups |
| [economy.md](economy.md) | HQ cash/week, nest bills, t=1 UN market books |
| [technology.md](technology.md) | Tech tree through level 10, **Combat matchups** (four weapon groups), module power curve |
| [resources.md](resources.md) | Canonical resource dictionary (seed, rarity, extraction) |
| [catalog.md](catalog.md) | Module types, items, skills, equipment (resources → resources.md) |
| [contracts.md](contracts.md) | In-game contract vectors |
| [engine-wishlist.md](engine-wishlist.md) | Engine gaps (effects, orders, groups, triggers) |

Live XML: `campaign/data.xml`, `campaign/gamein.xml` (generate `gamein.1.xml` from `galaxy.md`).

Test fixtures (`Tests/data.xml`, SampleGame) are **not** the campaign. `/player` manuals currently track the test catalog; after a campaign catalog lands, tell `/player` which file the humans are playing.
