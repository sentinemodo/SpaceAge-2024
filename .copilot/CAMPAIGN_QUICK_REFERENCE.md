# Campaign Play: Quick Reference Card

**Status:** Engine ready ✅ | Website in progress 📍 | AI walkthrough pending ⏳

---

## 🎯 What's Ready

### Engine Core ✅
- ✅ JUMP (1-week Alderson hops)
- ✅ Space MOVE (ΔAU × drive speed)
- ✅ Environment effects (gravity, temp, atmosphere)
- ✅ Typed combat (weapon groups, armor, shields)
- ✅ Hull groups (corvette…ark)
- ✅ Skills & item bonuses
- ✅ Healing & repair effects

### Campaign Data ✅
- ✅ `campaign/data.xml` (L0–L10 complete)
- ✅ `campaign/gamein.1.xml` (10 factions + NPC seeded)
- ✅ Python generator (`_gen_gamein.py`)

### Play Scripts ✅
- ✅ `init-run.ps1` — Create run
- ✅ `reports.ps1` — Generate reports (no Execute)
- ✅ `isolate.ps1` — Copy to faction folders
- ✅ `turn.ps1` — Full turn (UTF-8 → 1251, execute, isolate)
- ✅ `next.ps1` — Advance turn

### Agents ✅
- ✅ `/campaign-ai` — Play one faction
- ✅ `/campaign-gm` — Run scripts
- ✅ `/player` — Draft orders, validate reports

### Manuals ✅
- ✅ `player/rules.md` (order syntax)
- ✅ `player/basic_technologies.md` (L0–1)
- ✅ `player/advanced_technologies.md` (L2+)
- ✅ `player/campaign/basic_technologies.md` (L0–1 excerpt)
- ✅ `player/battle.md` (typed combat)

---

## 📍 In Progress

### Website Phase 1 (Est. 7 days)
- 📍 Astro scaffolding
- 📍 Home page (Alderson excerpt + credits)
- 📍 /client page (install)
- 📍 /turns page (faction status)
- 📍 /rules page (turn sequence)
- 📍 Vitest unit tests
- 📍 Playwright acceptance

### AI Walkthrough (Est. 5 days, after website scaffolding)
- ⏳ Run `init-run` → `reports` → `isolate`
- ⏳ Invoke `/campaign-ai` factions 2–11
- ⏳ Run `turn.ps1` → validate reports
- ⏳ Verify AI autonomous; check win condition

---

## ⏳ Next (Later)

### Phase 2 (Contracts)
- [ ] `play/no-turn.ps1` (between-turn wrapper)
- [ ] Mid-game contract injection
- [ ] Website `status.json` schema

### Phase 3 (Richness)
- [ ] Moon construction aliases & unique ids
- [ ] Moon region↔orbit exits
- [ ] Contract triggers (bombardment, fauna, pirate hunts)
- [ ] Events pipeline (alien reactivation)
- [ ] Gas-giant atmosphere & fuel consumption

---

## 🚀 Quick Start (For GM)

```powershell
# Build
nuget restore SpaceAge.sln
msbuild SpaceAge.sln /p:Configuration=Debug

# Create run
.\play\init-run.ps1 test1

# Generate reports
.\play\reports.ps1 test1

# Isolate to factions
.\play\isolate.ps1 test1

# For each faction 2-11:
#   Cursor: Invoke /campaign-ai (reads persona + report, writes story, calls /player)
#   /player: drafts order.{id}.txt

# Execute turn
.\play\turn.ps1 test1

# Advance turn
.\play\next.ps1 test1

# Check win
# (Manual: read reports; 1 faction left = solitary win; mutual allies = bloc win)
```

---

## 📋 Wishlists (Non-Blocking)

### Order Syntax (2 items, P3)
- `TRANSFER ALL [DAMAGED]` — Stack optimization
- `REPAIR` without precondition — Captured gun recovery

**Workarounds:** Manual count-and-transfer; merge stacks first

### Technology (1 item, P3)
- Wire medical facility heal — Designer choice; sickbay handles medical

**Workaround:** Sick-bay primary medical solution

