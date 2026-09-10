# SpaceAge player-agent (C# + Ollama)

Scriptable order-drafting runner for PBEM play. Lives **outside** `Game.exe` per [ADR-0009](../../architecture/adr/ADR-0009-local-llm-player-agent.md). Implementation plan: [`architecture/delivery/local-player-agent.md`](../../architecture/delivery/local-player-agent.md).

**Phase 0 status:** CLI contracts, Ollama client smoke test, config, and path layout.

**Phase 2 status:** Shared and faction RAG ingest (`ingest-shared`, `ingest-faction`), SQLite vector store, chunking, `retrieve` dev helper, and always-on prompt pack builder.

**Phase 3 status:** `draft` retrieves top-k chunks, calls Ollama chat, runs verb allowlist lint from `player/rules.md`, and writes UTF-8 orders. RunPod usage ledger arrives in Phases 7–8.

**Phase 4 status:** Incremental faction RAG refresh after isolate — latest report replaces prior report chunks, optional order-turn window, `ingest-run` batch for factions 2–11, `play/ingest-rag.ps1` hook, and `--story-only` for campaign-ai handoff.

**Phase 5 status:** Shared RAG rebuild after engine/catalog/manual updates — `refresh-shared` orchestrates `ingest-shared`, verb allowlist export (`regenerate-allowlist` → `Lint/verb-allowlist.json`), spot-check retrieve, optional run README note, and `play/refresh-shared-rag.ps1` wrapper.

