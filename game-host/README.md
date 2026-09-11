# SpaceAge Game Host

HTTP service for open beta: faction auth, report XML, order upload, GM turn runner.

Decision: [architecture/adr/ADR-0011-hosted-game-service.md](../architecture/adr/ADR-0011-hosted-game-service.md)

## Prerequisites

- Node.js 18+
- Built `Game/bin/Debug/Game.exe` (`msbuild SpaceAge.sln`)
- `campaign/data.xml` and `campaign/gamein.1.xml`

## Environment

| Variable | Default |
|----------|---------|
| `GAME_HOST_PORT` | `8787` |
| `GAME_HOST_RUN_ID` | `beta-1` |
| `GAME_HOST_GM_KEY` | `dev-gm-key` |
| `GAME_EXE` | `Game/bin/Debug/Game.exe` |
| `GAME_USE_MONO` | `1` on non-Windows (auto); set `0` to spawn `GAME_EXE` directly |
| `MONO_EXE` | `mono` — Mono binary when `GAME_USE_MONO` is active |

## Start

```powershell
cd game-host
npm start
```

## GM workflow

```powershell
# 1. Bootstrap run from campaign seed
curl -X POST http://localhost:8787/api/gm/init -H "X-GM-Key: dev-gm-key"

# 2. Generate turn-1 reports
curl -X POST http://localhost:8787/api/gm/reports -H "X-GM-Key: dev-gm-key"

# 3. After players submit orders, run turn
curl -X POST http://localhost:8787/api/gm/turn -H "X-GM-Key: dev-gm-key"

# Status for lobby
curl http://localhost:8787/api/gm/status -H "X-GM-Key: dev-gm-key"
```

Copy status JSON to `website/public/status.json` or use `play/generate-status.ps1`.

## Player login

```json
POST /api/auth/login
{ "factionId": 2, "password": "<from gamein>" }
```

Use returned `token` as `Authorization: Bearer <token>`.

## Tests

```powershell
npm test
```