### Engine (24 rows)
- **15 LIVE** ✅
- **5 PENDING** (moon names, moon orbits, events, fuel, atmosphere)
- **4 OUT-OF-SCOPE** (hostility-flip, multi-reward contracts, coastal, victory flag)

---

## 🔑 Key Files

| File | Role | Owner |
|------|------|-------|
| `campaign/data.xml` | Live catalog | Designer |
| `campaign/gamein.1.xml` | Turn 1 seed | Designer |
| `play/init-run.ps1` | Create run | GM (execute) |
| `play/reports.ps1` | Generate reports | GM (execute) |
| `play/isolate.ps1` | Isolate to factions | GM (execute) |
| `play/turn.ps1` | Full turn | GM (execute) |
| `play/next.ps1` | Advance turn | GM (execute) |
| `player/rules.md` | Order syntax | Player |
| `player/battle.md` | Combat rules | Player |
| `play/README.md` | Operations manual | GM (read) |
| `.cursor/agents/campaign-ai.md` | AI agent | Reference |
| `.cursor/agents/campaign-gm.md` | GM agent | Reference |
| `architecture/delivery/campaign-play.md` | Detailed plan | Reference |

---

## 🎓 Encoding Rules (Critical!)

| Content | Encoding | Where |
|---------|----------|-------|
| Engine XML (catalog, game state) | Windows-1251 | Disk (campaign/*, play/runs/*/data) |
| Turn reports | Windows-1251 | Disk (play/runs/*/turn, factions) |
| Player order drafts | UTF-8 | Repo (player/drafts, factions) |
| Player manuals | UTF-8 | Repo (player/*.md) |
| Personas & stories | UTF-8 | Repo (factions/NN/persona.md, story.md) |

**Important:** `turn.ps1` converts UTF-8 drafts → 1251 before engine execution. Do not double-convert.

---

## 🔌 Isolation Boundary

**Allowed in faction folder:**
- `persona.md` (id, password, preference, doctrine, win)
- `report.{T}.{id}.txt` (text report only)
- `order.{id}.txt` (current/draft orders)
- `story.md`, `story.{T}.md` (quarter plans)

**Never put in faction folder:**
- `gamein.xml`, `gameout.xml` (full game state)
- `report.*.xml` (leaks other cargo, techs)
- `campaign/data.xml` (catalog)
- Other faction folders
- NPC 1/12/13 files

---

## ✅ Launch Criteria

Campaign play launches when:

- [x] Engine green (all critical features live; SampleGame 448/448)
- [x] Campaign catalog loads (data.xml, gamein.1.xml)
- [x] Play scripts ready (init, reports, isolate, turn, next)
- [x] Agent definitions ready (campaign-ai, campaign-gm, /player)
- [ ] Website Phase 1 deployed (home, /client, /turns, /rules, astro build green, playwright green)
- [ ] One full AI quarter executes (turn 1→2) with 10 factions
- [ ] Reports match story objectives
- [ ] Win condition check works

**ETA:** 10 days (by Sept 9, 2026)

---

## 📞 Decision Points

| Decision | Option A | Option B | Impact |
|----------|----------|----------|--------|
| Website signup | Closed (10-player) | Open (anyone joins) | Multiplayer coordination complexity |
| Phase 2 priority | Contracts | Website polish | Gameplay depth vs UX |
| Phase 3 scope | Full wishlist | Selective (P2 only) | Development velocity vs feature completeness |

---

## 📚 Full Documentation

- **Codebase Review:** `CODEBASE_REVIEW.md`
- **Campaign Plan:** `CAMPAIGN_IMPLEMENTATION_ROADMAP.md`
- **Wishlist Details:** `WISHLIST_AND_GAPS_SUMMARY.md`
- **Launch Summary:** `CAMPAIGN_LAUNCH_SUMMARY.md`
- **Architecture:** `architecture/delivery/campaign-play.md`
- **Operations:** `play/README.md`

---

**Last Update:** 2026-08-30 | **Engine:** 0.1.157 | **Branch:** cursor/campaign-load-play

