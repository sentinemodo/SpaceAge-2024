# Player-agent Phase 0 — stack alternatives (C# runner, Docker Model Runner)

Last updated: 2026-09-10  
Status: **Accepted for Phase 0** — implemented stack is **C# + Ollama** (see `tools/player-agent/`)  
Baseline: [ADR-0009](../adr/ADR-0009-local-llm-player-agent.md), [`local-player-agent.md`](local-player-agent.md)

This document evaluates two proposed deviations from the approved Phase 0 plan:

1. **C#** for `tools/player-agent/` instead of Python or Node.
2. **Docker Model Runner (DMR)** with `ai/smollm2` (user shorthand: `ai/smoll2`) instead of **Ollama**.

---

## Executive summary

| Question | Recommendation |
|----------|----------------|
| C# runner in `tools/player-agent/`? | **Allowed in ADR spirit** (outside `Game/`). Requires Phase 0 doc + ADR amendment. Prefer **net8 SDK-style console**, **not** in `SpaceAge.sln`. |
| DMR instead of Ollama? | **Local dev: viable** with OpenAI-compatible client. **`ai/smollm2` is smoke-test only** — not for order drafting. Use `ai/qwen2.5-coder` or keep Ollama `qwen3-coder` for quality. |
| C# + DMR end-to-end? | **Viable for local spike and runner build-out**; **not** a drop-in replacement for the approved RunPod path without a new ADR and pod template work. |
| Lowest-risk path | **Approved plan**: Python/Node + Ollama locally + Ollama on RunPod. |
| Best C# path | **C# + Ollama** (change runner language only; keep ADR-0009 inference). |
| Best DMR path | **Python + DMR** if team already prefers Docker Desktop over a separate Ollama install. |

---

## 1. C# for `tools/player-agent/`

### ADR / plan alignment

| Constraint | C# in `tools/player-agent/` |
|------------|------------------------------|
| ADR-0009: runner **outside** `Game/` | **Compliant** — same boundary as Python/Node. |
| ADR-0003: no HTTP on `Game.exe` | **Compliant** — runner is a separate process. |
| Non-goal: “Writing C# inside `Game/` for the LLM stack” | **Compliant** — `tools/` is explicitly not `Game/`. |
| Phase 0 checklist: “Pick Python or Node … **no C#**” | **Conflicts** — treat as “no C# **in Game/**”; amend Phase 0 if C# is chosen. |
| `technology.md`: net48 / non-SDK for **engine** | **No conflict** — player-agent section already says inference is **separate from net48**. |
| ADR-0001: do not convert `Game/` to SDK / net8 | **No conflict** — new SDK project under `tools/` is a **bounded tool exception**, not engine migration. |

**Conclusion:** A small C# console app in `tools/player-agent/` does **not** violate the *spirit* of ADR-0009 (“no LLM in Game.exe”). It **does** require updating Phase 0 and recording a runner-language decision (ADR amendment or ADR-0010).

### Pros vs Python/Node (this repo’s RAG scope)

| Factor | C# | Python | Node |
|--------|-----|--------|------|
| Small corpus RAG (`player/*.md` + reports) | Adequate (`Microsoft.Extensions.AI`, `Microsoft.Extensions.VectorData`, LiteDB v6 vectors, or in-memory) | **Strongest** ecosystem (Chroma, LanceDB, FAISS) | Good (LangChain.js, LanceDB) |
| OpenAI-compatible client | **Excellent** (`Microsoft.Extensions.AI.OpenAI`) | Excellent | Excellent |
| CLI / scripting on Windows | **Native** (`System.CommandLine`, `dotnet run`) | Requires Python install / venv | Requires Node install |
| Repo language fit | **Same as engine** (but runner must not reference `Game.dll`) | New runtime in repo | New runtime in repo |
| Cloud CI (Mono + `SpaceAge.sln`) | **Separate** — `dotnet build` on `tools/player-agent/` only; do not add to Mono path | Same separation | Same separation |
| Verb allowlist lint | Trivial in any language (parse `rules.md` headings) | Same | Same |
| Isolation / file I/O | Same as plan (read `play/runs/…`, write UTF-8 orders) | Same | Same |

### Recommended C# layout

```text
tools/player-agent/
  PlayerAgent.csproj          # SDK-style, net8.0, OutputType=Exe
  Program.cs
  Commands/                     # ingest-shared, ingest-faction, draft, usage
  Inference/                    # IChatClient / IEmbeddingGenerator wiring
  Rag/                          # chunk, index, retrieve
  Lint/                         # verb allowlist
  README.md
  .data/                        # gitignored indexes + usage ledger
```

| Decision | Recommendation |
|----------|----------------|
| TFM | **net8.0** (LTS, modern AI packages). **Not** net472/net48 — no benefit; would drag legacy constraints. |
| Solution membership | **Do not** add to `SpaceAge.sln` initially. Optional `tools/player-agent/PlayerAgent.sln` or repo-root `SpaceAge.tools.slnf`. |
| Chat + embed client | `Microsoft.Extensions.AI.OpenAI` → configurable `base_url` |
| Vector store (Phase 0 spike) | `Microsoft.Extensions.VectorData` + **InMemory** or **SQLite** connector; or **LiteDB 6** vectors for single-file `.data/` |
| Later production store | Qdrant in Docker (optional) — same abstractions |

