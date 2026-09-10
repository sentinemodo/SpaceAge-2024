# Local player-agent LLM — implementation plan

Last updated: 2026-09-10  
Decision: [ADR-0009](../adr/ADR-0009-local-llm-player-agent.md)  
Related: `.cursor/agents/player.md`, `player/README.md`, campaign play isolation docs when present

This plan delivers a **scriptable player-agent runner** that drafts UTF-8 order files using **Ollama** (local Windows or **RunPod** RTX 4090) and **hybrid RAG** over player manuals and isolated reports. It does **not** change `Game.exe`, add engine HTTP, or fine-tune models in phase 1.

## Goals

1. Prepare inference environments (local Ollama and optional RunPod).
2. Prepare and maintain a RAG corpus from player documentation (and only allowed per-faction inputs).
3. Refresh RAG after new turn reports and after engine / catalog / player-manual updates.
4. Preserve campaign isolation and Windows-1251 turn encoding (UTF-8 drafts → `turn.ps1` / GM).
5. **Track RunPod usage** (pod time, sessions, estimated cost) for GM visibility.
6. **Guardrails** so rented GPU cannot run away on cost or unintended always-on / unbounded API use.

## Non-goals (this plan)

- Hosting inference on Vercel.
- Indexing `gamein.xml`, `gameout*.xml`, other factions’ reports, or raw full `data.xml` into the strategist path.
- Replacing Cursor `/player` in-repo until the runner matches its I/O contract and lint gates.
- Writing C# inside `Game/` for the LLM stack.
- Automatic billing disputes with RunPod (tracker is local estimate + API facts; human owns the RunPod console).

## Target layout

```text
tools/player-agent/          # C# net8 runner, ingest, lint, usage, guardrails (outside Game/)
  PlayerAgent.csproj         # in SpaceAge.sln
  README.md
  …                          # Commands/, Inference/, Rag/, Configuration/
  .data/                     # gitignored: SQLite indexes, usage ledger, budget state
play/runs/<id>/              # existing campaign isolation; reports & orders stay here
player/                      # canonical manuals (RAG shared corpus)
architecture/adr/ADR-0009-…  # decision
```

Suggested config (local, not committed secrets): endpoint URL (`localhost:11434` or RunPod proxy), model tags, index paths under `tools/player-agent/.data/` (gitignored), **budget caps** and RunPod API credentials for usage sync (env / local config only — never commit).

---

## Phase 0 — Decide runner stack and contracts

- [x] **C# / net8** console in `tools/player-agent/` (`PlayerAgent.csproj`, member of `SpaceAge.sln`). OpenAI-compatible HTTP client to Ollama. No reference to `Game.dll`.
- [x] Freeze I/O to match `/player`: read report (+ optional `story.md`) → draft `orders.{faction}.{turn}.{iteration}.txt` UTF-8; runner stays outside `Game/`.
- [x] **`--mode test|campaign`** required on `ingest-shared`, `ingest-faction`, and `draft` (separate shared indexes; `test` = SampleGame manuals, `campaign` = `player/campaign/*`).
- [x] Draft output: **no default** — require `--output <path>` (dev/test) **or** `--run <id> --faction <n>` (campaign → `play/runs/<id>/factions/NN/orders.{faction}.{turn}.{iteration}.txt`; turn = report turn + 1; iteration auto-increments).
- [x] Turn processing uses the **latest iteration** per faction/turn (`RepoPaths.ResolveActiveOrderPath`); older files kept for tracking/training.
- [x] Env vars: `OLLAMA_HOST`, `PLAYER_AGENT_CHAT_MODEL`, `PLAYER_AGENT_EMBED_MODEL`, `PLAYER_AGENT_INDEX_DIR`, `PLAYER_AGENT_ALLOW_RUNPOD`; plus RunPod/budget vars (Phase 7–8): `RUNPOD_API_KEY`, `PLAYER_AGENT_RUNPOD_POD_ID`, `PLAYER_AGENT_BUDGET_USD`, `PLAYER_AGENT_MAX_POD_HOURS`, `PLAYER_AGENT_REQUIRE_CONFIRM`.
- [x] Local defaults: chat **`qwen2.5-coder:7b`** (smoke + draft), embed **`nomic-embed-text`**. RunPod chat default **`qwen2.5-coder:14b`** when host is non-local unless overridden.
- [x] Vector index layout: **SQLite** under gitignored `tools/player-agent/.data/` (`shared-test`, `shared-campaign`, per-run faction DBs).
- [x] CLI stubs: `config`, `smoke`, `ingest-shared`, `ingest-faction`, `draft`, `usage` — ingest/draft/usage bodies land in Phases 2–3 and 7.
- [x] Document RunPod vs local switch in `tools/player-agent/README.md`.
- [x] Remote hosts require `--allow-runpod` or `PLAYER_AGENT_ALLOW_RUNPOD=1`; full ledger + budget guardrails in Phases 7–8; **thin warnings in Phase 1B**.

