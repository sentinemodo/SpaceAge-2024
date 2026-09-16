# SpaceAge-2024 documentation index

**Engine version:** read `Game/Program.cs` → `EngineVersion` (currently open-beta milestone).

This folder is the **navigation hub** for humans and Cursor agents. Strategic architecture lives in [`architecture/`](architecture/README.md); live player manuals in [`../play/player/`](../play/player/README.md).

## Quick links

| Audience | Start here |
|----------|------------|
| **Human players** | [`human/rules.md`](human/rules.md) — order syntax and live behavior |
| **Implementers** | [`architecture/overview.md`](architecture/overview.md) — current system architecture |
| **Cursor agents** | [`agents/README.md`](agents/README.md) — roster, boundaries, invocation |
| **Campaign GM** | [`../play/README.md`](../play/README.md) (AI isolation) · [`../game-host/README.md`](../game-host/README.md) (hosted beta) |
| **Game design** | [`../play/designer/README.md`](../play/designer/README.md) · [`../play/campaign/`](../play/campaign/README.md) |
| **Legacy / history** | [`legacy/README.md`](legacy/README.md) |

## Module map

| Module | Path | Doc entry |
|--------|------|-----------|
| Engine | `Game/` | [`architecture/modules-and-integrations.md`](architecture/modules-and-integrations.md) |
| Tests | `Tests/` | [`architecture/delivery/cicd-conventions.md`](architecture/delivery/cicd-conventions.md) |
| Campaign lobby | `website/` | [`../website/README.md`](../website/README.md) |
| Visual tool | `tools/visual-tool/` | [`../tools/visual-tool/README.md`](../tools/visual-tool/README.md) · [ADR-0010](architecture/adr/ADR-0010-visual-tool.md) |
| Game host | `game-host/` | [`../game-host/README.md`](../game-host/README.md) · [ADR-0011](architecture/adr/ADR-0011-hosted-game-service.md) |
| Player agent | `tools/player-agent/` | [`../tools/player-agent/README.md`](../tools/player-agent/README.md) · [ADR-0009](architecture/adr/ADR-0009-local-llm-player-agent.md) |
| Docker | `docker/` | [`../docker/README.md`](../docker/README.md) |

## Documentation roles (no duplication)

- **`docs/architecture/`** — strategic SSOT: ADRs, modules, delivery plans, technology constraints.
- **`play/player/`** — live order syntax and tech manuals (maintained by `/player` agent from engine code).
- **`play/designer/`** — campaign design intent (feeds `play/campaign/data.xml`; not live behavior).
- **`docs/agents/`** — agent boundaries and shared contracts (pointers to `.cursor/agents/` specs).
- **`docs/legacy/`** — superseded briefs and Alderson-era design text (not live behavior).
