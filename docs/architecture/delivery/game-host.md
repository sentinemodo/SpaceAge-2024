# Game host — implementation plan

Last updated: 2026-10-15  
Decision: [ADR-0011](../adr/ADR-0011-hosted-game-service.md)

## Public reach (current)

The GM laptop (`192.168.100.17`) serves game-host on port 8787. Caddy terminates TLS for `https://spaceage-pbem.duckdns.org` and proxies to that port. DuckDNS points at WAN `91.220.222.102`. The Huawei HS8145V forwards TCP 80 and 443 only; 8787 is not on the WAN. ngrok (`manatee-sabbath-kudos.ngrok-free.dev`) is the fallback. Operational steps: [`play/router-port-forward.md`](../../../play/router-port-forward.md) and [`play/hosted-beta-gm.md`](../../../play/hosted-beta-gm.md). The Pages workflow sets `PUBLIC_CLIENT_URL` to `https://spaceage-pbem.duckdns.org/client/` and `PUBLIC_STATUS_URL` to the game-host `/api/public/lobby-status` endpoint so the lobby dashboard stays live without redeploying static `status.json`.

HTTP service wrapping `Game.exe` and the `play/runs/` file layout for open beta.

## Layout

```text
game-host/
  package.json
  server.mjs           # listen, CORS, /health, /client static, public lobby
  routes/
    session.mjs        # auth, faction session, admin browse, battle sim
    ai.mjs             # personas, story, AI query
    runpod.mjs         # session RunPod start / stop / status
    gm.mjs             # GM-key turn runner
  lib/
    game-exe.mjs       # only Game.exe spawn
    paths.mjs          # resolves play/runs/<runId>/
    auth.mjs           # faction login + sessions
    http.mjs           # JSON, CORS, request body
    session-view.mjs   # run/turn scope for a session
    status.mjs         # status.json generator
  README.md

play/runs/             # gitignored canonical campaign tree (shared with play/*.ps1)
  <runId>/
    data/
    turn/
    factions/02..11/
```

`server.mjs` dispatches by path. Each route module returns whether it handled the request. `lib/game-exe.mjs` remains the only place that spawns `Game.exe` (`/reports`, full turn, parse-orders, battle sim). API paths are unchanged.

## API summary

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/health` | — | Liveness and feature flags |
| GET | `/api/public/lobby-status` | — | Lobby status JSON |
| POST | `/api/auth/login` | — | `{ factionId, password }` → session token |
| POST | `/api/auth/logout` | session | End session |
| GET | `/api/session/meta` | session | Turn, faction, run, admin browse |
| GET | `/api/session/runs` | session | Runs this session may see |
| GET | `/api/session/turns` | session | Turns in the viewed run |
| POST | `/api/session/context` | session | Admin sets viewed run/turn |
| POST | `/api/session/view-as` | admin session | Browse as another faction |
| GET | `/api/session/report.xml` | session | Latest XML report for faction |
| GET | `/api/session/report.txt` | session | Latest text report |
| GET | `/api/session/report-sections` | session | Text report split into sections |
| GET | `/api/static/basic-technologies` | session | Technologies markdown |
| GET | `/api/session/orders` | session | Submitted orders for the viewed turn |
| PUT | `/api/session/orders` | session | Submit order file body |
| POST | `/api/session/check-orders` | session | Lightweight syntax warnings |
| POST | `/api/session/parse-orders` | session | Parse orders via `Game.exe` |
| POST | `/api/session/battle-sim` | session | Battle sim via `Game.exe` |
| GET | `/api/session/runpod/status` | session | RunPod pod status |
| POST | `/api/session/runpod/start` | session | Start the RunPod pod |
| POST | `/api/session/runpod/stop` | session | Stop the RunPod pod |
| GET | `/api/session/personas` | session | List AI personas |
| GET | `/api/session/persona` | session | Read one persona |
| PUT | `/api/session/persona` | admin session | Save a persona |
| GET | `/api/session/story` | session | Faction story text |
| PUT | `/api/session/story` | session | Save faction story |
| POST | `/api/session/ai/query` | session | Ask the faction AI |
| POST | `/api/session/ai/draft-story` | session | Regenerate story from a persona |
| GET | `/api/session/admin/lobby-status` | admin session | Same status JSON as the GM |
| POST | `/api/session/admin/sync-lobby-status` | admin session | Write lobby status file |
| GET | `/api/session/admin/orders` | admin session | Read one faction's orders |
| PUT | `/api/session/admin/orders` | admin session | Write one faction's orders |
| POST | `/api/session/admin/parse-orders` | admin session | Parse orders for a named faction |
| POST | `/api/gm/init` | GM key | Set run id + copy gamein/data |
| POST | `/api/gm/reports` | GM key | Run `/reports` |
| POST | `/api/gm/turn` | GM key | Full turn |
| POST | `/api/gm/isolate` | GM key | Copy text reports to factions |
| GET | `/api/gm/status` | GM key | Status schema v1 JSON |
| POST | `/api/gm/sync-lobby-status` | GM key | Write lobby status file |

## Environment

| Variable | Purpose |
|----------|---------|
| `GAME_HOST_PORT` | Default 8787 |
| `GAME_HOST_GM_KEY` | GM endpoints |
| `GAME_EXE` | Path to `Game.exe` |
| `GAME_HOST_RUN_ID` | Active run (default `beta-1`) |
| `REPO_ROOT` | Repository root for `play/campaign/data.xml` |

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