**Done when:** README states how to point the same runner at local Ollama or RunPod without code forks. **Met (2026-09-10).**

---

## Phase 1 — Prepare the environment

### 1A. Local Windows (default when VRAM allows)

- [ ] Install [Ollama](https://ollama.com/) for Windows; confirm service on `http://localhost:11434`.
- [ ] Pull chat model: `qwen3-coder:30b` (or `8b` / `qwen2.5-coder:14b` on ≤12 GB VRAM).
- [ ] Pull embeddings: `nomic-embed-text`.
- [ ] Smoke-test `POST /v1/chat/completions` and an embed call.
- [ ] Confirm no game files are required on the Ollama host beyond what the runner sends in the request.

### 1B. RunPod (approved rented GPU)

- [ ] Create RunPod account; prefer **Secure Cloud** when reports will be sent.
- [ ] Deploy **1× RTX 4090 (24 GB)** with an Ollama-capable template (or base image + install Ollama).
- [ ] Attach a **persistent volume**; pull `qwen3-coder:30b` + `nomic-embed-text` once onto the volume.
- [ ] Expose HTTPS proxy to Ollama’s OpenAI-compatible port; **do not** leave `0.0.0.0:11434` open without auth/proxy.
- [ ] Set local runner `OLLAMA_HOST` to the proxy URL; re-run the same smoke tests as 1A.
- [ ] Ops checklist: start pod → draft batch → **stop/terminate**; record approximate $/hr from current listing (prices change).
- [ ] Do **not** treat RunPod as production-ready for campaign batches until **Phase 7 (usage tracker)** and **Phase 8 (cost/usage guardrails)** are done.

### 1C. Security baseline (both hosts)

- [ ] Never persist faction passwords on the GPU volume or in remote chat logs.
- [ ] Strip `#faction N "password"` from prompts sent off-box; inject only at local file write.
- [ ] Serialize or queue multi-faction calls on one 4090.

**Done when:** Local and (if used) RunPod endpoints both complete a smoke chat + embed; stop-when-idle is written into the runner README.

---

## Phase 2 — Prepare documentation for RAG

### 2A. Corpus inventory (shared index)

| Source | Chunk strategy | Metadata |
|--------|----------------|----------|
| `player/rules.md` | By verb / `##` heading | `doc=rules`, `verb=…` |
| `player/battle.md` | By section | `doc=battle` |
| `player/basic_technologies.md` + `player/advanced_technologies.md` | Tech / module / item entry | `doc=tech`, `mode=test` |
| `player/campaign/basic_technologies.md` (+ advanced when present) | Same | `doc=tech`, `mode=campaign` |
| Prior drafts under `player/drafts/` (optional style) | Per `#modulestack` / `#person` block | `doc=draft` |

Play mode selects **`test`** vs **campaign** tech manuals — do not mix both into one query without an explicit `--mode` flag.

### 2B. Corpus inventory (per-faction index)

| Source | Chunk strategy | Notes |
|--------|----------------|-------|
| Isolated `report.*.txt` (faction folder) | By unit / major section; cap size | Never other factions |
| `story.md` / objective | Whole or short sections | From campaign-ai or human |
| Prior `orders.{faction}.{turn}.{iteration}.txt` for that seat | Per stack block | Style + continuity |

### 2C. Always-on pack (not only vectors)

Build a fixed prompt pack generator:

1. Compact prefixes & subjects from `rules.md`.
2. Truncated report (or structured excerpts).
3. Objective text.
4. Faction id line **without** password when `OLLAMA_HOST` is remote.

Implemented in `Rag/PromptPackBuilder.cs`; wired into `draft --dry-run` in Phase 3.

### 2D. Ingest tool

- [x] `tools/player-agent` command: `ingest-shared` — embed + store chunks for manuals.
- [x] Command: `ingest-faction --run <id> --faction <n>` — report + story + prior orders only.
- [x] Vector store under gitignored `.data/` (**SQLite**, Phase 0 layout in `tools/player-agent/Rag/`).
- [x] Store path + heading metadata; support filter-by-planned-verb for retrieval (`retrieve --verb …`).

### 2E. Documentation hygiene (human / `/player`)

Before first production ingest, ensure manuals are the live truth:

- [ ] `player/rules.md` reflects implemented verbs only (per `/player` hard rules).
- [ ] Campaign vs SampleGame tech manuals point at the correct catalog excerpt.
- [ ] `player/README.md` links this plan and notes which files are RAG sources.

**Done when:** `ingest-shared` + one `ingest-faction` produce a retrievable index; a dry-run retrieve for verb `MOVE` returns rules chunks with correct metadata. **Code complete (2026-09-10)** — run ingest against a live Ollama host to populate indexes; unit tests cover chunking, SQLite replace, and verb-filtered retrieval.

---

## Phase 3 — Draft loop + lint

- [x] Retrieve top-k ~4–8 chunks (prefer verb filter when objective names MOVE/JUMP/…).
- [x] Call chat model; stream optional.
- [x] **Verb allowlist lint** against headings / known `EOrderType` list exported as data (static list generated from `rules.md` is enough for v1).
- [x] Write UTF-8 draft to `player/drafts/` or `play/runs/<id>/factions/NN/orders.{faction}.{turn}.{iteration}.txt`.
- [x] Reminder in README: GM / `turn.ps1` converts to Windows-1251 for `/turn-dir`.

**Done when:** One SampleGame or campaign faction report produces a lint-passing draft comparable in shape to existing `player/drafts/`. **Code complete (2026-09-10)** — run `ingest-shared` + `ingest-faction`, then `draft` against a live Ollama host to produce a lint-passing file; unit tests cover allowlist, lint, prompt pack, and writer normalization.

---

## Phase 4 — Update RAG after new turn reports

Trigger: after `Game.exe` turn (or reports-only) and isolate copies reports into `play/runs/<id>/factions/NN/`.

### Automation (preferred)

- [x] Hook or script step in the play loop (document next to `turn.ps1` / isolate scripts): for each faction 2–11, run `ingest-faction` for the **new** report path; drop or version-stamp the previous turn’s report chunks for that faction.
- [x] Do **not** rebuild the entire shared manual index on every turn (unless manuals changed).
- [x] Optional: keep last N turns of prior orders in the per-faction index for style; prune older than N.

### Manual fallback

- [x] Document: after isolate, run  
  `… ingest-faction --run <id> --faction <n>`  
  before drafting that seat.

### Campaign-ai handoff

- [x] After `story.md` updates, re-ingest that faction’s story chunk before draft.
- [x] `/campaign-ai` continues to own strategy text; runner consumes it as RAG + prompt pack input.

**Done when:** A second turn’s report replaces the prior report in the faction index without contaminating other seats; draft path uses the new report. **Code complete (2026-09-10)** — `ingest-faction` defaults to incremental refresh (latest report, story, `--max-order-turns` default 3); `ingest-run` + `play/ingest-rag.ps1` batch factions 2–11; `--story-only` for campaign-ai; unit tests cover planner, prune, and SQLite delete.

---

## Phase 5 — Update RAG after engine / data file updates

Triggers (any of):

- Engine order/combat behavior change that `/player` would refresh into manuals.
- `Tests/data.xml` or `campaign/data.xml` catalog change affecting techs/modules/items.
- Manual edit to `player/rules.md`, `player/battle.md`, or tech manuals.

### Procedure

1. **Refresh player docs first** (Cursor `/player` docs-only path or human): update `rules.md` / tech manuals / `battle.md` from engine + the correct catalog. Do not ingest stale manuals.
2. **Rebuild shared index:** `ingest-shared` (full replace or content-hash upsert).
3. **Test vs campaign:** rebuild the index that matches the play mode; if both modes are used on one machine, keep **separate** index directories (`…/shared-test`, `…/shared-campaign`).
4. **Faction indexes:** no mandatory wipe; re-ingest factions only if report templates or order syntax examples in drafts must change.
5. **Bump allowlist:** regenerate verb list from updated `rules.md` before the next draft batch.
6. **Smoke:** retrieve one changed verb / one new tech id; confirm chunks appear.

### Checklist (copy into PR / turn notes when catalog or engine docs change)

- [ ] Player manuals updated for the change set
- [ ] `ingest-shared` completed for affected mode(s)
- [ ] Verb allowlist regenerated
- [ ] Spot-check retrieve for new/changed headings
- [ ] (If mid-campaign) note in run README that RAG shared index was rebuilt at engine version X / catalog revision Y

**Done when:** A catalog or rules change is followed by a documented ingest + allowlist refresh before the next AI order batch.

---

## Phase 6 — Wire into campaign play (optional)

- [ ] Document optional replacement of Cursor `/player` calls with the local runner in campaign play docs when present (link only; keep Cursor path valid).
- [ ] Queue ten faction drafts against one Ollama (local or RunPod).
- [ ] Record isolation audit: no shared index contains another faction’s report.

**Done when:** One full AI turn batch (factions 2–11) can be drafted via the runner with isolation intact. If the batch used RunPod, Phases 7–8 must already be green.

---

## Phase 7 — RunPod usage tracker

Local ledger + optional API sync so the GM can see **what ran, how long, and roughly what it cost**. Does not replace the RunPod billing console.

### 7A. Session ledger (required)

- [ ] On every RunPod-backed session, append a record under `tools/player-agent/.data/usage/` (gitignored JSONL or SQLite), including at least:
  - `session_id`, `started_at`, `ended_at`, `duration_sec`
  - `pod_id`, `gpu_class` (e.g. RTX 4090), `cloud_tier` (Community / Secure)
  - `run_id` / faction list touched (ids only — no report bodies)
  - `chat_calls`, `embed_calls`, optional token/prompt size estimates if available
  - `hourly_rate_usd` (from config or last known listing), `estimated_cost_usd`
  - `stop_reason` (`user`, `guardrail`, `error`, `idle-timeout`)
- [ ] Commands: `usage start`, `usage stop`, `usage status`, `usage report [--since …] [--month …]`.
- [ ] Runner **auto-starts** a usage session when `OLLAMA_HOST` points at RunPod and **auto-stops** on successful batch end, Ctrl+C, or guardrail kill.
- [ ] Print a one-line cost summary after each batch (duration × rate).

### 7B. RunPod API sync (recommended)

- [ ] Optional `usage sync`: pull pod runtime / billing-adjacent facts via RunPod API (pod uptime, last exit) and reconcile with the local ledger (flag mismatches).
- [ ] Never store the API key in git; use env / local secrets file.
- [ ] If API sync is unavailable, ledger still works from local start/stop timestamps.

### 7C. Visibility

- [ ] `usage report` outputs human-readable totals for the current calendar month and the active campaign run.
- [ ] Document in README how to compare local estimates to the RunPod console.

**Done when:** A start→draft→stop cycle writes a ledger row with non-zero duration and estimated cost; `usage report` shows the session; API sync either works or is explicitly documented as deferred with local-only tracking still mandatory.

---

## Phase 8 — Cost and unintended-usage guardrails

Hard stops so a forgotten pod or a runaway loop cannot burn budget. **Fail closed** when using RunPod: if budget state is missing or caps are unset, refuse to start the pod / refuse chat calls.

### 8A. Budget and time caps

- [ ] Config (env or local file): `PLAYER_AGENT_BUDGET_USD` (soft + hard monthly or per-run), `PLAYER_AGENT_MAX_POD_HOURS` (per session and/or per calendar month), optional `PLAYER_AGENT_MAX_CHAT_CALLS` per session.
- [ ] Before starting a RunPod pod or issuing the first remote chat call, load ledger + caps; **abort** if projected or actual spend would exceed the hard cap.
- [ ] Soft cap: warn and require interactive confirmation (`PLAYER_AGENT_REQUIRE_CONFIRM=1` default for RunPod).
- [ ] Hard cap: exit non-zero; do not start or continue the pod.

### 8B. Idle and always-on protection

- [ ] **Idle timeout:** if no chat/embed call for N minutes (configurable, e.g. 10–15), automatically stop/terminate the pod and close the usage session (`stop_reason=idle-timeout`).
- [ ] **Max session wall clock:** hard kill at `PLAYER_AGENT_MAX_POD_HOURS` even if calls continue (forces deliberate restart + new confirmation).
- [ ] Startup check: if a tracked pod is still “up” from a previous crashed runner, refuse new work until `usage stop` / explicit `pod reclaim` (stop remote + close ledger).
- [ ] Prefer terminate-on-idle over leaving a stopped-but-billed storage surprise; document volume vs pod billing in README.

### 8C. Unintended usage controls

- [ ] **Explicit opt-in for RunPod:** remote host requires `--allow-runpod` (or env `PLAYER_AGENT_ALLOW_RUNPOD=1`) in addition to a non-localhost `OLLAMA_HOST`.
- [ ] **Confirm before start:** interactive “Start RTX 4090 at ~$X/hr? [y/N]” unless `--yes` is passed **and** soft cap not exceeded.
- [ ] **No background daemon** that auto-starts pods on repo open or on every git hook.
- [ ] Rate-limit: max concurrent Ollama requests = 1 (or low N) when remote; reject unbounded parallel ten-faction fan-out without a queue.
- [ ] Dry-run mode: `draft --dry-run` builds RAG + prompt pack but does not call chat and does not start a pod.
- [ ] Localhost Ollama path skips RunPod caps but may still log usage as `host=local` with `$0` for consistency.

### 8D. Alerts and failure behavior

- [ ] On guardrail trip: stop pod (best effort), write ledger `stop_reason=guardrail`, print clear next steps (`usage report`, raise budget, or switch to local).
- [ ] Optional: write a small `play/runs/<id>/gm/llm-usage.md` snippet (costs only, no secrets) after a campaign batch.
- [ ] Never disable guardrails via a hidden flag in committed scripts; any break-glass requires a local-only override file that is gitignored.

**Done when:** (1) unset budget refuses RunPod start; (2) idle timeout stops a live pod in a test; (3) exceeding max chat calls or hard USD cap aborts and stops the pod; (4) missing `--allow-runpod` cannot reach a remote host.

---

## Ops summary

| Event | Shared RAG | Per-faction RAG | Inference host | Usage / guardrails |
|-------|------------|-----------------|----------------|--------------------|
| First setup | Full `ingest-shared` | Per seat after isolate | Local or RunPod start | Set budget caps; enable tracker |
| New turn reports | No | Re-`ingest-faction` each seat | Keep or restart pod | Session continues; still under caps |
| New `story.md` | No | Re-ingest that seat’s story | — | — |
| Engine / `data.xml` / manual update | Rebuild shared (correct mode) + allowlist | Optional | — | Prefer local ingest (embeds); if RunPod, track embed time |
| Idle / cap hit | — | — | **Stop RunPod** | Ledger `idle-timeout` or `guardrail` |
| Idle between quarters | — | — | **Stop RunPod** | `usage report` for the quarter |

## Risks

| Risk | Mitigation |
|------|------------|
| Hallucinated verbs / ids | Allowlist lint; manuals as sole syntax source |
| Privacy on RunPod | Secure Cloud; strip passwords; excerpts; stop pod |
| Stale RAG after catalog change | Phase 5 checklist mandatory before AI drafts |
| Mixed test + campaign chunks | Separate shared indexes / `--mode test|campaign` |
| Context blow-up | Chunk reports; never feed galaxy XML |
| Forgotten always-on pod / cost overrun | Phase 7 ledger + Phase 8 idle timeout, hard USD/hour caps, opt-in + confirm |
| Unintended remote calls | `--allow-runpod`, fail-closed without budget, no auto-start daemons |
| Ledger vs RunPod console drift | `usage sync` + README note that console is authoritative for billing |

## Reading order for implementers

1. [ADR-0009](../adr/ADR-0009-local-llm-player-agent.md)  
2. This plan  
3. `.cursor/agents/campaign-ai.md` / campaign play isolation docs (when present)  
4. `.cursor/agents/player.md` (I/O contract to preserve)  
5. `player/README.md`
