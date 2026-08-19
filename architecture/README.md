# SpaceAge-2024 architecture

This folder is the **source of truth** for the SpaceAge engine’s modules, file contracts, technology choices, **PBAI product loop**, and **test-layer mapping**. It lives **in this repository** (not the workspace `Architectures/` tree) so cloud agents and git history travel with the code.

Implementation stays in `Game/` and `Tests/`. Architecture documents here are **strategic and slow-changing**. If code and these docs disagree, update the docs (ADR or dated revision) rather than silently diverging.

## How to use

| Reader | Start here |
|--------|------------|
| Implementers / TDD | [`overview.md`](overview.md), then [`modules-and-integrations.md`](modules-and-integrations.md) (test layers), then [ADR-0007](adr/ADR-0007-pbai-product-loop.md) for the agent loop |
| Product / PBAI | [`overview.md`](overview.md) (glossary), agent diagram in [`modules-and-integrations.md`](modules-and-integrations.md), [ADR-0007](adr/ADR-0007-pbai-product-loop.md) |
| Stack / versions | [`technology.md`](technology.md) |
| Official library docs | [`docs-index.md`](docs-index.md) |
| Why a choice exists | [`adr/`](adr/) |
| Build, test, versioning | [`delivery/cicd-conventions.md`](delivery/cicd-conventions.md) |

**Player-facing live notes** are `player/` (order syntax, battle, tech manuals, drafts, wishlists). **Campaign / catalog design** is `designer/` then `campaign/`. `Game/documentation/` (`Basics.txt`, `Concepts.txt`, `Rules.txt`) is **historical design intent**, not the PBAI interface and not a substitute for live manuals. This tree describes **software** architecture and the agent loop around the batch engine — not the full rulebook.

Cursor agent briefs (not architecture): `.cursor/agents/player.md`, `.cursor/agents/game-designer.md`. TDD owns C#/tests.

## Layout

| Path | Purpose |
|------|---------|
| `overview.md` | Summary, principles, glossary (including PBAI) |
| `modules-and-integrations.md` | Bounded contexts, file I/O contracts, engine and agent diagrams, test layers |
| `technology.md` | Runtime, libraries, version constraints; target presentation (not in `Game.exe`) |
| `dependencies/` | Package and project dependency map |
| `diagrams/` | Pointers; Mermaid lives in the docs above |
| `docs-index.md` | Canonical URLs, versions, date retrieved |
| `adr/` | Architecture Decision Records |
| `delivery/` | Branching, environments, versioning, CI entrypoints |
| `future-work.md` | Deferred modernization backlog (ADR-gated) |

There is no `cybersecurity/` package yet; add it only if a security review is commissioned.

## Changing the architecture

Prefer a numbered ADR in `adr/` or a dated revision note in the affected file (what changed, why, impact on tests/modules). Persistence / `DataFile` splits: [ADR-0006](adr/ADR-0006-datafile-facade-and-xml-seams.md). Product loop (PBAI wrapping the batch engine): [ADR-0007](adr/ADR-0007-pbai-product-loop.md).

## Revision

- 2026-08-19: PBAI product loop ([ADR-0007](adr/ADR-0007-pbai-product-loop.md)). Live player notes are `player/`, not only `Game/documentation/`.
