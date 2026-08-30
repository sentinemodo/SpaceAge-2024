# Player agent workspace

The **player** Cursor agent (`.cursor/agents/player.md`) reads turn reports and drafts orders. It does not change C#. Live **campaign** catalog and galaxy are owned by **game-designer** (`campaign/`, `designer/`); do not edit those from this agent. SampleGame manuals stay on `Tests/data.xml`. Campaign-ai and campaign play use the campaign L0–L1 excerpt.

| File | Maintained by the player agent |
|------|--------------------------------|
| [rules.md](rules.md) | Implemented order syntax, grouped immediate then long |
| [order_wishlist.md](order_wishlist.md) | Suggested syntax (not in the engine) |
| [basic_technologies.md](basic_technologies.md) | SampleGame catalog techs, then associated module types, then item types (level 0 then 1; `Tests/data.xml`) |
| [campaign/basic_technologies.md](campaign/basic_technologies.md) | Campaign L0–L1 excerpt (`campaign/data.xml`); used by campaign-ai / campaign play |
| [advanced_technologies.md](advanced_technologies.md) | SampleGame catalog techs level 2+, prerequisites diagram, then modules and items (`Tests/data.xml`) |
| [battle.md](battle.md) | Rules of engagement from the live battle loop |
| [technologies_wishlist.md](technologies_wishlist.md) | Suggested techs / balance |
| [drafts/](drafts/) | Order files for a turn |

TDD (`.cursor/rules/csharp-tdd.mdc`) launches this agent for **docs-only** refresh before each commit (`rules.md`, `basic_technologies.md`, `advanced_technologies.md`, `battle.md`), to **write or update orders**, to **check reports** against expected beats, and to **validate golden candidates**. TDD does not draft orders, interpret reports, or replace goldens until this agent matches **and** the human approves.

Outdated copies (do not treat as live): `Game/documentation/Rules.txt`, `Game/documentation/Basics.txt`.
