---
name: local-ollama-monitor
description: >-
  Monitor local Ollama inference progress via docker logs ollama and Ollama HTTP
  APIs (/api/ps, /api/tags). Use when a parent agent or user is waiting on a long
  chat/embed job and needs realtime status, errors, or completion proof.
---

# Local Ollama — monitor & logs

## Ollama HTTP (no separate Ollama MCP)

Base URL: `$env:OLLAMA_HOST` or `http://127.0.0.1:11434` after `play/_common.ps1` dot-source.

| Endpoint | Use |
|----------|-----|
| `GET /api/tags` | Server up; installed models |
| `GET /api/ps` | **Active** requests: model name, size, `expires_at`, optional progress |
| `POST /api/generate` or `/api/chat` | Smoke test (parent runs real drafts) |

Example status poll (PowerShell):

```powershell
. .\play\_common.ps1
Initialize-OllamaEnv
Invoke-RestMethod "$($env:OLLAMA_HOST.TrimEnd('/'))/api/ps"
```

## Docker logs

Recent snapshot:

```powershell
docker logs ollama --tail 100
```

Follow (background; use Shell `notify_on_output` for load/generate/error patterns):

```powershell
docker logs -f --tail 50 ollama
```

### Log lines → user-facing progress

Interpret for the parent agent (wording varies by Ollama version):

| Log pattern | Meaning |
|-------------|---------|
| `llama runner started`, `loaded`, `offloading` | Model weights loading / GPU setup |
| `prompt eval`, `eval time` | Processing prompt (may be slow on first token) |
| `generation`, `eval count`, tokens/s | Actively generating output |
| `CUDA`, `OOM`, `out of memory` | GPU memory failure — suggest restart slimmer model or restart container |
| `connection reset`, `broken pipe` | Client disconnected or server hiccup |
| Quiet after busy period | Likely **complete** — confirm with `/api/ps` empty |

## Monitoring loop (during a parent query)

1. Baseline: `/api/tags` + `/api/ps`.
2. Every **15–30s** (or on notify from `docker logs -f`):
   - `/api/ps` again
   - `docker logs ollama --tail 40`
3. Report one short paragraph: phase (load / prompt / generate / idle), model name, errors.
4. Stop when `/api/ps` has no running entries **and** logs show idle, or parent says stop.

## Streaming note

Player-agent and play scripts may use non-streaming HTTP. Logs remain the best **cross-client** progress signal; `/api/ps` is the best **structured** signal.

## Handoff snippet

```markdown
### Ollama progress (local)
- **Phase:** …
- **Model:** …
- **API /api/ps:** …
- **Latest log:** … (one line)
- **Action if stuck:** restart container or run `.\play\cicd.ps1 restart-local-llm`
```
