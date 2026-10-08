---
name: local-ollama-docker
description: >-
  Operate SpaceAge local Ollama via Docker Desktop and the ollama container
  (start, restart, create, health). Use when checking Docker/Ollama status,
  fixing unreachable localhost:11434, or before player-agent drafts. Pair with
  local-ollama-monitor during long queries.
---

# Local Ollama — Docker

## Preconditions

- **Docker Desktop** on Windows (WSL2 backend). Engine must answer `docker info`.
- Container name **`ollama`** (exact match in repo scripts).
- Repo root: run PowerShell commands from SpaceAge-2024 root.

## Quick commands

| Goal | Command |
|------|---------|
| Full repair + warm + verify | `.\play\cicd.ps1 restart-local-llm` |
| Verify API + models only | `.\play\ollama-check.ps1` |
| Is container running? | `docker ps --filter name=^/ollama$` |
| Start stopped | `docker start ollama` |
| Restart running | `docker restart ollama` |
| API up? | `Invoke-RestMethod http://127.0.0.1:11434/api/tags` |

Create-if-missing (only when `docker ps -a` shows no `ollama`):

```powershell
docker run -d --name ollama -p 11434:11434 -v ollama:/root/.ollama ollama/ollama
```

Pull models inside the container (if check fails):

```powershell
docker exec ollama ollama pull qwen2.5-coder:7b
docker exec ollama ollama pull nomic-embed-text
```

Environment defaults live in [`play/_common.ps1`](../../../play/_common.ps1) (`Initialize-OllamaEnv`, `Test-OllamaDocker`).

## Docker MCP (`user-MCP_DOCKER`)

1. `GetDynamicTools` → `namespace: user-MCP_DOCKER`.
2. If status is **error**: tell the user MCP Docker failed in Cursor Settings; use Shell commands in this skill.
3. If healthy: inspect tools (container list, logs, exec). **Always read schemas** before `CallDynamicTool`.
4. Host `ollama` on the GM machine may **not** be visible inside MCP sandbox containers — when MCP cannot see the container, Shell `docker` on the host wins.

Cursor config reference: user `mcp.json` runs `docker mcp gateway run --profile sentinemodo`.

## Diagnosis cheatsheet

| Symptom | Likely cause | Fix |
|---------|----------------|-----|
| `docker info` fails | Docker Desktop stopped | Start Docker Desktop; retry |
| Connection refused :11434 | Container down | `docker start ollama` or `restart-local-llm` |
| API 200 but check throws on model | Model not pulled | `docker exec ollama ollama pull …` |
| Restart loop / OOM in logs | VRAM or bad state | `docker restart ollama`; reduce parallel jobs |
| Wrong host | Remote RunPod | Not this skill — use `/runpod-runner` |

## Do not

- Remove volume `ollama` without explicit user approval (deletes downloaded models).
- Run `docker compose` for game-host unless user asked for full stack (`/cicd`).
