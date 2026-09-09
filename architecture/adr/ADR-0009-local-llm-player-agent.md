# ADR-0009: Local / rented LLM for player-agent order development

Date: 2026-09-09  
Status: Accepted  
Context: User approved Ollama-based inference with RunPod as the rented GPU hosting option for the SpaceAge player agent that drafts faction orders.

## Problem Statement

Campaign play uses Cursor `/player` (and `/campaign-ai` → `/player`) to turn isolated faction reports into UTF-8 order drafts. That path depends on cloud IDE agents and does not give a reproducible, scriptable inference stack for:

- Local or rented GPU inference without hanging an LLM off `Game.exe`
- Grounding drafts in live player manuals and per-faction reports (RAG)
- Ten parallel faction drafts per turn while preserving isolation (no other factions’ reports, no raw `gamein` / full `data.xml` for the strategist path)
- Optional remote GPU when the Windows workstation lacks ~16–24 GB VRAM

The engine remains a batch file-in/file-out tool ([ADR-0003](ADR-0003-filesystem-pbem-batch.md)). Encoding for turn orders stays Windows-1251 ([ADR-0002](ADR-0002-windows-1251-io.md)); drafts may be UTF-8 until `turn.ps1` / GM conversion.

## Decision

| Layer | Choice |
|-------|--------|
| **Inference runtime** | **Ollama** (OpenAI-compatible HTTP API) |
| **Chat model** | **Qwen3-Coder** — `qwen3-coder:30b` when ≥~16–24 GB VRAM; `qwen3-coder:8b` or `qwen2.5-coder:14b` on smaller GPUs |
| **Embeddings** | **`nomic-embed-text`** (same Ollama host) |
| **RAG** | Hybrid: always-on prompt pack + vector retrieval over chunked player manuals / report / prior drafts |
| **Orchestration** | Thin runner **outside** `Game/` (e.g. `tools/player-agent/`); mirrors `/player` I/O; writes UTF-8 `order.*` after verb allowlist lint |
| **Default hosting** | **Local Windows Ollama** when hardware allows |
| **Rented GPU hosting** | **[RunPod](https://www.runpod.io/)** — **1× RTX 4090 (24 GB)**; Ollama only on the pod; prefer **Secure Cloud** when faction report text leaves the machine |
| **Not chosen** | Vercel as model host; engine-hosted HTTP; fine-tuning before RAG + lint; indexing `gamein` / other factions’ reports into the strategist index |

### RunPod usage (approved)

- Pod runs **Ollama only** (chat + embed models on a persistent volume).
- Local Windows runner keeps `play/runs`, manuals, RAG indexes, passwords, and order file writes.
- Runner calls `https://<runpod-proxy>/v1/chat/completions` (same contract as `http://localhost:11434/v1`).
- **Stop / terminate the pod when idle.** Do not leave always-on 4090 for occasional PBEM turns.
- Strip `#faction … "password"` from remote prompts; inject password only when writing the local order file.
- Prefer report **excerpts** over full dumps; never upload `gamein.xml` / other factions’ reports to the pod disk.

### RAG isolation (must match campaign play)

| Index | Contents |
|-------|----------|
| **Shared** | `player/rules.md`, `player/battle.md`, SampleGame or campaign tech manuals under `player/` (path chosen by play mode) |
| **Per-faction** | That faction’s isolated report, `story.md` / objective, prior `order.*` / drafts for that seat |
| **Never** | `gamein.xml`, `gameout*.xml`, other factions’ reports, full raw `campaign/data.xml` / `Tests/data.xml` for the campaign strategist path (catalog grounding stays via player manuals) |

### Always-on prompt pack (not vector-only)

1. Faction id (password injected only at local write time for remote inference).
2. Latest text report (or truncated unit / orders-template sections).
3. Compact prefixes & subjects excerpt from `player/rules.md`.
4. Tactical objective from `story.md` or user objective.

Post-generation: validate against live verb list (rules headings and/or future `OrdersReader`-aligned allowlist) before accepting the draft.

## Rationale

- **Ollama** matches Windows + Linux GPU pods, one API shape, and scripted agent loops without a separate web product.
- **Qwen3-Coder** fits order syntax drafting and tool-style JSON; 30B MoE is the quality target on 24 GB; smaller coder tags remain the budget path.
- **Hybrid RAG** fits a small, structured corpus (`player/*.md` + reports): selective recall beats dumping the repo or fine-tuning on sparse order corpora.
- **RunPod** is the approved *rented* host: RTX 4090 inventory, Ollama-friendly templates, per-second billing, TLS proxy — better ops than Vast.ai marketplace variance; cheaper/simpler than Lambda A100/H100 for this VRAM class.
- **Vercel** is rejected as LLM host (no GPU Ollama / Qwen weights); optional later only for a browser-facing assistant API that still calls a provider or RunPod — not for PBEM ten-faction file loops today.
- Keeping the runner next to `play/` preserves ADR-0003 file contracts and campaign isolation (see `.cursor/agents/campaign-ai.md` / campaign play docs when present).

## Consequences

### Benefits

- Reproducible local or rented inference independent of Cursor cloud agents.
- Clear privacy boundary: local default; RunPod is an explicit trust trade-off with redaction rules.
- Same OpenAI-compatible client for localhost and RunPod.
- RAG refresh hooks after turns and catalog/engine doc updates (see implementation plan).

### Costs / risks

- Remote GPU: report text leaves the Windows trust boundary (mitigate with Secure Cloud, redaction, stop-when-idle).
- Models invent verbs / module ids — require allowlist lint + manuals as source of truth.
- Ten concurrent factions can thrash one 4090 — serialize or queue Ollama requests.
- RunPod pricing and SKU availability change — re-check listings before long sessions.

### Follow-on docs

- Implementation plan: [`delivery/local-player-agent.md`](../delivery/local-player-agent.md)
- Stack summary: [`technology.md`](../technology.md) § Local player-agent inference
- Vendor links: [`docs-index.md`](../docs-index.md)

## Alternatives considered

| Option | Why not primary |
|--------|-----------------|
| LM Studio | GUI experimentation; weaker for scripted loops |
| llama.cpp only | More ops burden for the same outcome |
| vLLM on day one | Linux/server throughput; awkward for Windows PBEM laptop workflow |
| Vast.ai as default | Cheaper possible; host variance — keep as deliberate budget alternative |
| Lambda Labs | Overkill VRAM/cost for 24 GB MoE inference |
| Cloud chat APIs only (OpenAI/Anthropic via gateway) | Acceptable later hybrid if local/RunPod quality fails; not the approved default |
| Fine-tune on order corpora first | Sparse data; manuals + retrieval + lint first |
| Vercel-hosted model | Platform does not host this class of GPU inference |

## Implementation plan pointer

Phased work (environment, RAG corpus prep, post-turn and engine/data refresh) is specified in [`architecture/delivery/local-player-agent.md`](../delivery/local-player-agent.md). Do not implement inside `Game.exe`.
