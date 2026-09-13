# Game-host Docker (GM laptop → Railway)

Runs **game-host**, **Game.exe** (via Mono), and the **visual tool** in one container.  
**Player-agent** and **Ollama** stay on the GM machine — not in this image.

## GM laptop (local Docker)

### Prerequisites

- Docker Desktop (Windows)
- **Ollama in Docker** on port **11434** (not the Windows Ollama installer)
- .NET 8 SDK (for `player-agent` on the host)

```powershell
# Existing stack (example container name: ollama)
docker start ollama
docker exec ollama ollama list
# Required models: qwen2.5-coder:7b, nomic-embed-text
docker exec ollama ollama pull qwen2.5-coder:7b
docker exec ollama ollama pull nomic-embed-text
```

Copy `.env.example` to `.env` — `OLLAMA_HOST=http://127.0.0.1:11434` is the default for Docker Ollama.

### Start game-host

From the repository root:

```powershell
docker compose up --build -d
# or: .\play\docker-up.ps1
```

- API + visual client: http://localhost:8787/client/
- Health: http://localhost:8787/health

Persistent data (host paths, gitignored):

| Host path | Container path | Purpose |
|-----------|----------------|---------|
| `game-host/runs/` | `/app/game-host/runs` | Live campaign (`gamein.xml`, orders, reports) |
| `play/runs/` | `/app/play/runs` | Init passwords, player-agent RAG inputs |

### Bootstrap open beta

```powershell
# 1. Start container (above)
# 2. Init + reports via GM API
.\play\beta-launch.ps1 -GameHostUrl http://localhost:8787
```

### Turn loop (GM)

```powershell
# After players submit orders (visual tool → game-host API)
Invoke-RestMethod -Method Post -Uri http://localhost:8787/api/gm/turn -Headers @{ 'X-GM-Key' = 'dev-gm-key' }

# Copy isolated reports for player-agent RAG (reads play/runs/…/factions/)
.\play\sync-game-host-factions.ps1 -Run beta-1

# Player-agent on host → Ollama Docker on 11434
.\play\ollama-check.ps1
.\play\sync-game-host-factions.ps1 -Run beta-1
.\play\ingest-rag.ps1 -Run beta-1 -Mode campaign
.\play\draft-run.ps1 -Run beta-1 -Mode campaign -DryRun   # drop -DryRun to draft

# Publish lobby status
.\play\generate-status.ps1 beta-1
```

### Environment

Copy `.env.example` to `.env` and set `GAME_HOST_GM_KEY` before open beta.

| Variable | Default | Notes |
|----------|---------|-------|
| `GAME_HOST_PORT` | `8787` | Host port mapping |
| `GAME_HOST_RUN_ID` | `beta-1` | Active run directory |
| `GAME_HOST_GM_KEY` | `dev-gm-key` | Rotate for beta |
| `GAME_HOST_CORS` | `*` | Set to GitHub Pages origin in production |

## Railway (later)

1. Connect repo; Railway builds this `Dockerfile`.
2. Attach a **volume** at `/app/game-host/runs` (campaign state survives redeploys).
3. Set env: `GAME_HOST_GM_KEY`, `GAME_HOST_CORS`, `GAME_HOST_RUN_ID`.
4. Railway sets `PORT`; entrypoint maps it to `GAME_HOST_PORT`.
5. **Do not** run Ollama on Railway — keep player-agent + Ollama on the GM laptop calling the public game-host URL for order upload only.

Player-agent on the GM laptop during Railway hosting (Ollama stays in local Docker):

```powershell
docker start ollama
.\play\ollama-check.ps1
# Draft locally; submit orders via game-host public URL or GM turn workflow
```
