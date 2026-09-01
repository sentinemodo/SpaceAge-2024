# Campaign Play Implementation Roadmap

**Last Updated:** 2026-08-30  
**Engine Version:** 0.1.157  
**Current Branch:** `cursor/campaign-load-play`  
**Status:** Ready for campaign play testing

---

## Overview

SpaceAge is ready to **launch campaign play** — AI isolation loop where 10 player factions (2–11) play with 13-week turns via `/campaign-ai` and `/player` agents. This document lists what has been **completed**, what **remains**, and the implementation order.

## ✅ What's DONE (As of 2026-08-30)

### Engine Core (`Game/` C#)

- [x] `JUMP` order: 1-week Alderson pair hop (ships only); `pair=` on `<alderson>` gates
- [x] Body environment: gravity, temperature, atmosphere loading and effects
- [x] Shuttle `h2o2` launch/land surcharge; frigate+ atmosphere land ban
- [x] Space MOVE duration from ΔAU × catalog drive speed (effective speed with mass factor)
- [x] System XYZ coordinates: load, save, report output
- [x] Hull groups: `corvette`, `destroyer`, `cruiser`, `capital`, `ark` (+ `IsShipHull` helper)
- [x] Typed combat: weapon-group vs resists matchup; armor (×5 hitWeight, no capture); shield (90% intercept)
- [x] Item equipment combat bonuses: attack, defense, damage from cargo items
- [x] Skill system: `usable-in` gates, combat/production/research bonuses, cure-chance healing
- [x] Sick-bay healing: weekly conversion (`wndtrn` → `terran`); quarterly wounded decay
- [x] Use-produce effects: `[repair]` module damage via `UseOrder` + `ProducingEffect`
- [x] RESEARCH flavour reveal: stars/planets/moons/belts descriptions when scanned

### Campaign XML & Catalog

- [x] `campaign/data.xml`: Complete L0–L10 technology tree, all module types, items, skills, races
- [x] `campaign/gamein.1.xml`: Turn 1 seed with 10 factions (2–11), NPC 1/12/13, Gates, inhabited starts
- [x] `campaign/_gen_gamein.py`: Python generator from `designer/galaxy.md`
- [x] Environment defaults: Arbor/Anvil/Selene/Scoria/Pyre planets/moons with gravity/temp/atmosphere
- [x] Combat balance catalog: weapon groups, resistances, armor/shield modules
- [x] Hull groups retagged: `corhul` → corvette, `deshul` → destroyer, `cruhul` → cruiser, `arkhul` → ark

### Campaign Infrastructure (`play/` Scripts)

- [x] `play/init-run.ps1`: Create run directory, patch passwords (2–11), write personas
- [x] `play/reports.ps1`: Execute `Game.exe /reports` (no Execute, no SaveGame)
- [x] `play/isolate.ps1`: Copy text reports to `factions/NN/` (isolation boundary)
- [x] `play/turn.ps1`: Full turn (UTF-8 → 1251, engine execute, isolate)
- [x] `play/next.ps1`: Advance game state (`gameout.N` → `gamein.xml`)
- [x] `play/_common.ps1`: Shared helpers (encoding, file paths)
- [x] `play/README.md`: Operations manual, script usage, encoding, isolation rules

### Agents & Coordination (`.cursor/`)

- [x] `/campaign-ai` agent: Play one faction; write story; call `/player`
- [x] `/campaign-gm` agent: Operate scripts; maintain `play/README.md`
- [x] Agent rulesets (`.mdc`): Activate on file patterns; hard boundaries

### Player Interface & Manuals

- [x] `player/rules.md`: Live order syntax (including `JUMP`, space MOVE ETA, hull groups)
- [x] `player/basic_technologies.md`: L0–1 techs from `Tests/data.xml` (SampleGame)
- [x] `player/advanced_technologies.md`: L2+ techs with `requires` graph
- [x] `player/battle.md`: Typed combat rules, tactics, diplomacy, equipment bonuses
- [x] `player/campaign/basic_technologies.md`: L0–1 excerpt from `campaign/data.xml` (campaign play)

### Testing

