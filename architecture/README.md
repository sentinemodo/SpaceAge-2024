# SpaceAge-2024 architecture

This folder is the **source of truth** for the SpaceAge engine’s modules, file contracts, technology choices, and **test-layer mapping**. It lives **in this repository** (not the workspace `Architectures/` tree) so cloud agents and git history travel with the code.

Implementation stays in `Game/` and `Tests/`. Architecture documents here are **strategic and slow-changing**. If code and these docs disagree, update the docs (ADR or dated revision) rather than silently diverging.

## How to use

| Reader | Start here |
|--------|------------|
| Implementers / TDD | [`overview.md`](overview.md), then [`modules-and-integrations.md`](modules-and-integrations.md) (test layers) |
| Stack / versions | [`technology.md`](technology.md) |
| Official library docs | [`docs-index.md`](docs-index.md) |
| Why a choice exists | [`adr/`](adr/) |
| Build, test, versioning | [`delivery/cicd-conventions.md`](delivery/cicd-conventions.md) |

Player-facing design notes remain in `Game/documentation/` (`Basics.txt`, `Concepts.txt`, `Rules.txt`). This tree describes the **software** architecture, not the full rulebook.

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
| `delivery/` | Branching, environments, versioning, CI entrypoints |

There is no `cybersecurity/` package yet; add it only if a security review is commissioned.

## Changing the architecture

Prefer a numbered ADR in `adr/` or a dated revision note in the affected file (what changed, why, impact on tests/modules).