**Blockers / mitigations**

| Blocker | Mitigation |
|---------|------------|
| Phase 0 says Python/Node only | Amend [`local-player-agent.md`](local-player-agent.md) Phase 0; add ADR note for runner language. |
| No `dotnet` in cloud Mono image | Player-agent is **local Windows GM tooling** — document “not built on Mono CI” or add optional GitHub Action `dotnet build tools/player-agent`. |
| Accidental `Game` reference | No project reference to `Game.csproj`; duplicate static verb list from `rules.md`, do not call `OrdersReader`. |

---

## 2. Docker Model Runner vs Ollama

### What is `ai/smollm2`?

User query says `ai/smoll2`; Docker Hub / DMR use **`ai/smollm2`** (SmolLM2, ~360M parameters, Q4 quants).

**Docker Model Runner (DMR)** is Docker Desktop / Docker Engine feature that pulls OCI model artifacts and serves:

| API | Base URL (host TCP) |
|-----|---------------------|
| OpenAI-compatible | `http://localhost:12434/engines/v1` |
| Ollama-compatible | `http://localhost:12434` (`/api/chat`, `/api/embeddings`, …) |
| Native DMR | `/models/*`, etc. |

Default port **12434** (Ollama uses **11434**). On Windows, enable Model Runner + **TCP host access** (`docker desktop enable model-runner --tcp 12434`).

### Chat vs embeddings

| Role | `ai/smollm2` | Approved Ollama stack |
|------|--------------|------------------------|
| Chat / draft | Tiny general model — **smoke tests only** | `qwen3-coder:30b` (quality target) |
| Embeddings | **Not an embedding model** | `nomic-embed-text` |

DMR **cannot** use one `smollm2` pull for both chat and RAG embeddings. Pattern matches Ollama: **two models**:

- Chat: `ai/qwen2.5-coder` (or `ai/smollm2` for plumbing only)
- Embeddings: `ai/all-minilm` with Compose/runtime flag **`--embeddings`** (see [Docker Compose models — Embeddings](https://docs.docker.com/ai/compose/models-and-compose/))

There is **no** `nomic-embed-text` tag in DMR; use `ai/all-minilm` or similar and **re-embed** if switching hosts (dimension mismatch vs `nomic-embed-text`).

### Can DMR replace Ollama?

| Surface | DMR | Ollama |
|---------|-----|--------|
| OpenAI `/v1/chat/completions` | Yes (`/engines/v1/…`) | Yes |
| OpenAI `/v1/embeddings` | Yes (separate embed model + `--embeddings`) | Yes |
| Ollama `/api/*` | Compatibility layer on same port | Native |
| RunPod (approved) | **Not in ADR-0009** — no standard RunPod template | **Approved**: Ollama on 4090 pod |
| Model tags | `docker model pull ai/qwen2.5-coder` | `ollama pull qwen3-coder:30b` |
| Windows laptop workflow | Needs Docker Desktop + GPU passthrough (WSL2) | Single installer, common docs |
| Quality for order syntax | **`smollm2`: poor**; **`ai/qwen2.5-coder`: acceptable**; **`qwen3-coder:30b`: best** (ADR target) | ADR-aligned |

### RunPod impact

ADR-0009 explicitly: **“Pod runs Ollama only”** and runner calls `https://<runpod-proxy>/v1/…` (same as `localhost:11434/v1`).

Switching the **pod** to DMR would require:

- Custom pod image / startup (DMR on Linux Engine, not Docker Desktop)
- New ops checklist, persistent volume layout, proxy port
- Revised guardrails and usage tracker assumptions
- **New ADR** — do not silently swap Ollama on RunPod

**Practical approach:** Keep **Ollama on RunPod**; use DMR **only for local Windows** if desired. Runner’s inference abstraction already supports both via `base_url`.

### Operational comparison

| | Local Windows + Ollama | Local Windows + DMR |
|--|------------------------|---------------------|
| Install | `ollama.com` installer | Docker Desktop + enable Model Runner |
| Pull | `ollama pull …` | `docker model pull ai/…` |
| Smoke model | `qwen3-coder:8b` | `ai/smollm2` (fast, low quality) |
| Prod chat model | `qwen3-coder:30b` | `ai/qwen2.5-coder` (closest DMR catalog match to ADR) |
| Embed model | `nomic-embed-text` | `ai/all-minilm` + `--embeddings` |
| VRAM | Model-dependent | Same underlying weights (llama.cpp) |

---

## 3. Combined stack matrix

| Stack | Local dev | RunPod | RAG | Order draft quality | Risk |
|-------|-----------|--------|-----|---------------------|------|
| **Python + Ollama** (approved) | ✓ | ✓ | ✓ | ✓ (ADR models) | **Lowest** |
| **C# + Ollama** | ✓ | ✓ | ✓ | ✓ | **Low** — language change only |
| **Python + DMR (qwen2.5-coder + all-minilm)** | ✓ | ✗ without new ADR | ✓ (re-embed) | Good, not identical to 30B MoE | Medium |
| **C# + DMR (qwen2.5-coder + all-minilm)** | ✓ | ✗ without new ADR | ✓ | Good | Medium |
| **Any + DMR smollm2 only** | Spike only | ✗ | Partial | **Unacceptable** for orders | High hallucination risk |

**Verdict:** **C# + Ollama** is the best C# choice. **C# + DMR** is viable for local development if Docker is already central; treat **`smollm2` as Phase 0 plumbing only**, not the campaign chat model.

---

## 4. Required doc / ADR changes (if proceeding)

### C# runner only (keep Ollama)

- Amend [`local-player-agent.md`](local-player-agent.md) Phase 0: allow C#; remove ambiguous “no C#”.
- Add revision note to [`technology.md`](../technology.md) § Local player-agent: runner language = C# / net8.
- Optional short ADR-0010: “Player-agent runner language: C# in `tools/`”.

### DMR for local inference (keep Ollama on RunPod)

- Amend ADR-0009 **or** ADR-0010: local host may be **Ollama or DMR**; RunPod remains **Ollama**.
- Update env vars: `PLAYER_AGENT_INFERENCE_BASE_URL`, `PLAYER_AGENT_CHAT_MODEL`, `PLAYER_AGENT_EMBED_MODEL` (document DMR defaults vs Ollama defaults).
- Update [`docs-index.md`](../docs-index.md): DMR API reference + date retrieved.
- Phase 1A: add DMR smoke path alongside Ollama smoke path.

### Full DMR everywhere (including RunPod)

- **Not recommended** without explicit GM acceptance of ops cost and quality tradeoffs.
- Requires ADR-0009 revision (replace “Ollama only on pod”), full Phase 1B rewrite, new pod template.

---

## 5. Concrete Phase 0 — if user chooses C# + DMR

### Project

- Path: `tools/player-agent/PlayerAgent.csproj`
- TFM: `net8.0`, SDK-style, `RollForward=Major` optional
- Packages (indicative): `Microsoft.Extensions.AI.OpenAI`, `Microsoft.Extensions.Hosting`, `System.CommandLine`, `Microsoft.Extensions.VectorData` + InMemory/Sqlite

### CLI commands (match plan)

```text
player-agent ingest-shared [--mode sample|campaign]
player-agent ingest-faction --run <id> --faction <n>
player-agent draft --run <id> --faction <n> [--dry-run]
player-agent usage start|stop|status|report
```

### Environment variables

| Variable | DMR example | Ollama example |
|----------|-------------|----------------|
| `PLAYER_AGENT_INFERENCE_BASE_URL` | `http://localhost:12434/engines/v1` | `http://localhost:11434/v1` |
| `PLAYER_AGENT_CHAT_MODEL` | `ai/qwen2.5-coder` (prod spike) / `ai/smollm2` (smoke) | `qwen3-coder:30b` |
| `PLAYER_AGENT_EMBED_BASE_URL` | same or separate | same |
| `PLAYER_AGENT_EMBED_MODEL` | `ai/all-minilm` | `nomic-embed-text` |
| `PLAYER_AGENT_INDEX_DIR` | `tools/player-agent/.data/index` | same |
| RunPod / budget vars | unchanged from plan | unchanged |

Implement **`IInferenceHost`** (or MEAI clients) so switching DMR ↔ Ollama ↔ RunPod is config-only.

### Embeddings when chat is `smollm2`

Always run a **second** DMR model for embeddings, e.g.:

```yaml
# illustrative compose fragment
models:
  embed:
    model: ai/all-minilm
    runtime_flags:
      - "--embeddings"
```

Re-run `ingest-shared` after any embed model or dimension change.

### Minimum validation spike (before Phase 2–3)

1. **DMR host:** `docker model pull ai/smollm2` + `docker model pull ai/all-minilm`; configure embed with `--embeddings`; smoke `POST …/chat/completions` and `POST …/embeddings`.
2. **C# retrieve:** chunk `player/rules.md` → embed → store → `TopK` retrieve for query “MOVE order syntax”.
3. **Quality gate (required):** repeat draft with **`ai/qwen2.5-coder`** (or local Ollama `qwen3-coder:8b`) on one SampleGame report; run verb allowlist lint; compare shape to `player/drafts/`.
4. **Explicit fail:** if only `smollm2` passes lint inconsistently, do **not** proceed to Phase 6 campaign batch with that model.

---

## 6. Reading order

1. [ADR-0009](../adr/ADR-0009-local-llm-player-agent.md) — current accepted decision  
2. [`local-player-agent.md`](local-player-agent.md) — phased implementation  
3. **This document** — Phase 0 alternatives  
4. [Docker Model Runner API](https://docs.docker.com/ai/model-runner/api-reference/) — DMR endpoints (retrieved 2026-09-10)
