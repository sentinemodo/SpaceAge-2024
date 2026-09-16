# SpaceAge-2024 architecture

This folder is the **source of truth** for the SpaceAge engine's modules, file contracts, technology choices, and **test-layer mapping**. It lives **in this repository** (not the workspace `Architectures/` tree) so cloud agents and git history travel with the code.

Engine implementation stays in `Game/` and `Tests/`. Open-beta surfaces (`website/`, `game-host/`, `tools/visual-tool/`, `tools/player-agent/`) are documented here and in [`docs/README.md`](../docs/README.md). Architecture documents here are **strategic and slow-changing**. If code and these docs disagree, update the docs (ADR or dated revision) rather than silently diverging.

## How to use

| Reader | Start here |
|--------|------------|
| Implementers / TDD | [`overview.md`](overview.md), then [`modules-and-integrations.md`](modules-and-integrations.md) (test layers) |
| Website developer / tester | [`delivery/website.md`](delivery/website.md), pairing [`.cursor/agents/website-pairing.md`](../.cursor/agents/website-pairing.md), scenarios [`website/e2e/scenarios.md`](../website/e2e/scenarios.md), [ADR-0007](adr/ADR-0007-public-campaign-website.md) |
| Human players | [`play/player/rules.md`](../play/player/rules.md) · [`docs/human/`](../docs/human/) |
| Cursor agents | [`docs/agents/README.md`](../docs/agents/README.md) |
| Stack / versions | [`technology.md`](technology.md) (engine, website, and local player-agent inference are **separate** sections) |
| Local / RunPod player-agent LLM | [ADR-0009](adr/ADR-0009-local-llm-player-agent.md), plan [`delivery/local-player-agent.md`](delivery/local-player-agent.md) |
| Official library docs | [`docs-index.md`](docs-index.md) |
| Why a choice exists | [`adr/`](adr/) |
| Build, test, versioning | [`delivery/cicd-conventions.md`](delivery/cicd-conventions.md) |

Cursor pairing for the lobby: `/website-developer` (`.cursor/agents/website-developer.md`, `.cursor/rules/website-astro.mdc`) and `/website-tester` (`.cursor/agents/website-tester.md`, `.cursor/rules/website-tester.mdc`). File-scoped (`alwaysApply: false`). Contract: [`delivery/website.md`](delivery/website.md). Green Playwright is the done gate — not a browser tour.

Legacy Alderson design text: [`docs/legacy/alderson/`](../docs/legacy/alderson/). Live player rules: [`play/player/rules.md`](../play/player/rules.md). This tree describes **software** architecture.

## Layout

| Path | Purpose |
|------|---------|
| `overview.md` | Summary, principles, glossary |
| `modules-and-integrations.md` | Bounded contexts, file I/O contracts, diagrams, test layers |
| `technology.md` | Runtime, libraries, version constraints |
| `dependencies/` | Package and project dependency map |
| `diagrams/` | Pointers; Mermaid lives in the docs above |
| `docs-index.md` | Canonical URLs, versions, date retrieved |
| `adr/` | Architecture Decision Records |
| `delivery/` | Branching, environments, versioning, CI entrypoints; SampleGame turn 4 checklist [`delivery/samplegame-turn4.md`](delivery/samplegame-turn4.md); campaign play plan [`delivery/campaign-play.md`](delivery/campaign-play.md); **local player-agent LLM** [`delivery/local-player-agent.md`](delivery/local-player-agent.md); public lobby plan [`delivery/website.md`](delivery/website.md) (Phase 4 `/eta` + `/battle`); lobby scenario seed [`delivery/website-scenarios.md`](delivery/website-scenarios.md) |
| `future-work.md` | Deferred modernization backlog (ADR-gated) |

There is no `cybersecurity/` package yet; add it only if a security review is commissioned.

## Changing the architecture

Prefer a numbered ADR in `adr/` or a dated revision note in the affected file (what changed, why, impact on tests/modules). Persistence / `DataFile` splits: [ADR-0006](adr/ADR-0006-datafile-facade-and-xml-seams.md). `ModuleStack` partial seams: [ADR-0008](adr/ADR-0008-modulestack-decomposition.md). Public website: [ADR-0007](adr/ADR-0007-public-campaign-website.md). Local / RunPod player-agent LLM: [ADR-0009](adr/ADR-0009-local-llm-player-agent.md).
