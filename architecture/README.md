# SpaceAge-2024 architecture

This folder is the **source of truth** for the SpaceAge engine’s modules, file contracts, technology choices, and **test-layer mapping**. It lives **in this repository** (not the workspace `Architectures/` tree) so cloud agents and git history travel with the code.

Engine implementation stays in `Game/` and `Tests/`. The public lobby is a **separate** surface (`website/` when built — see [`delivery/website.md`](delivery/website.md)). Architecture documents here are **strategic and slow-changing**. If code and these docs disagree, update the docs (ADR or dated revision) rather than silently diverging.

## How to use

| Reader | Start here |
|--------|------------|
| Implementers / TDD | [`overview.md`](overview.md), then [`modules-and-integrations.md`](modules-and-integrations.md) (test layers) |
| Website developer / tester | [`delivery/website.md`](delivery/website.md) (**Cursor agents and test pairing**), seed catalog [`delivery/website-scenarios.md`](delivery/website-scenarios.md), [ADR-0007](adr/ADR-0007-public-campaign-website.md) |
| Stack / versions | [`technology.md`](technology.md) (engine and website are **separate** sections) |
| Official library docs | [`docs-index.md`](docs-index.md) |
| Why a choice exists | [`adr/`](adr/) |
| Build, test, versioning | [`delivery/cicd-conventions.md`](delivery/cicd-conventions.md) |

Cursor pairing for the lobby: `/website-developer` (`.cursor/agents/website-developer.md`, `.cursor/rules/website-astro.mdc`) and `/website-tester` (`.cursor/agents/website-tester.md`, `.cursor/rules/website-tester.mdc`). File-scoped (`alwaysApply: false`). Contract: [`delivery/website.md`](delivery/website.md). Green Playwright is the done gate — not a browser tour.

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
| `delivery/` | Branching, environments, versioning, CI entrypoints; SampleGame turn 4 checklist [`delivery/samplegame-turn4.md`](delivery/samplegame-turn4.md); campaign play plan [`delivery/campaign-play.md`](delivery/campaign-play.md); public lobby plan [`delivery/website.md`](delivery/website.md); lobby scenario seed [`delivery/website-scenarios.md`](delivery/website-scenarios.md) |
| `future-work.md` | Deferred modernization backlog (ADR-gated) |

There is no `cybersecurity/` package yet; add it only if a security review is commissioned.

## Changing the architecture

Prefer a numbered ADR in `adr/` or a dated revision note in the affected file (what changed, why, impact on tests/modules). Persistence / `DataFile` splits: [ADR-0006](adr/ADR-0006-datafile-facade-and-xml-seams.md). Public website: [ADR-0007](adr/ADR-0007-public-campaign-website.md).