**Phase 6 status:** Campaign play wiring — `draft-run` queues sequential drafts for factions 2–11, `audit-isolation` verifies shared/faction indexes, audit notes land in `play/runs/<id>/gm/isolation-audit.md`, and `play/draft-run.ps1` wraps the batch. Cursor `/player` remains valid for single-seat work.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Ollama](https://ollama.com/) on Windows (local) or on a RunPod RTX 4090 (remote)
- Models:
  - **Local chat + smoke:** `ollama pull qwen2.5-coder:7b`
  - **Local / remote embed:** `ollama pull nomic-embed-text`
  - **RunPod (quality):** `ollama pull qwen2.5-coder:14b` (or `qwen3-coder:30b` when VRAM allows)

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
| `PLAYER_AGENT_CHAT_MODEL` | `qwen2.5-coder:7b` | Used for smoke, draft, and ingest; RunPod default `qwen2.5-coder:14b` |
| `PLAYER_AGENT_EMBED_MODEL` | `nomic-embed-text` | Same host as chat |
| `PLAYER_AGENT_INDEX_DIR` | `tools/player-agent/.data/` | Gitignored SQLite indexes |
| `PLAYER_AGENT_ALLOW_RUNPOD` | unset | Set `1` or pass `--allow-runpod` for remote hosts |

RunPod / budget variables (`RUNPOD_API_KEY`, `PLAYER_AGENT_BUDGET_USD`, …) are defined in the plan for Phases 7–8. Phase 1B adds thin start/stop warnings before full guardrails.

## Local vs RunPod (same runner)

Point `OLLAMA_HOST` at localhost or the RunPod HTTPS proxy. No code fork.

```powershell
# Local
$env:OLLAMA_HOST = "http://127.0.0.1:11434"
$env:PLAYER_AGENT_CHAT_MODEL = "qwen2.5-coder:7b"

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

Order files use **`orders.{faction}.{turn}.{iteration}.txt`** (matches SampleGame / `player/drafts/`). Example after turn 1 report for faction 2: **`orders.2.2.1.txt`** (faction 2, turn 2 orders, first iteration).

| Use case | Arguments | Output path |
|----------|-----------|-------------|
| Dev / agent testing | `--output player/drafts/orders.2.2.1.txt` | Explicit path |
| Campaign run | `--run <id> --faction <n>` | Auto: next `orders.{faction}.{turn}.{iteration}.txt` under `play/runs/<id>/factions/NN/` |

Turn defaults to **report turn + 1** (from latest `report.{turn}.{faction}.txt`). Iteration defaults to the **next free** number for that faction/turn. Override with `--turn` / `--iteration`.

**Turn processing:** when multiple iterations exist for the same faction and turn, **`RepoPaths.ResolveActiveOrderPath`** (highest iteration) is the file fed to `Game.exe`. Older iterations stay on disk for tracking, training, and development.

Example:

```powershell
dotnet run --project tools/player-agent/PlayerAgent.csproj -- draft `
  --mode test --faction 2 --output player/drafts/orders.2.2.1.txt --dry-run

dotnet run --project tools/player-agent/PlayerAgent.csproj -- draft `
  --mode campaign --run smoke-test --faction 2
```

UTF-8 drafts in faction folders; `play/turn.ps1` converts to Windows-1251 for `Game.exe`.

## Commands

| Command | Phase | Purpose |
|---------|-------|---------|
| `config` | 0 | Show resolved settings |
| `smoke` | 0 | Chat + embedding connectivity test |
| `ingest-shared --mode …` | 2 | Embed shared manuals into SQLite |
| `ingest-faction --mode … --run … --faction …` | 2/4 | Embed faction corpus (incremental by default; `--full` for Phase 2 rebuild) |
| `ingest-run --mode … --run …` | 4 | Batch incremental refresh for factions 2–11 after isolate |
| `refresh-shared --mode test\|campaign\|both` | 5 | Rebuild shared index + allowlist + spot-check after manual/catalog/engine updates |
| `regenerate-allowlist` | 5 | Export verb list from `player/rules.md` to `Lint/verb-allowlist.json` |
| `retrieve --mode … --index shared\|faction --query …` | 2 | Dev helper: top-k vector search (optional `--verb MOVE`) |
| `draft --mode … --faction …` | 3 | Generate order draft (lint + UTF-8 write) |
| `draft-run --mode … --run …` | 6 | Batch draft factions 2–11 (isolation audit first) |
| `audit-isolation --mode … [--run …]` | 6 | Verify shared/faction RAG indexes are not cross-contaminated |
| `usage …` | 7 | RunPod ledger and reports |

`--dry-run` on ingest chunks sources without calling embed; on draft builds the prompt pack without chat. `--clear` wipes the target SQLite index before ingest (or alone with `--dry-run`).

### RAG ingest (Phase 2)

```powershell
# Chunk-only dry run
dotnet run --project tools/player-agent/PlayerAgent.csproj -- ingest-shared --mode test --dry-run

# Full shared ingest (requires Ollama + nomic-embed-text)
dotnet run --project tools/player-agent/PlayerAgent.csproj -- ingest-shared --mode test

# Rebuild from scratch (clears duplicate/stale chunks)
dotnet run --project tools/player-agent/PlayerAgent.csproj -- ingest-shared --mode test --clear

# Faction ingest after isolate (incremental: latest report + story + last 3 order turns)
dotnet run --project tools/player-agent/PlayerAgent.csproj -- `
  ingest-faction --mode campaign --run demo --faction 2

# All AI seats after isolate (preferred — or use play/ingest-rag.ps1)
dotnet run --project tools/player-agent/PlayerAgent.csproj -- `
  ingest-run --mode campaign --run demo

# Phase 2 full rebuild (every report + order on disk)
dotnet run --project tools/player-agent/PlayerAgent.csproj -- `
  ingest-faction --mode campaign --run demo --faction 2 --full

# Campaign-ai updated story.md — re-embed objective only
dotnet run --project tools/player-agent/PlayerAgent.csproj -- `
  ingest-faction --mode campaign --run demo --faction 2 --story-only

# Retrieve MOVE rules chunks
dotnet run --project tools/player-agent/PlayerAgent.csproj -- `
  retrieve --mode test --index shared --query "move stack to orbit" --verb MOVE --top 4
```

### Shared RAG refresh (Phase 5)

Run **after** `/player` docs-only refresh (or human edit) when `player/rules.md`, tech manuals, `player/battle.md`, or the catalog change — **not** after every turn.

```powershell
# Plan only (checklist + chunk counts + allowlist preview)
dotnet run --project tools/player-agent/PlayerAgent.csproj -- `
  refresh-shared --mode test --dry-run

# Rebuild SampleGame shared index + allowlist + spot-check MOVE
dotnet run --project tools/player-agent/PlayerAgent.csproj -- `
  refresh-shared --mode test --spot-check-verb MOVE

# Campaign manuals + note in run README (mid-campaign catalog bump)
dotnet run --project tools/player-agent/PlayerAgent.csproj -- `
  refresh-shared --mode campaign --spot-check-tech helium --note-run smoke-test

# Both shared indexes on one machine
dotnet run --project tools/player-agent/PlayerAgent.csproj -- `
  refresh-shared --mode both

# Or use the play wrapper
.\play\refresh-shared-rag.ps1 -Mode campaign -SpotCheckTech helium -NoteRun smoke-test

# Allowlist only (no Ollama)
dotnet run --project tools/player-agent/PlayerAgent.csproj -- regenerate-allowlist
```

Faction indexes are **not** wiped by default; re-`ingest-faction` only if report templates or order syntax examples in drafts must change.

### Draft loop (Phase 3)

Prerequisites: Ollama running, indexes populated (`ingest-shared` + `ingest-faction` for the seat).

```powershell
# Prompt pack only (no chat); uses latest report under play/runs/<id>/factions/NN/
dotnet run --project tools/player-agent/PlayerAgent.csproj -- draft `
  --mode campaign --run smoke-test --faction 2 `
  --dry-run

# Full draft: retrieve → chat → lint → write UTF-8 (e.g. orders.2.2.1.txt)
dotnet run --project tools/player-agent/PlayerAgent.csproj -- draft `
  --mode campaign --run smoke-test --faction 2

# Second iteration for the same turn → orders.2.2.2.txt
dotnet run --project tools/player-agent/PlayerAgent.csproj -- draft `
  --mode campaign --run smoke-test --faction 2

# Override report path (dev)
dotnet run --project tools/player-agent/PlayerAgent.csproj -- draft `
  --mode test --faction 2 --report path/to/report.txt --output player/drafts/orders.2.2.1.txt
```

Lint reads live verb headings from `player/rules.md` (immediate + long orders). Drafts that use unknown verbs fail closed and are not written. Password is read from the report template or `persona.md` and injected locally; remote hosts strip `#faction … "password"` from the chat prompt.

UTF-8 drafts in faction folders or `player/drafts/`; **`play/turn.ps1` converts to Windows-1251** before `Game.exe`.

### Campaign batch draft (Phase 6)

Prerequisites: shared index built (`ingest-shared` or `refresh-shared`), per-seat indexes refreshed (`ingest-rag` / `ingest-run`), Ollama running.

```powershell
# Isolation audit only (shared + factions 2–11)
dotnet run --project tools/player-agent/PlayerAgent.csproj -- `
  audit-isolation --mode campaign --run smoke-test

# Full batch: audit → draft each seat sequentially
dotnet run --project tools/player-agent/PlayerAgent.csproj -- `
  draft-run --mode campaign --run smoke-test

# Prompt packs only (no chat, no writes)
dotnet run --project tools/player-agent/PlayerAgent.csproj -- `
  draft-run --mode campaign --run smoke-test --dry-run

# Or use the play wrapper (typical after ingest-rag)
.\play\draft-run.ps1 -Run smoke-test -Mode campaign
```

Audit results append to `play/runs/<id>/gm/isolation-audit.md` unless `--no-record-audit`. Use Cursor `/player` for a single seat or when you want human-in-the-loop review before writing orders.

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

Write: UTF-8 `orders.{faction}.{turn}.{iteration}.txt` with `#faction`, `#modulestack` / `#person`, `#end`. Post-generation verb allowlist lint (Phase 3). Turn processing uses the **latest iteration** for that turn.

Never index: `gamein.xml`, `gameout*.xml`, other factions’ reports, raw full `data.xml`.
