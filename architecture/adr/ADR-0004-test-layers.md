# ADR-0004: Test layers as namespaces in one Tests project

Date: 2026-08-16  
Status: Accepted (documents existing practice)

## Context

TDD guidance elsewhere splits unit / module / integration so CI can gate them independently. This repo has a **single** `Tests` csproj and NUnit 4, with two namespaces already in use.

## Decision

- **Unit** = namespace `UnitTests` (in-process, shared `data.xml` / `gamein.xml`).
- **Integration** = namespace `IntegrationTests` (SampleGame goldens, report comparisons).
- **Module** layer is **not** used.
- Do not add extra test projects or `[Trait]` unless a later ADR introduces CI jobs that need them.
- Default pipeline runs the **whole** `Tests.dll`.

## Consequences

- New tests go to the lowest layer that can prove the behavior (usually `UnitTests`).
- Multi-turn report/XML goldens belong under `Tests/SampleGame/` in `IntegrationTests`.
- Filters, if needed: NUnit `--where "namespace == UnitTests"` / `IntegrationTests` (not `dotnet test --filter` as the primary path).