- [x] `Tests/SampleGame/` golden refresh: Engine 0.1.157 green (448/448 tests pass)
- [x] Campaign catalog load test: Verify `campaign/data.xml` and `campaign/gamein.1.xml` load
- [x] Campaign galaxy test: Systems, Gates, factions, HQs load correctly
- [x] JUMP order test: Pair-id hop, 1 week, ships only
- [x] Space MOVE duration tests: AU × drive speed calculations
- [x] Hull group tests: Parse, ToToken, `IsShipHull` helper
- [x] Typed combat tests: Weapon vs resist matchups, armor bias, shield intercept

### Documentation

- [x] `architecture/overview.md`: High-level architecture
- [x] `architecture/modules-and-integrations.md`: Bounded contexts
- [x] `architecture/technology.md`: Stack (C#, .NET 4.8, NUnit, Astro, Playwright)
- [x] `architecture/docs-index.md`: Library versions, external references
- [x] `architecture/delivery/campaign-play.md`: This plan (detailed targets and todos)
- [x] `designer/galaxy.md`: 10-system seed spec (Helios/Fomal, Gates, NPC, environments)
- [x] `designer/combat-balance.md`: Typed matchup table, armor/shield stats
- [x] `designer/environments.md`: Gravity/temp/atmosphere bands
- [x] `designer/catalog.md`: Module types, items, skills (reference)
- [x] `designer/contracts.md`: Contract vectors (injection triggers)
- [x] `designer/xml-schema.md`: XML tokens the engine accepts

### Git Preparation

- [x] Branch `cursor/campaign-load-play` pushed to origin
- [x] SampleGame golden refresh committed
- [x] Engine version updated to 0.1.157
- [x] Campaign files committed (not mixed with SampleGame golden changes)

---

## ⏳ What Remains (Next Steps to Launch)

### Immediate: Ready for First Campaign Run

**These are blockers for playability:**

#### 1. **Public Campaign Website** (Parallel to AI loop; Architect + Website-Developer)

| Task | Owner | Status | Notes |
|------|-------|--------|-------|
| Website architecture & stack | `/project-architect` | Pending | Read `architecture/delivery/website.md`, ADR-0007 first |
| Astro scaffold + home page | `/website-developer` | Pending | Excerpt Rules.txt (§§1–2.1) + credits (Atlantis, Rise of Heroes, Vincent Archer) |
| `/client` page (install, run) | `/website-developer` | Pending | Excerpt Rules.txt §1.1 |
| `/turns` page (faction roster, status) | `/website-developer` | Pending | Show factions 2–11, turn #, orders-submitted status; no passwords, no reports |
| `/rules` page (how a turn works) | `/website-developer` | Pending | Excerpt Rules.txt §2.1; hard-science only |
| Visual-tool link | `/website-developer` | Pending | Placeholder to external tool (not implemented here) |
| Vitest unit tests (schema, helpers) | `/website-developer` | Pending | Red → Green; no manual browser acceptance |
| Playwright acceptance (WS-001…WS-004) | `/website-tester` | Pending | Scenario catalog in `architecture/delivery/website-scenarios.md` |
| `astro check` + `vitest run` green | `/website-developer` | Pending | CI gates |
| `astro build` + `astro preview` + Playwright green | `/website-tester` | Pending | Chromium headless |

**Deliverable:** Static Astro site; open lobby Phase 1 pages.

#### 2. **AI Campaign Loop Walkthrough** (TDD + Campaign-GM)

| Task | Owner | Status | Notes |
|------|-------|--------|-------|
| Run `init-run.ps1 test1` | `/campaign-gm` | Pending | Creates `play/runs/test1/` with personas, passworded `gamein.xml` |
| Run `reports.ps1 test1` | `/campaign-gm` | Pending | Game.exe /reports → `turn/report.1.*.txt` |
| Run `isolate.ps1 test1` | `/campaign-gm` | Pending | Copy to `factions/02/`…`11/` |
| Launch `campaign-ai` faction 2 | `/campaign-ai` | Pending | Read report, persona, write story, call `/player` |
| Run `turn.ps1 test1` | `/campaign-gm` | Pending | Full turn (UTF-8 → 1251, engine, isolate) |
| Run `next.ps1 test1` | `/campaign-gm` | Pending | Advance turn; check win condition |
| Repeat 9 more times (factions 3–11) | `/campaign-ai` + `/campaign-gm` | Pending | Walkthrough Q1 of campaign |
| Verify turn 2 reports match story objectives | `/player` | Pending | Validate AI strategy ↔ outcome |

**Deliverable:** One full quarter (turn 1→2) completes successfully; AI agents play autonomously.

#### 3. **Between-Turn UN Contracts (Optional, Phase 1 closure)**

| Task | Owner | Status | Notes |
|------|-------|--------|-------|
| `play/no-turn.ps1` | TDD or GM | Pending | Wrap `/no-turn` line; stage `turn/order.1.txt` (UN); optional `next` |
| Mid-game contract injection | `/game-designer` | Pending | Write contract XML; patch run `gamein.xml`; `/player` drafts order |

**Deliverable:** GM can inject UN contracts mid-turn without advancing play.

---

### Later: Engine Wishlist (TDD, Any Order by Impact)

**These improve gameplay but are not blockers for launch:**

| Wishlist Item | Objective | Owner | Priority | Status |
|---------------|-----------|-------|----------|--------|
| **Skill children in battle** | Combat bonuses from officer skills (`usable-in="battle"`, attack/defense %) | `/csharp-tdd` | Medium | **LIVE 0.1.155** |
| **Sick-bay heal cadence** | Weekly conversion + quarterly decay | `/csharp-tdd` | Medium | **LIVE 0.1.150** |
| ~~Use-produce effects~~ | `REPAIR` via `ProducingEffect` | `/csharp-tdd` | Medium | **LIVE 0.1.151** |
| ~~Typed combat~~ | Weapon group vs resists; armor; shield | `/csharp-tdd` | High | **LIVE 0.1.155+** |
| ~~Hull groups~~ | Corvette, destroyer, cruiser, capital, ark | `/csharp-tdd` | High | **LIVE 0.1.149** |
| ~~Space MOVE from AU × drive~~ | Intra-system duration formula | `/csharp-tdd` | High | **LIVE 0.1.148** |
| Moon construction aliases | Unique moon ids (`M00001`) survive load | `/csharp-tdd` | Low | Pending |
| Moon orbits & exits | Second `loadGalaxyExits` pass for moons | `/csharp-tdd` | Low | Pending |
| Contract triggers: `survive-weeks`, `destroy-stack`, `region-resource-below` | Bombardment, fauna, pirate hunts | `/csharp-tdd` | Low | Pending |
| Events pipeline (timed spawns) | Alien reactivation, impacts | `/csharp-tdd` | Low | Pending |
| Item radiation / equipment in vacuum | Vests, suits, dosimeters | `/csharp-tdd` | Low | Pending |
| `atmosphere` location-type | Gas-giant cloud regions | `/csharp-tdd` | Low | Pending |
| Drive-dependent fuel system | `fustor` / `plsdv` / `arkeng` consumes `heliu3` | `/csharp-tdd` | Low | Pending |
| Gas-giant orbit resources | `deutrm`, `heliu3` on `gasgnt` orbits | `/csharp-tdd` | Low | Pending |
| `SEE` / scan bonus from `survsc` | Anomaly gameplay | `/csharp-tdd` | Low | Pending |

**Deliverable:** Richer gameplay; not required for first quarter.

---

## 🎯 Implementation Order (Recommended)

### Phase 1: Campaign Playability (3–7 days)

**Goal:** Run one full AI-driven quarter (turn 1→2) end-to-end.

1. **Website scaffolding** (in parallel with AI loop)
   - Architect reviews `website.md` + ADR-0007 (1 day)
   - Website-Developer scaffolds Astro project (1 day)
   - Website-Developer writes `/`, `/client`, `/turns`, `/rules` pages (2 days)
   - Website-Tester writes Playwright for Phase 1 scenarios (1 day)
   - `astro check` + `vitest run` + Playwright green (0.5 day)

2. **Campaign AI loop walkthrough** (3–5 days)
   - TDD runs `init-run.ps1` → `reports.ps1` → `isolate.ps1` → verify directory structure (0.5 day)
   - Campaign-GM invokes `/campaign-ai` faction 2; `/player` drafts order.2.txt (1 day)
   - Campaign-GM repeats for factions 3–11 (2 days)
   - Campaign-GM runs `turn.ps1` → validates turn 2 reports (1 day)
   - Campaign-GM checks win condition (0.5 day)
   - Iterate Q1 twice more if time (optional)

3. **Documentation & PR**
   - `/player` refreshes `player/rules.md`, `player/battle.md` if needed (0.5 day)
   - Archive this roadmap; update `campaign-play.md` with "status: playable" (0.5 day)
   - Create branch `cursor/campaign-launch` (or rename `cursor/campaign-load-play`)
   - `git push -u origin HEAD` (0 day)
   - `gh pr create` with test plan (0 day)

### Phase 2: Between-Turn Contracts & UI Polish (1–2 weeks)

**Goal:** Support mid-game UN intervention; polish website status JSON.

1. **`play/no-turn.ps1`** (1 day)
   - Wrap `/no-turn` line; stage UN orders; optional promote to next gamein

2. **Website status JSON** (1 day)
   - Define schema (factions 2–11, status enum, timestamps, no secrets)
   - Update `website/public/status.json` placeholder

3. **Contract injection** (2–3 days)
   - Designer writes mid-game contract XML
   - TDD verifies contract trigger execution
   - `/player` drafts counter-order
   - `/campaign-ai` reacts to mid-game event

### Phase 3: Wishlist Slice (2–4 weeks)

**Goal:** Richer gameplay (as time permits).

1. **Moon construction & exits** (1 week)
   - Second `loadGalaxyExits` pass
   - Unique moon ids (`M00001`) survive load
   - Unit tests for moon orbits

2. **Contract triggers** (1 week)
   - `survive-weeks` (bombardment)
   - `destroy-stack` (pirate hunt)
   - `region-resource-below` (fauna/anomaly)

3. **Events pipeline** (1 week)
   - Timed alien reactivation
   - Impact week scheduling

---

## 📋 Acceptance Criteria for "Campaign Play Ready"

Before declaring Phase 1 complete, verify:

### Engine & Data
- [ ] `Campaign/data.xml` and `campaign/gamein.1.xml` load without errors
- [ ] `Tests/SampleGame/` all 448 tests green
- [ ] `Game.exe /reports` generates turn 1 reports (no Execute)
- [ ] `Game.exe` full turn increments turn, generates turn 2 reports + gameout

### Scripts
- [ ] `init-run.ps1 <id>` creates run directory, patches passwords
- [ ] `reports.ps1 <id>` generates text reports (one per faction + NPC)
- [ ] `isolate.ps1 <id>` copies reports to faction folders only
- [ ] `turn.ps1 <id>` converts UTF-8 → 1251, runs engine, isolates
- [ ] `next.ps1 <id>` advances turn (gameout → gamein)

### AI Loop
- [ ] `/campaign-ai` reads persona + report; writes story
- [ ] `/campaign-ai` calls `/player` with tactical objective
- [ ] `/player` drafts UTF-8 order.{id}.txt
- [ ] `turn.ps1` converts to 1251, engine executes, reports match story objectives
- [ ] 10 factions (2–11) play autonomously; NPC 1/12/13 governed by contracts
- [ ] Win check identifies surviving faction(s) or ally bloc

### Website (Phase 1)
- [ ] Home page shows Alderson excerpt + credits
- [ ] `/client` page explains install & run
- [ ] `/turns` page shows factions 2–11, current turn, orders-submitted status
- [ ] `/rules` page shows how a turn works (excerpt from Rules.txt)
- [ ] Visual-tool link is present
- [ ] No passwords, `gamein.xml`, or report contents leaked
- [ ] `astro check` green
- [ ] `vitest run` green
- [ ] Playwright tests green (WS-001…WS-004)

### Artifacts & Documentation
- [ ] Branch `cursor/campaign-load-play` (or `cursor/campaign-launch`) pushed to origin
- [ ] PR created with title + test plan
- [ ] `play/README.md` reflects all scripts and gaps
- [ ] `campaign-play.md` marked "Complete" with status = playable
- [ ] This roadmap archived or updated with completion date

---

## 🚨 Known Gaps (Documented, Not Blockers)

### In `play/README.md`

| Gap | Workaround | Priority |
|-----|-----------|----------|
| `play/no-turn.ps1` | GM runs `/no-turn` line directly | P2 |
| NPC 12/13 raid orders | Not needed for Q1 (contracts only) | P3 |
| `website/public/status.json` producer | Placeholder JSON in repo; Phase 2 connects play loop | P2 |

### In `designer/engine-wishlist.md`

| Gap | Impact | When |
|-----|--------|------|
| Moon region→orbit exits | Campaign moons only; workaround: planet-adjacent dust belts | Phase 3 |
| Contract triggers (fauna, bombardment) | Flavor only; workaround: static contracts | Phase 3 |
| Events pipeline | Alien reactivation flavor; workaround: manual injection | Phase 3 |

### In `player/order_wishlist.md`

| Wishlist | Impact | Player Objective |
|----------|--------|-----------------|
| `TRANSFER ALL [DAMAGED]` | Partial workaround: `TRANSFER` with count | Captured gun recovery |
| `REPAIR` without precondition | Workaround: repair-capable parent absorbs stack first | Damaged stack restoration |

### In `player/technologies_wishlist.md`

| Wishlist | Impact | Workaround |
|----------|--------|-----------|
| Wire medical facility heal | Flavor only; `[sckbay]` still primary | No technical blocker |

---

## 🔄 Handoff Checklist

When transitioning from Phase 1 (campaign playable) to Phase 2 (polish):

1. **TDD** runs full test suite; confirms SampleGame + campaign catalog load tests pass
2. **Campaign-GM** walks through one full quarter (Q1, turn 1→2) with all 10 factions
3. **Website-Developer** confirms Astro site builds and serves staticially
4. **Website-Tester** confirms Playwright tests pass on `astro build` + `astro preview`
5. **Player-Agent** validates Q1 reports against Q1 stories (match expected beats)
6. **Architect** confirms no deviations from `architecture/delivery/website.md`
7. **Human review:** Approves PR, confirms acceptance criteria met
8. **Merge** to `main`; tag release (e.g., `v0.1.157-campaign-load-play`)

---

## 📚 Key References

- **Campaign Play Plan:** `architecture/delivery/campaign-play.md`
- **Website Architecture:** `architecture/delivery/website.md`, ADR-0007
- **Agent Definitions:** `.cursor/agents/campaign-ai.md`, `.cursor/agents/campaign-gm.md`, `.cursor/agents/website-developer.md`
- **Operations Manual:** `play/README.md`
- **Design Specs:** `designer/galaxy.md`, `designer/combat-balance.md`, `designer/environments.md`
- **Test Acceptance:** SampleGame green (448/448 tests), campaign catalog load, JUMP/MOVE/combat fixtures

---

## 🎬 Quick Start (For GM)

```powershell
# From repo root
cd C:\Users\akacz\Documents\Cursor2\GitHub\Repositories\SpaceAge-2024

# Build
nuget restore SpaceAge.sln
msbuild SpaceAge.sln /p:Configuration=Debug

# Initialize campaign run
.\play\init-run.ps1 test1

# Generate turn 1 reports (no Execute)
.\play\reports.ps1 test1

# Isolate reports to faction folders
.\play\isolate.ps1 test1

# For each faction 2-11, invoke campaign-ai via Cursor
# Each call reads persona + report, writes story, calls /player for orders

# Execute full turn 1→2
.\play\turn.ps1 test1

# Advance to next gamein
.\play\next.ps1 test1

# Repeat: isolate → AI → turn → next
```

---

## 📞 Questions?

See `.cursor/agents/` for agent responsibilities. See `play/README.md` for encoding and isolation rules. See `architecture/docs-index.md` for external references.

