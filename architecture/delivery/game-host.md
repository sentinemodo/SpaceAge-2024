# Game host — implementation plan

Last updated: 2026-09-11  
Decision: [ADR-0011](../adr/ADR-0011-hosted-game-service.md)

HTTP service wrapping `Game.exe` and the `play/runs/` file layout for open beta.

## Layout

```text
game-host/
  package.json
  server.mjs           # HTTP entry
  lib/
    paths.mjs          # run directory resolution
    auth.mjs           # faction login + sessions
    game-exe.mjs       # spawn Game.exe
    status.mjs         # status.json generator
  runs/                # gitignored active campaigns
    <runId>/
      data/
      turn/
      factions/02..11/  # isolated text reports (optional mirror)
  README.md
```

## API summary

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| POST | `/api/auth/login` | — | `{ factionId, password }` → session token |
| POST | `/api/auth/logout` | session | End session |
| GET | `/api/session/report.xml` | session | Latest XML report for faction |
| GET | `/api/session/report.txt` | session | Latest text report |
| GET | `/api/session/meta` | session | Turn, faction name, status |
| PUT | `/api/session/orders` | session | Submit order file body |
| POST | `/api/session/check-orders` | session | Validate orders; return warnings |
| POST | `/api/gm/init` | GM key | Set run id + copy gamein/data |
| POST | `/api/gm/reports` | GM key | Run `/reports` |
| POST | `/api/gm/turn` | GM key | Full turn |
| POST | `/api/gm/isolate` | GM key | Copy text reports to factions |
| GET | `/api/gm/status` | GM key | Status schema v1 JSON |
| GET | `/health` | — | Liveness |

## Environment

| Variable | Purpose |
|----------|---------|
| `GAME_HOST_PORT` | Default 8787 |
| `GAME_HOST_GM_KEY` | GM endpoints |
| `GAME_EXE` | Path to `Game.exe` |
| `GAME_HOST_RUN_ID` | Active run (default `beta-1`) |
| `REPO_ROOT` | Repository root for `campaign/data.xml` |

## Done gate

- Vitest or integration script: login → fetch report → submit orders → GM turn → next report
- Leak test: faction 2 session cannot GET faction 3 report
- `README.md` documents local start + env vars

## Phased slices

- [x] GH-1 Scaffold
- [x] GH-2 Run bootstrap
- [x] GH-3 Faction auth
- [x] GH-4 Report XML distribution
- [x] GH-5 Order upload
- [x] GH-6 Turn runner
- [x] GH-7 Status JSON hook
- [x] GH-8 Check-orders bridge
