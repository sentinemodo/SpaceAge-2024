# SpaceAge player-agent (C# + Ollama)

Scriptable order-drafting runner for PBEM play. Lives **outside** `Game.exe` per [ADR-0009](../../architecture/adr/ADR-0009-local-llm-player-agent.md). Implementation plan: [`architecture/delivery/local-player-agent.md`](../../architecture/delivery/local-player-agent.md).

**Phase 0 status:** CLI contracts, Ollama client smoke test, config, and path layout.

**Phase 2 status:** Shared and faction RAG ingest (`ingest-shared`, `ingest-faction`), SQLite vector store, chunking, `retrieve` dev helper, and always-on prompt pack builder. Draft loop and usage ledger arrive in Phases 3 and 7–8.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Ollama](https://ollama.com/) on Windows (local) or on a RunPod RTX 4090 (remote)
- Models:
  - **Local (plumbing):** `ollama pull smollm2`
  - **Local / remote embed:** `ollama pull nomic-embed-text`
  - **RunPod (quality tests):** `ollama pull qwen2.5-coder:14b`

## Build and run

From the repository root:

```powershell
dotnet build tools/player-agent/PlayerAgent.csproj
dotnet run --project tools/player-agent/PlayerAgent.csproj -- config
dotnet run --project tools/player-agent/PlayerAgent.csproj -- smoke
```

After build, the executable is `tools/player-agent/bin/Debug/net8.0/player-agent.exe`.

## Configuration (environment)

| Variable | Local default | Notes |
|----------|---------------|--------|
| `OLLAMA_HOST` | `http://127.0.0.1:11434` | Base URL; OpenAI API is `{host}/v1` |
| `PLAYER_AGENT_CHAT_MODEL` | `smollm2` | Use `qwen2.5-coder:14b` on RunPod |
| `PLAYER_AGENT_EMBED_MODEL` | `nomic-embed-text` | Same host as chat |
| `PLAYER_AGENT_INDEX_DIR` | `tools/player-agent/.data/` | Gitignored SQLite indexes |
| `PLAYER_AGENT_ALLOW_RUNPOD` | unset | Set `1` or pass `--allow-runpod` for remote hosts |

RunPod / budget variables (`RUNPOD_API_KEY`, `PLAYER_AGENT_BUDGET_USD`, …) are defined in the plan for Phases 7–8. Phase 1B adds thin start/stop warnings before full guardrails.

## Local vs RunPod (same runner)

Point `OLLAMA_HOST` at localhost or the RunPod HTTPS proxy. No code fork.

```powershell
# Local
$env:OLLAMA_HOST = "http://127.0.0.1:11434"
$env:PLAYER_AGENT_CHAT_MODEL = "smollm2"

# RunPod (example — use your pod proxy URL)
$env:OLLAMA_HOST = "https://<runpod-proxy>"
$env:PLAYER_AGENT_CHAT_MODEL = "qwen2.5-coder:14b"
dotnet run --project tools/player-agent/PlayerAgent.csproj -- smoke --allow-runpod
```

**Remote hosts require `--allow-runpod` or `PLAYER_AGENT_ALLOW_RUNPOD=1`.** Strip `#faction … "password"` from prompts sent off-box; inject the password only when writing the local order file.

## Play mode (`--mode`)

Required on `ingest-shared`, `ingest-faction`, and `draft`:

| Mode | Shared tech manuals |
|------|---------------------|
| `test` | `player/basic_technologies.md`, `player/advanced_technologies.md` |
| `campaign` | `player/campaign/basic_technologies.md`, `player/campaign/advanced_technologies.md` |

Shared indexes are stored separately: `.data/shared-test/`, `.data/shared-campaign/`.

## Draft output (no default)

Always specify **one** of:

| Use case | Arguments | Output path |
|----------|-----------|-------------|
| Dev / agent testing | `--output player/drafts/order.2.txt` | Explicit path |
| Campaign run | `--run <id> --faction <n>` | `play/runs/<id>/factions/NN/order.{n}.txt` |

Example:

```powershell
dotnet run --project tools/player-agent/PlayerAgent.csproj -- draft `
  --mode test --output player/drafts/order.2.txt --dry-run

dotnet run --project tools/player-agent/PlayerAgent.csproj -- draft `
  --mode campaign --run demo --faction 2 --dry-run
```

UTF-8 drafts in faction folders; `play/turn.ps1` converts to Windows-1251 for `Game.exe`.

## Commands

| Command | Phase | Purpose |
|---------|-------|---------|
| `config` | 0 | Show resolved settings |
| `smoke` | 0 | Chat + embedding connectivity test |
| `ingest-shared --mode …` | 2 | Embed shared manuals into SQLite |
| `ingest-faction --mode … --run … --faction …` | 2 | Embed isolated faction report / story / orders |
| `retrieve --mode … --index shared\|faction --query …` | 2 | Dev helper: top-k vector search (optional `--verb MOVE`) |
| `draft --mode …` | 3 | Generate order draft |
| `usage …` | 7 | RunPod ledger and reports |

`--dry-run` on ingest chunks sources without calling embed; on draft builds the prompt pack without chat.

### RAG ingest (Phase 2)

```powershell
# Chunk-only dry run
dotnet run --project tools/player-agent/PlayerAgent.csproj -- ingest-shared --mode test --dry-run

# Full shared ingest (requires Ollama + nomic-embed-text)
dotnet run --project tools/player-agent/PlayerAgent.csproj -- ingest-shared --mode test

# Faction ingest after isolate copies report into play/runs/<id>/factions/NN/
dotnet run --project tools/player-agent/PlayerAgent.csproj -- `
  ingest-faction --mode campaign --run demo --faction 2

# Retrieve MOVE rules chunks
dotnet run --project tools/player-agent/PlayerAgent.csproj -- `
  retrieve --mode test --index shared --query "move stack to orbit" --verb MOVE --top 4
```

Unit tests: `dotnet test tools/player-agent-tests/PlayerAgent.Tests.csproj`.

## Index layout (gitignored)

```text
tools/player-agent/.data/
  shared-test/shared.sqlite
  shared-campaign/shared.sqlite
  runs/<run-id>/faction-NN/faction.sqlite
  usage/                         # Phase 7 ledger
```

## I/O contract (matches `/player`)

Read: isolated text report, optional `story.md`, shared + faction RAG chunks, always-on prompt pack from `player/rules.md`.

Write: UTF-8 `order.{faction}.txt` with `#faction`, `#modulestack` / `#person`, `#end`. Post-generation verb allowlist lint (Phase 3).

Never index: `gamein.xml`, `gameout*.xml`, other factions’ reports, raw full `data.xml`.
