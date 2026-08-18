# ADR-0005: ModuleStack partial files for ownership and upkeep

Date: 2026-08-18  
Status: Accepted

## Context

`ModuleStack` is a large type (over 2000 lines). Architecture forbids splitting a god class without an ADR. SampleGame turn 3 added recursive ownership transfer, nested technology collection, and weekly medical consume/death on that type.

## Decision

Keep a **single** `ModuleStack` type. Split only the new turn-3 helpers into `partial` files:

- `ModuleStack.Ownership.cs` — `SetOwnerRecursive`, `CollectTechnologiesRecursive`
- `ModuleStack.Upkeep.cs` — `ExecuteMedicalConsume` and medical no-consume death

This is the first use of `partial` in the engine. It is a file split only; namespaces, accessibility, and call sites stay the same.

## Consequences

- Do not split the rest of `ModuleStack` (orders, combat, XML, movement) without a new ADR that names the seams.
- New consume or ownership helpers belong in these partials when they are the same concern.
