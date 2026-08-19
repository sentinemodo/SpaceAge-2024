# Player agent workspace

The **player** Cursor agent (`.cursor/agents/player.md`) is the **PBAI** translator: human story/intent → legal `order.*` (precise syntax still allowed). It reads turn reports and drafts orders. It does not change C# or hand-edit `gameout`. Live **campaign** catalog and galaxy are owned by **game-designer** (`campaign/`, `designer/`); do not edit those from this agent. Wishlists here are for `/game-designer` (catalog) and TDD (engine), not for this agent to implement. Manuals below track the catalog the humans are playing (`Tests/data.xml` until a campaign file exists).

| File | Maintained by the player agent |
|------|--------------------------------|
| [rules.md](rules.md) | Implemented order syntax, grouped immediate then long |
| [order_wishlist.md](order_wishlist.md) | Suggested syntax (not in the engine) |
| [basic_technologies.md](basic_technologies.md) | Catalog techs, then associated module types, then item types (level 0 then 1) |
| [advanced_technologies.md](advanced_technologies.md) | Catalog techs level 2+, prerequisites diagram, then modules and items |
| [battle.md](battle.md) | Rules of engagement from the live battle loop |
| [technologies_wishlist.md](technologies_wishlist.md) | Suggested techs / balance |
| [drafts/](drafts/) | Order files for a turn |

TDD (`.cursor/rules/csharp-tdd.mdc`) launches this agent for **docs-only** refresh before each commit (`rules.md`, `basic_technologies.md`, `advanced_technologies.md`, `battle.md`), to **write or update orders**, to **check reports** against expected beats, and to **validate golden candidates**. TDD does not draft orders, interpret reports, or replace goldens until this agent matches **and** the human approves.

Outdated copies (do not treat as live): `Game/documentation/Rules.txt`, `Game/documentation/Basics.txt`.
