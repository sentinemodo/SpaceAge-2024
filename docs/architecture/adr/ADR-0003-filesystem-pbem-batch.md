# ADR-0003: Filesystem PBEM batch processor

Date: 2026-08-16  
Status: Accepted (documents existing practice)

## Context

SpaceAge is a play-by-email game. Historically a human GM collected emailed orders, ran a turn program, and mailed reports back. The 2024 engine continues that shape.

## Decision

The engine is an **offline batch program**:

- Inputs: `data.xml`, `gamein.xml`, `order.*` (and CLI `/data`, `/turn-dir`, `/check`).
- Outputs: `gameout.{turn}.xml`, `report.{turn}.{faction}.txt` (optional `.xml`), console logs.
- Faction `email` is stored and copied into report headers; **the engine does not send mail**.
- No database, HTTP API, or message bus.

Stub hooks (`Request`, `EventsReaders`) may later add extra turn-dir files; they stay no-ops until a feature lands with tests.

## Consequences

- Integrations are file-format contracts, not service contracts.
- External automation (mailer, GM scripts) lives outside this repository.
- Adding SMTP, a web UI, or a DB is a new product surface and needs a new ADR.
