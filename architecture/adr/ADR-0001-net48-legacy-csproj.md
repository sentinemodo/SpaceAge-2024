# ADR-0001: Stay on .NET Framework 4.8 and legacy csproj

Date: 2026-08-16  
Status: Accepted (documents existing practice)

## Context

The engine is a long-lived C# console application with non-SDK `.csproj` files targeting `v4.8`. Cursor Cloud agents run on Ubuntu and execute the same solution under **Mono**.

## Decision

Keep **.NET Framework 4.8** and **legacy csproj**. Cloud uses Mono (`mono-complete`, `msbuild`/`xbuild`) rather than migrating to .NET 8 SDK-style projects.

## Consequences

- Windows Visual Studio and `vstest.console` remain the native CLR path.
- Cloud tests go through `scripts/cloud-install.sh` and `scripts/run-tests.sh`.
- If a change only works on the real Framework CLR, document it and keep tests for a Windows worker — do not silently retarget.
- A future SDK/.NET (Core) migration needs a new ADR, a rewritten install image, and a full test pass.
