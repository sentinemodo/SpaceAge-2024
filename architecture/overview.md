# SpaceAge-2024 — architecture overview

Status: Current-state description (engine `0.1.141`)  
Last updated: 2026-08-18

## Purpose

SpaceAge is a **turn-based play-by-email (PBEM) space 4X engine**. A GM (or script) runs a console executable that:

1. Loads static catalog XML and current game state.
2. Reads faction order files for the turn.
3. Advances the world **13 weeks** (one quarter).
4. Writes per-faction text reports (email-shaped headers) and a new game XML snapshot.

The engine does **not** send email, host an API, or use a database. External mailers and GM tools wrap `Game.exe`.

## Concise architecture

Two Visual Studio projects in `SpaceAge.sln`:

| Project | Assembly | Role |
|---------|----------|------|
| `Game` | `Game.exe` | Engine: load → orders → execute → reports → save |
| `Tests` | `Tests.dll` | NUnit 4 fixtures: in-process unit tests and SampleGame golden-file integration |

Domain code lives under `Game/` in a **single namespace** `SpaceAge` (folders are organizational, not namespace boundaries). Almost every entity type exposes a static `All` registry.

## Principles

1. **File-in / file-out batch.** The only integration surface is the filesystem (`data.xml`, `gamein.xml`, `order.*`, `gameout.{turn}.xml`, `report.{turn}.{faction}.*`).
2. **Preserve Windows-1251** on all game XML, orders, and reports (`Encoding.GetEncoding(1251)`).
3. **Do not retarget** off .NET Framework 4.8 or convert to SDK-style projects unless an ADR says so.
4. **Test-first.** New behavior starts in `Tests/` (see test layers in [`modules-and-integrations.md`](modules-and-integrations.md)).
5. **Deterministic turns.** Randomness and generated IDs go through `Sequence`; tests push known values.
6. **Keep diffs small.** Large “modernization” (DI containers, splitting the `SpaceAge` namespace, replacing `*.All`) is out of scope unless requested and recorded as an ADR.

## Current vs design notes

`Game/documentation/Concepts.txt` and `Rules.txt` describe a richer design (officer types, market delivery times, combat superiorities) than the running code. Treat those files as **design intent**. Treat this folder plus the C# as **what the engine actually does**. Gaps (stub `Events`/`Request`, incomplete SampleGame turns 4–5, economy TODOs) are documented as constraints, not as unimplemented product backlog unless an ADR promotes them.

## Glossary

| Term | Meaning |
|------|---------|
| **Turn** | One engine run; calendar quarter. `Game.Date` = year `startingYear` (2020) + `Turn`/4 (integer division), month from `Turn` % 4 (1→January, 2→April, 3→July, 0→September) |
| **Week** | Inner loop 1..13 inside `Game.Execute()` |
| **Faction** | Player corporation: password, email, bank, orders, attitudes |
| **Module stack** | Primary game unit (ship, base, army); nested stacks; owns orders and effects |
| **Order** | Parsed command (`move`, `use`, `produce`, …); immediate vs long; optional `+`/`-` conditions and `@` repeat |
| **Effect** | Multi-week activity attached to a stack or person (produce, move, train, receive) |
| **Catalog (`data.xml`)** | Static types: items, modules, technologies, stars, planets, races, skills |
| **Game state (`gamein` / `gameout`)** | Factions, galaxy graph, saved orders |
| **PBEM** | Players submit orders by email; GM runs the exe; reports returned by email (outside this repo) |

## Risks (architectural)

| Risk | Mitigation in this architecture |
|------|----------------------------------|
| Global `*.All` registries leak between tests | `Game.ClearDictionaries()` in fixture teardown; one game at a time |
| Mono vs real .NET 4.8 CLR differences | Cloud runs the solution under Mono (`.cursor/install.sh`); use a Windows Visual Studio / real-CLR pass for issues that only reproduce there |
| Encoding bugs on non-Windows | `mono-complete` provides code page 1251 on the cloud image; never drop 1251 |
| `DataFile` as a god class | Seams named in [ADR-0006](adr/ADR-0006-datafile-facade-and-xml-seams.md); extract only along those phases. `DataFile` stays the host/test facade |
| Stub pipeline steps (`Request`, `Events`) | Leave no-ops unless a feature requires them; cover with tests when activating |

## What implementers should read first

1. This file.
2. [`modules-and-integrations.md`](modules-and-integrations.md) — turn pipeline, module boundaries, **test layers**.
3. [`technology.md`](technology.md) — versions and “do not upgrade unless asked”.
4. [`delivery/cicd-conventions.md`](delivery/cicd-conventions.md) — how to restore, build, and run tests.
5. Persistence / `DataFile` work: [ADR-0006](adr/ADR-0006-datafile-facade-and-xml-seams.md) before any extract.
