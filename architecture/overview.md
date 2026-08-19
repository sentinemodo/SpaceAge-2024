# SpaceAge-2024 — architecture overview

Status: Current-state description (engine `0.1.141`) plus PBAI product direction  
Last updated: 2026-08-19

## Purpose

SpaceAge is a **play-by-AI (PBAI)** product wrapped around a **PBEM-shaped** turn engine. Humans interact primarily by **story / intent**. The **player agent** (`/player`) translates that into legal `order.*` files. The C# console executable remains the **only** turn processor:

1. Loads static catalog XML and current game state.
2. Reads faction order files for the turn.
3. Advances the world **13 weeks** (one quarter).
4. Writes per-faction text reports (email-shaped headers) and a new game XML snapshot.

The engine does **not** send email, host an API, use a database, or ask an LLM to decide turn outcomes. Precise order syntax stays a valid path (humans, tests, TDD). An external mailer may still wrap the same files (**PBEM** as a compatible mode). Visualization of the world is a **planned** read-only layer, not present in the engine.

## Concise architecture

Two Visual Studio projects in `SpaceAge.sln` today:

| Project | Assembly | Role |
|---------|----------|------|
| `Game` | `Game.exe` | Engine: load → orders → execute → reports → save |
| `Tests` | `Tests.dll` | NUnit 4 fixtures: in-process unit tests and SampleGame golden-file integration |

Around the engine (same repo, not extra assemblies yet): `/player` and `/game-designer` Cursor agents; target **visualization** as a future module/folder or project. Domain code lives under `Game/` in a **single namespace** `SpaceAge` (folders are organizational, not namespace boundaries). Almost every entity type exposes a static `All` registry.

## Principles

1. **File-in / file-out batch (engine).** The only *engine* integration surface is the filesystem (`data.xml`, `gamein.xml`, `order.*`, `gameout.{turn}.xml`, `report.{turn}.{faction}.*`). PBAI and visualization wrap this surface; they do not replace it.
2. **Preserve Windows-1251** on all game XML, orders, and reports (`Encoding.GetEncoding(1251)`).
3. **Do not retarget** off .NET Framework 4.8 or convert to SDK-style projects unless an ADR says so.
4. **Test-first.** New behavior starts in `Tests/` (see test layers in [`modules-and-integrations.md`](modules-and-integrations.md)).
5. **Deterministic turns.** Randomness and generated IDs go through `Sequence`; tests push known values.
6. **Keep diffs small.** Large “modernization” (DI containers, splitting the `SpaceAge` namespace, replacing `*.All`) is out of scope unless requested and recorded as an ADR.
7. **LLMs do not execute turns.** Agents translate intent, design catalog/galaxy, or write tests. Only `Game.exe` advances the world. Agents never hand-edit `gameout` as a substitute for a batch run.

## Current vs design notes

`Game/documentation/Concepts.txt` and `Rules.txt` describe a richer design (officer types, market delivery times, combat superiorities) than the running code. Treat those files as **historical design intent**. Treat **`player/`** as live order/tech/battle manuals. Treat **`designer/`** / **`campaign/`** as live catalog and galaxy design. Treat this folder plus the C# as **what the engine actually does**. Gaps (stub `Events`/`Request`, incomplete SampleGame turns 4–5, economy TODOs) are documented as constraints, not as unimplemented product backlog unless an ADR promotes them.

PBAI vs PBEM: [ADR-0007](adr/ADR-0007-pbai-product-loop.md). Engine file-batch: [ADR-0003](adr/ADR-0003-filesystem-pbem-batch.md).

## Glossary

| Term | Meaning |
|------|---------|
| **Turn** | One engine run; calendar quarter. `Game.Date` = year `startingYear` (2020) + `Turn`/4 (integer division), month from `Turn` % 4 (1→January, 2→April, 3→July, 0→September) |
| **Week** | Inner loop 1..13 inside `Game.Execute()` |
| **Faction** | Player corporation: password, email, bank, orders, attitudes |
| **Module stack** | Primary game unit (ship, base, army); nested stacks; owns orders and effects |
| **Order** | Parsed command (`move`, `use`, `produce`, …); immediate vs long; optional `+`/`-` conditions and `@` repeat |
| **Intent** | Natural-language story of what the faction should do this turn, not order syntax |
| **PBAI** | Play-by-AI: humans state intent; `/player` drafts legal orders; `Game.exe` executes the turn |
| **Player agent** | Cursor `/player`: reads reports and `player/` manuals; writes legal `order.*` or syntax/tech wishlists |
| **Designer agent** | Cursor `/game-designer`: owns `designer/` then `campaign/`; engine gaps go to `designer/engine-wishlist.md` |
| **Visualization** | Target read-only (or near-read-only) presentation of engine artifacts; not a rules engine; **not present** yet |
| **Effect** | Multi-week activity attached to a stack or person (produce, move, train, receive) |
| **Catalog (`data.xml`)** | Static types: items, modules, technologies, stars, planets, races, skills |
| **Game state (`gamein` / `gameout`)** | Factions, galaxy graph, saved orders |
| **PBEM** | Compatible mode: players/GM exchange orders and reports by email via an **external** mailer; not the product default |

## Risks (architectural)

| Risk | Mitigation in this architecture |
|------|----------------------------------|
| Global `*.All` registries leak between tests | `Game.ClearDictionaries()` in fixture teardown; one game at a time |
| Mono vs real .NET 4.8 CLR differences | Cloud runs the solution under Mono (`.cursor/install.sh`); use a Windows Visual Studio / real-CLR pass for issues that only reproduce there |
| Encoding bugs on non-Windows | `mono-complete` provides code page 1251 on the cloud image; never drop 1251 |
| `DataFile` as a god class | Seams named in [ADR-0006](adr/ADR-0006-datafile-facade-and-xml-seams.md); extract only along those phases. `DataFile` stays the host/test facade |
| Stub pipeline steps (`Request`, `Events`) | Leave no-ops unless a feature requires them; cover with tests when activating |
| LLM invents order verbs or turn results | [ADR-0007](adr/ADR-0007-pbai-product-loop.md): wishlists only; engine is the sole executor |
| Visualization becomes a second engine | Keep viz out of `Game.exe`; consume files; no combat/movement reimplementation |

## What implementers should read first

1. This file.
2. [ADR-0007](adr/ADR-0007-pbai-product-loop.md) — PBAI loop, ownership, what must not change.
3. [`modules-and-integrations.md`](modules-and-integrations.md) — turn pipeline, module boundaries, agent diagram, **test layers**.
4. [`technology.md`](technology.md) — versions and “do not upgrade unless asked”; target presentation is not in the engine.
5. [`delivery/cicd-conventions.md`](delivery/cicd-conventions.md) — how to restore, build, and run tests.
6. Persistence / `DataFile` work: [ADR-0006](adr/ADR-0006-datafile-facade-and-xml-seams.md) before any extract.

## Revision

- 2026-08-19: Product purpose is PBAI wrapping the existing batch engine ([ADR-0007](adr/ADR-0007-pbai-product-loop.md)). Glossary: PBAI, intent, player/designer agents, visualization. Principle: LLMs do not execute turns.
