# ADR-0004: Test layers as namespaces in one Tests project

Date: 2026-08-16  
Status: Accepted (documents existing practice)  
Last updated: 2026-08-18

## Context

TDD guidance elsewhere splits unit / module / integration so CI can gate them independently. This repo has a **single** `Tests` csproj and NUnit 4, with two namespaces already in use.

## Decision

- **Unit** = namespace `UnitTests` (in-process, shared `data.xml` / `gamein.xml`, plus owned copies under `Tests/fixtures/` when a richer world is needed).
- **Integration** = namespace `IntegrationTests` (SampleGame goldens, report comparisons).
- **Module** layer is **not** used.
- Do not add extra test projects or `[Trait]` unless a later ADR introduces CI jobs that need them.
- Default pipeline runs the **whole** `Tests.dll`.
- Tests load **committed** fixtures only. A campaign turn must not write the next turn’s `gamein` (or overwrite a unit fixture) as a side effect of running.

## Consequences

- New tests go to the lowest layer that can prove the behavior (usually `UnitTests`).
- Multi-turn report/XML goldens belong under `Tests/SampleGame/` in `IntegrationTests`. Each live SampleGame test loads a checked-in `gamein.N` (or `gamein.2_contract.xml` for turn 2) and compares generated `*_saved` / report files to committed goldens.
- Unit tests must not `LoadGameDocument` from `Tests/SampleGame/`. If they need a campaign-shaped world, copy it into `Tests/fixtures/` and own it there.
- Filters, if needed: NUnit `--where "namespace == UnitTests"` / `IntegrationTests` (not `dotnet test --filter` as the primary path).

## Revision

- 2026-08-18: Record committed-fixture independence. SampleGame tests no longer `copyFile` turn N output onto turn N+1 input; `_4a` asserts against committed `gamein.2_contract.xml` instead of supplying `_5`’s input in the same run.
