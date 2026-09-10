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

**RAG sources** for the approved local/RunPod player-agent stack ([ADR-0009](../architecture/adr/ADR-0009-local-llm-player-agent.md), [delivery plan](../architecture/delivery/local-player-agent.md)): `rules.md`, `battle.md`, tech manuals under `player/`, plus per-faction isolated reports / story / prior orders — not raw `gamein` or other factions’ reports. After engine or catalog changes, refresh these manuals before rebuilding the shared RAG index.

**Local runner (Phase 0+):** [`tools/player-agent/`](../tools/player-agent/README.md) — C# net8 CLI (`dotnet run --project tools/player-agent/PlayerAgent.csproj -- …`). Use `--mode test|campaign`; draft with `--output` or `--run` + `--faction`, or batch all AI seats with [`play/draft-run.ps1`](../play/draft-run.ps1) (`draft-run`) after [`play/ingest-rag.ps1`](../play/ingest-rag.ps1). Cursor `/player` is still the path for one seat or when you want review before files are written. Use `ingest-faction --story-only` when `/campaign-ai` updates `story.md`. After engine or catalog changes, refresh these manuals first, then run [`play/refresh-shared-rag.ps1`](../play/refresh-shared-rag.ps1) (or `refresh-shared`) before the next AI draft batch.
