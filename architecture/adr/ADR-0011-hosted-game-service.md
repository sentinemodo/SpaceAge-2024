# ADR-0011: Hosted campaign game service

Date: 2026-09-11  
Status: **Accepted**  
Plan: [`../delivery/game-host.md`](../delivery/game-host.md)

Supplements [ADR-0003](ADR-0003-filesystem-pbem-batch.md) (batch engine) and revises the **public-origin** bar in [ADR-0007](ADR-0007-public-campaign-website.md): the static lobby still never serves `gamein.xml`; the **game-host** origin does, behind faction authentication.

## Context

Open beta requires ten invited players to use a **hosted web client** instead of local report files. The engine remains `Game.exe` on .NET 4.8 with file-in/file-out semantics. A thin HTTP layer wraps the existing `play/runs/<id>/` layout.

## Decision

1. **New bounded context: Game Host.** Lives in `game-host/` at the repo root. Not a `Game` project reference.
2. **Stack:** Node.js 18+ HTTP server (Express or native `http`). Spawns `Game.exe` as a child process — same CLI as [play/README.md](../../play/README.md). No engine HTTP inside `Game.exe`.
3. **State:** One active run directory per campaign on server disk:
   - `data/data.xml`, `data/gamein.xml`, `data/gameout.{N}.xml`
   - `turn/order.{id}.txt`, `turn/report.{turn}.{faction}.txt` + `.xml`
4. **Authentication:** Faction id (2–11) + password from live `gamein.xml` `#faction` lines. Session token (HTTP-only cookie or Bearer). Passwords never logged or returned in API bodies.
5. **Player read API (faction-scoped):**
   - `GET /api/session/report.xml` — latest `report.{turn}.{faction}.xml` for authenticated faction
   - `GET /api/session/report.txt` — text report fallback
   - `GET /api/session/meta` — turn number, faction name, engine version (no foreign intel)
6. **Player write API:**
   - `PUT /api/session/orders` — UTF-8 order body; server converts to Windows-1251 for `turn/order.{id}.txt`
7. **GM API (shared secret `GAME_HOST_GM_KEY`):**
   - `POST /api/gm/init` — bootstrap run from uploaded gamein
   - `POST /api/gm/reports` — `Game.exe /reports`
   - `POST /api/gm/turn` — full turn execute
   - `POST /api/gm/isolate` — copy text reports to faction-facing paths
   - `GET /api/gm/status` — orders-submission schema for lobby JSON
8. **Validation bridge:**
   - `POST /api/session/check-orders` — write temp order file, load game + parse orders, return parse/validation messages (extends `/check` path)
9. **Catalog:** `campaign/data.xml` on host disk only; not exposed as full download to clients.
10. **Hosting:** Windows VM or developer machine with built `Game.exe`. Railway/other PaaS acceptable if Windows + Mono path documented ([ADR-0001](ADR-0001-net48-legacy-csproj.md)).

## Options considered

| Option | Decision | Why |
|--------|----------|-----|
| Engine REST inside `Game.exe` | Rejected | Violates ADR-0003 batch host |
| ASP.NET Core wrapper | Rejected | CLR migration; ADR-0001 |
| Email-only PBEM | Rejected | Open beta requires hosted state |
| Full `gamein.xml` to client | Rejected | Faction intel leak; UT-005 |
| PostgreSQL game state | Rejected | File layout already proven in `play/` |

## Consequences

- Visual tool (`visual-tool/`) consumes game-host API, not local files.
- Lobby (`website/`) links to visual tool; status JSON from `generate-status.ps1` or game-host `/api/gm/status`.
- Security tests required: faction A cannot fetch faction B report (UT-005).
- GM must protect `GAME_HOST_GM_KEY` and run disk backups of `game-host/runs/`.

## Revision

- 2026-09-11: Accepted for open beta.
